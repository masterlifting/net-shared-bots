
using System.Collections.Immutable;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotRequestService
{
    Task HandleText(string chatId, string text, CancellationToken cToken);
    Task HandleAudio(string chatId, string fileId, long? fileSize, string? mimeType, string? title, CancellationToken cToken);
    Task HandleContact(string chatId, string phoneNumber, string firstName, string? lastName, CancellationToken cToken);
    Task HandleDocument(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName, CancellationToken cToken);
    Task HandleLocation(string chatId, double latitude, double longitude, CancellationToken cToken);
    Task HandlePhoto(string chatId, ImmutableArray<(string FileId, long? FileSize)> photos, CancellationToken cToken);
    Task HandleVideo(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName, CancellationToken cToken);
    Task HandleVoice(string chatId, string fileId, long? fileSize, string? mimeType, CancellationToken cToken);
}
