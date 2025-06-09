public interface IRoomManager
{
    void MoveToRoom(Session session, int roomId);
    void Leave(Session session);
    bool IsNicknameTaken(string nickname);
    Session? FindByNickname(string nickname);
    void RemoveRoom(Room room);
}
