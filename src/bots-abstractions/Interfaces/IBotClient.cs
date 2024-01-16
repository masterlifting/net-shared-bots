using Net.Shared.Bots.Abstractions.Models.Response;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotClient
{
    string AdminId { get; }

    Task Listen(Uri uri, CancellationToken cToken);
    Task Listen(CancellationToken cToken);
    Task Receive(string data, CancellationToken cToken);

    Task<byte[]> LoadFile(string fileId, CancellationToken cToken);
    
    Task SendButtons(ButtonsEventArgs args, CancellationToken cToken);
    Task SendWebApp(WebAppEventArgs args, CancellationToken cToken);
    Task SendMessage(MessageEventArgs args, CancellationToken cToken);
    Task SendMessage(string chatId, Message message, CancellationToken cToken);
    Task SendButtons(string chatId, Buttons buttons, CancellationToken cToken);
}
