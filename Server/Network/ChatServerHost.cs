using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class ChatServerHost
{
    private readonly IConfiguration _config;
    private readonly int _port;
    private readonly int _workerCount;
    private readonly int _cooldownMs;
    private TcpListener _listener;

    public ChatServerHost()
    {
        // 1) 설정 로드
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serverSection = _config.GetSection("Server");
        _port = serverSection.GetValue<int>("Port");
        _workerCount = serverSection.GetValue<int>("WorkerCount");
        _cooldownMs = serverSection.GetValue<int>("CooldownMs");

        // 2) 커맨드 등록 및 디스패처 초기화
        CommandTable.RegisterAll();
        MessageDispatcher.Initialize(_workerCount, _cooldownMs);

        // 3) TcpListener 준비
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    public async Task RunAsync()
    {
        Console.WriteLine(
            $"서버 시작 (Port: {_port}, Workers: {_workerCount}, Cooldown: {_cooldownMs}ms)"
        );

        _listener.Start();
        Console.WriteLine($"TCP Listener 시작 → {IPAddress.Any}:{_port}");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("종료 요청 감지, 서버 종료 중...");
            cts.Cancel();
            _listener.Stop();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"클라이언트 연결 수락 실패: {ex.Message}");
                    continue;
                }

                Console.WriteLine($"클라이언트 접속: {client.Client.RemoteEndPoint}");
                var session = new Session(client);
                RoomManager.Instance.MoveToRoom(session, 0);
                _ = session.HandleAsync();
            }
        }
        finally
        {
            await MessageDispatcher.Instance.StopAsync();
            Console.WriteLine("MessageDispatcher 종료 완료");
            Console.WriteLine("서버 종료");
        }
    }
}
