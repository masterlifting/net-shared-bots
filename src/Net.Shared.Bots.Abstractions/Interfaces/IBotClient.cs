using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotClient
{
    Task Listen(Uri uri, CancellationToken cToken);
    Task Listen(CancellationToken cToken);
    Task Receive(string data, CancellationToken cToken);

    Task<byte[]> LoadFile(string fileId, CancellationToken cToken);
    
    Task SendButtons(ButtonsEventArgs args, CancellationToken cToken);
    Task SendWebApp(WebAppEventArgs args, CancellationToken cToken);
    Task SendMessage(MessageEventArgs args, CancellationToken cToken);
}
