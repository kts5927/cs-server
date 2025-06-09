public static class CommandTable
{
    public static void RegisterAll()
    {
        CommandRegistry.Register("/nick", CommandHandler.Instance.HandleNick);
        CommandRegistry.Register("/all", CommandHandler.Instance.HandleAll);
        CommandRegistry.Register("/whisper", CommandHandler.Instance.HandleWhisper);
        CommandRegistry.Register("/join", CommandHandler.Instance.HandleJoin);
        CommandRegistry.Register("/who", CommandHandler.Instance.HandleWho);
        CommandRegistry.Register("/exit", CommandHandler.Instance.HandleExit);
    }
}
