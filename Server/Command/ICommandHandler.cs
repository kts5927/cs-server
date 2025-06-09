using System.Threading.Tasks;

public interface ICommandHandler
{
    Task HandleNick(Session session, Packet packet);
    Task HandleAll(Session session, Packet packet);
    Task HandleWhisper(Session session, Packet packet);
    Task HandleJoin(Session session, Packet packet);
    Task HandleWho(Session session, Packet packet);
    Task HandleExit(Session session, Packet packet);
    Task HandleUnknown(Session session, Packet packet);
}
