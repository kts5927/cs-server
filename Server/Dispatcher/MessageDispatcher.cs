using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

public class MessageDispatcher : IMessageDispatcher
{
    public static MessageDispatcher Instance => (MessageDispatcher)MessageDispatcherHolder.Instance;

    private readonly Channel<(Session, Packet)> _channel;
    private readonly List<Task> _workers = new();
    private readonly int _cooldownMs;
    private bool _isStopped = false;

    private MessageDispatcher(int workerCount, int cooldownMs)
    {
        _cooldownMs = cooldownMs;
        _channel = Channel.CreateUnbounded<(Session, Packet)>(
            new UnboundedChannelOptions { SingleReader = false, SingleWriter = false }
        );

        for (int i = 0; i < workerCount; i++)
        {
            _workers.Add(Task.Run(ProcessQueueAsync));
        }
    }

    /// <summary>
    /// Program.cs에서 최초 1회 호출하여 싱글톤 인스턴스 생성
    /// </summary>
    public static void Initialize(int workerCount, int cooldownMs)
    {
        if (MessageDispatcherHolder.Instance == null)
        {
            MessageDispatcherHolder.Instance = new MessageDispatcher(workerCount, cooldownMs);
        }
    }

    public async Task Enqueue(Session session, Packet packet)
    {
        if (_isStopped)
            return;

        await _channel.Writer.WriteAsync((session, packet));
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (var (session, packet) in _channel.Reader.ReadAllAsync())
        {
            try
            {
                // 쿨다운 검사
                if (!session.TryUpdateChatTime(_cooldownMs))
                {
                    await session.Send("[시스템] 채팅 쿨다운 중입니다.");
                    continue;
                }

                // 명령어 처리: HandleAll 내부에서 바로 Broadcast
                await CommandRegistry.Execute(session, packet);
            }
            catch (Exception ex)
            {
                Log.Error("Dispatcher 예외: {Error}", ex);
            }
        }
    }

    public async Task StopAsync()
    {
        _isStopped = true;
        _channel.Writer.Complete();
        try
        {
            await Task.WhenAll(_workers);
        }
        catch
        { /* 무시 */
        }
    }
}
