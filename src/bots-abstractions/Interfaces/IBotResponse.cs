using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotResponse
{
    Task Create(Message message, Command command, CancellationToken cToken);
}
