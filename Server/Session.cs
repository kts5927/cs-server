using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

public class Session
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly byte[] _buffer = new byte[1024];
    private readonly object _cooldownLock = new();
    private DateTime _lastChatTime = DateTime.MinValue;

    public DateTime LastChatTime
    {
        get
        {
            lock (_cooldownLock)
            {
                return _lastChatTime;
            }
        }
        private set
        {
            lock (_cooldownLock)
            {
                _lastChatTime = value;
            }
        }
    }

    public string Nickname { get; set; } = "익명";
    public Room? Room { get; set; }

    public Session(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _client.ReceiveTimeout = 30000; // 30초간 유휴 시 예외 발생
        _client.SendTimeout = 30000;
        // TCP Keep-Alive 설정 (윈도우 전용 옵션 예시)
        try
        {
            _client.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.KeepAlive,
                true
            );
        }
        catch
        { /* 무시 */
        }
    }

    /// <summary>
    /// 1) 클라이언트 메시지 수신
    /// 2) packet 파싱 → MessageDispatcher.Enqueue
    /// </summary>
    public async Task HandleAsync()
    {
        try
        {
            while (true)
            {
                int read;
                try
                {
                    read = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
                }
                catch (Exception ex)
                {
                    Log.Warning("세션[{Nick}] 수신 오류: {Error}", Nickname, ex.Message);
                    break;
                }

                if (read == 0) // 연결 종료
                    break;

                string msg = Encoding.UTF8.GetString(_buffer, 0, read).Trim();
                Log.Debug("세션[{Nick}] 수신 메시지: {Msg}", Nickname, msg);

                Packet packet;
                try
                {
                    packet = Packet.Parse(msg);
                }
                catch
                {
                    await Send("[시스템] 잘못된 패킷 형식입니다.");
                    continue;
                }

                // 큐에 넣기 (쿨다운, 명령어 처리는 디스패처가 담당)
                await MessageDispatcher.Instance.Enqueue(this, packet);
            }
        }
        finally
        {
            Log.Information("세션[{Nick}] 연결 종료", Nickname);
            RoomManager.Instance.Leave(this); // 방에서 제거
            _stream.Close();
            _client.Close();
        }
    }

    /// <summary>
    /// /// 서버→클라이언트 메시지 전송 (비동기)
    /// </summary>
    public async Task Send(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
        try
        {
            await _stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Log.Warning("세션[{Nick}] 전송 오류: {Error}", Nickname, ex.Message);
        }
    }

    /// <summary>
    /// 쿨다운 판단 후, 통과 시 내부 타임스탬프 갱신
    /// </summary>
    public bool TryUpdateChatTime(int cooldownMs)
    {
        lock (_cooldownLock)
        {
            var now = DateTime.UtcNow;
            if ((now - _lastChatTime).TotalMilliseconds < cooldownMs)
                return false;
            _lastChatTime = now;
            return true;
        }
    }
}
