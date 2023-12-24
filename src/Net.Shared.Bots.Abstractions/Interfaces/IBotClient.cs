namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotClient
{
    Task Listen(Uri uri, CancellationToken cToken);
    Task Listen(CancellationToken cToken);
    Task Send(IBotMessage message, CancellationToken cToken);
    Task Receive(string data, CancellationToken cToken);
    Task<byte[]> LoadFile(string fileId, CancellationToken cToken);
}
