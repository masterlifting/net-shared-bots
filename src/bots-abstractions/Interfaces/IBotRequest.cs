using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Request;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotRequest
{
    Task HandleText(TextEventArgs text, CancellationToken cToken);
    Task HandleAudio(AudioEventArgs audio, CancellationToken cToken);
    Task HandleContact(ContactEventArgs contact, CancellationToken cToken);
    Task HandleDocument(DocumentEventArgs document, CancellationToken cToken);
    Task HandleLocation(LocationEventArgs location, CancellationToken cToken);
    Task HandlePhoto(PhotoEventArgs photo, CancellationToken cToken);
    Task HandleVideo(VideoEventArgs video, CancellationToken cToken);
    Task HandleVoice(VoiceEventArgs voice, CancellationToken cToken);
}
