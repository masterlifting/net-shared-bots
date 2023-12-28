using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotRequestService
{
    Task OnTextHandler(TextEventArgs text);
    Task OnAudioHandler(AudioEventArgs audio);
    Task OnContactHandler(ContactEventArgs contact);
    Task OnDocumentHandler(DocumentEventArgs document);
    Task OnLocationHandler(LocationEventArgs location);
    Task OnPhotoHandler(PhotoEventArgs photo);
    Task OnVideoHandler(VideoEventArgs video);
    Task OnVoiceHandler(VoiceEventArgs voice);
}
