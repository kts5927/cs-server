using System.Collections.Generic;
using System.Threading.Tasks;

public class Room
{
    private readonly List<Session> _members = new();
    private readonly object _membersLock = new();

    public int RoomId { get; }

    public Room(int roomId)
    {
        RoomId = roomId;
    }

    public void Enter(Session session)
    {
        lock (_membersLock)
        {
            _members.Add(session);
        }
        session.Room = this;
    }

    public void Leave(Session session)
    {
        lock (_membersLock)
        {
            _members.Remove(session);
        }
        session.Room = null;
    }

    public int MemberCount
    {
        get
        {
            lock (_membersLock)
            {
                return _members.Count;
            }
        }
    }

    public async Task Broadcast(Session sender, string message)
    {
        List<Task> tasks = new();
        lock (_membersLock)
        {
            foreach (var s in _members)
            {
                if (s != sender)
                {
                    tasks.Add(s.Send($"{sender.Nickname}: {message}"));
                }
            }
        }
        await Task.WhenAll(tasks);
    }

    public bool ContainsNickname(string nickname)
    {
        lock (_membersLock)
        {
            return _members.Exists(s => s.Nickname == nickname);
        }
    }

    public Session? FindByNickname(string nickname)
    {
        lock (_membersLock)
        {
            return _members.Find(s => s.Nickname == nickname);
        }
    }

    public List<string> GetNicknames()
    {
        lock (_membersLock)
        {
            List<string> names = new();
            foreach (var s in _members)
            {
                names.Add(s.Nickname);
            }
            return names;
        }
    }
}
