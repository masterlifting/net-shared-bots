namespace Net.Shared.Bots.Abstractions;

public static class Constants
{
    public static class Commands
    {
        public const string Start = "start";
        public const string Ask = "ask";
        public const string Answer = "answer";
    }

    public static class CommandParameters
    {
        public const string ChatId = "chatId";
        public const string Message = "message";
    }

    public enum ResponseMessageBehavior
    {
        New,
        Replace,
        Reply
    }

    public enum ResponseButtonsColumns
    {
        Auto,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4
    }
}
