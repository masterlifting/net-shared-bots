namespace Net.Shared.Bots.Abstractions;

public static class Constants
{
    public enum BotMessageType
    {
        Text,
        Photo,
        Audio,
        Video,
        Document,
        Location,
        Contact,
        Voice,
        Sticker
    }

    public static class Telegram
    {
        public enum ButtonStyle
        {
            VerticallyStrict,
            VerticallyFlex,
            Horizontally
        }
    }
}
