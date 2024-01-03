using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotRequestService
{
    Task OnTextHandler(TextEventArgs text, CancellationToken cToken);
    Task OnAudioHandler(AudioEventArgs audio, CancellationToken cToken);
    Task OnContactHandler(ContactEventArgs contact, CancellationToken cToken);
    Task OnDocumentHandler(DocumentEventArgs document, CancellationToken cToken);
    Task OnLocationHandler(LocationEventArgs location, CancellationToken cToken);
    Task OnPhotoHandler(PhotoEventArgs photo, CancellationToken cToken);
    Task OnVideoHandler(VideoEventArgs video, CancellationToken cToken);
    Task OnVoiceHandler(VoiceEventArgs voice, CancellationToken cToken);
}
