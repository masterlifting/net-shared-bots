using Net.Shared.Abstractions.Models.Exceptions;

namespace Net.Shared.Bots.Abstractions.Models.Exceptions;

public class BotUserInvalidOperationException(string error) : UserInvalidOperationException(error)
{
}
