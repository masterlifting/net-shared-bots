
namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotClient
{
    Task Listen(Uri uri, CancellationToken cToken);
    Task Listen(CancellationToken cToken);
    Task Send(IBotMessage message, CancellationToken cToken);
    Task Receive(string data, CancellationToken cToken);
    Task<byte[]> LoadFile(string fileId, CancellationToken cToken);
    Task SendButtons(string chatId, Dictionary<string, string> clientButtons, CancellationToken cToken);
    Task SendWebForm(string chatId, object data, CancellationToken cToken);
    Task SendText(string chatId, string v, CancellationToken cToken);
}
