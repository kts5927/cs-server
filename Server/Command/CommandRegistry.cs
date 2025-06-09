using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class CommandRegistry
{
    private static readonly Dictionary<string, Func<Session, Packet, Task>> _commands = new();

    public static void Register(string command, Func<Session, Packet, Task> func)
    {
        _commands[command] = func;
    }

    public static async Task Execute(Session session, Packet packet)
    {
        if (_commands.TryGetValue(packet.Type, out var func))
        {
            await func(session, packet);
        }
        else
        {
            await CommandHandler.Instance.HandleUnknown(session, packet);
        }
    }
}
