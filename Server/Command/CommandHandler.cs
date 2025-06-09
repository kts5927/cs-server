using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class CommandHandler : ICommandHandler
{
    public static readonly CommandHandler Instance = new CommandHandler();

    private CommandHandler() { }

    public async Task HandleNick(Session session, Packet packet)
    {
        string args = packet.Payload.Trim();
        if (string.IsNullOrEmpty(args))
        {
            await session.Send("[시스템] 사용법: /nick [닉네임]");
            return;
        }

        if (session.Nickname != args && RoomManager.Instance.IsNicknameTaken(args))
        {
            await session.Send("[시스템] 이미 사용 중인 닉네임입니다.");
            return;
        }

        string old = session.Nickname;
        session.Nickname = args;
        await session.Send($"[닉네임 변경] {old} → {args}");
    }

    public async Task HandleAll(Session session, Packet packet)
    {
        string message = packet.Payload.Trim();
        if (string.IsNullOrEmpty(message))
        {
            await session.Send("[시스템] 사용법: /all [메시지]");
            return;
        }

        if (session.Room != null)
        {
            await session.Room.Broadcast(session, message);
        }
        else
        {
            await session.Send("[시스템] 방에 참여하고 있지 않습니다.");
        }
    }

    public async Task HandleWhisper(Session session, Packet packet)
    {
        string[] parts = packet.Payload.Split(' ', 2);
        if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
        {
            await session.Send("[시스템] 사용법: /whisper [닉네임] [내용]");
            return;
        }

        string targetName = parts[0];
        string message = parts[1];

        Session? target = RoomManager.Instance.FindByNickname(targetName);
        if (target == null)
        {
            await session.Send("[시스템] 대상 닉네임을 찾을 수 없습니다.");
            return;
        }

        await target.Send($"[귓속말 ← {session.Nickname}] {message}");
        await session.Send($"[귓속말 → {target.Nickname}] {message}");
    }

    public async Task HandleJoin(Session session, Packet packet)
    {
        if (!int.TryParse(packet.Payload.Trim(), out int roomId))
        {
            await session.Send("[시스템] 사용법: /join [방번호]");
            return;
        }

        RoomManager.Instance.MoveToRoom(session, roomId);
        await session.Send($"[시스템] {roomId}번 방에 입장하였습니다.");
    }

    public async Task HandleWho(Session session, Packet packet)
    {
        if (session.Room == null)
        {
            await session.Send("[시스템] 현재 방에 속해있지 않습니다.");
            return;
        }

        List<string> names = session.Room.GetNicknames();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[현재 방 인원]");
        foreach (string name in names)
            sb.AppendLine($"- {name}");

        await session.Send(sb.ToString());
    }

    public async Task HandleExit(Session session, Packet packet)
    {
        if (session.Room == null)
        {
            await session.Send("[시스템] 현재 아무 방에도 속해있지 않습니다.");
            return;
        }

        RoomManager.Instance.Leave(session);
        await session.Send("[시스템] 방에서 나갔습니다.");
    }

    public async Task HandleUnknown(Session session, Packet packet)
    {
        await session.Send("[시스템] 알 수 없는 명령입니다.");
    }
}
