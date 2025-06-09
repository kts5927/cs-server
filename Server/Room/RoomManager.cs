using System.Collections.Concurrent;
using System.Collections.Generic;
using Serilog;

public class RoomManager : IRoomManager
{
    private static readonly RoomManager _instance = new();
    public static RoomManager Instance => _instance;

    private readonly object _roomsLock = new();
    private readonly Dictionary<int, Room> _rooms = new();

    private RoomManager() { }

    public void MoveToRoom(Session session, int roomId)
    {
        Room? prev = session.Room;
        if (prev != null)
        {
            Leave(session);
        }

        Room? room;
        lock (_roomsLock)
        {
            if (!_rooms.TryGetValue(roomId, out room))
            {
                room = new Room(roomId);
                _rooms[roomId] = room;
                Log.Information("새 방 생성: {RoomId}", roomId);
            }
        }

        room!.Enter(session);
        Log.Information("세션[{Nick}] → 방[{RoomId}] 입장", session.Nickname, roomId);
    }

    public void Leave(Session session)
    {
        var curr = session.Room;
        if (curr == null)
            return;

        curr.Leave(session);
        Log.Information("세션[{Nick}] 방[{RoomId}] 퇴장", session.Nickname, curr.RoomId);

        lock (_roomsLock)
        {
            if (curr.MemberCount == 0)
            {
                _rooms.Remove(curr.RoomId);
                Log.Information("빈 방 삭제: {RoomId}", curr.RoomId);
            }
        }
    }

    public void RemoveRoom(Room room)
    {
        lock (_roomsLock)
        {
            if (_rooms.Remove(room.RoomId))
                Log.Information("방 제거: {RoomId}", room.RoomId);
        }
    }

    public bool IsNicknameTaken(string nickname)
    {
        lock (_roomsLock)
        {
            foreach (var room in _rooms.Values)
            {
                if (room.ContainsNickname(nickname))
                    return true;
            }
        }
        return false;
    }

    public Session? FindByNickname(string nickname)
    {
        lock (_roomsLock)
        {
            foreach (var room in _rooms.Values)
            {
                var s = room.FindByNickname(nickname);
                if (s != null)
                    return s;
            }
        }
        return null;
    }
}
