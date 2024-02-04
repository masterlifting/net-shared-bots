using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Response;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotClient
{
    string AdminId { get; }

    Task Listen(Uri uri, CancellationToken cToken);
    Task Listen(CancellationToken cToken);
    Task Receive(string data, CancellationToken cToken);

    Task<byte[]> LoadFile(string fileId, CancellationToken cToken);
    
    Task DeleteMessage(Message message, CancellationToken cToken);
    Task<Result> SendText(TextEventArgs args, CancellationToken cToken);
    Task<Result> SendButtons(ButtonsEventArgs args, CancellationToken cToken);
    Task<Result> SendWebApp(WebAppEventArgs args, CancellationToken cToken);
}
