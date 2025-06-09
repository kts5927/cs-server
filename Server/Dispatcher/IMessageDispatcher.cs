using System.Threading.Tasks;

public interface IMessageDispatcher
{
    Task Enqueue(Session session, Packet packet);
    Task StopAsync();
}

public static class MessageDispatcherHolder
{
    // 전역에서 접근 가능한 단일 인스턴스
    public static IMessageDispatcher Instance { get; set; } = null!;
}
