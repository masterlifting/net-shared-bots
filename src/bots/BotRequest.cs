using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots;

internal sealed class BotRequest(
    IBotCommandsStore commandsStore,
    IBotResponse response) : IBotRequest
{
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly IBotResponse _response = response;

    public async Task HandleText(TextEventArgs args, CancellationToken cToken)
    {
        if (args.Text.Value.StartsWith('/'))
        {
            var commandName = args.Text.Value.TrimStart('/');
            var command = await _commandsStore.Create(args.ChatId, commandName, [], cToken);
            await _response.Create(args.ChatId, command, cToken);
        }
        else if (Guid.TryParse(args.Text.Value, out var guid))
        {
            var command = await _commandsStore.Get(args.ChatId, guid, cToken);
            await _response.Create(args.ChatId, command, cToken);
        }
        else
            throw new NotSupportedException($"The message '{args.Text.Value}' is not supported.");
    }
    public Task HandlePhoto(PhotoEventArgs photo, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleAudio(AudioEventArgs audio, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleVideo(VideoEventArgs video, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleVoice(VoiceEventArgs voice, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleDocument(DocumentEventArgs document, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleLocation(LocationEventArgs location, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleContact(ContactEventArgs contact, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
