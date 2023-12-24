
using System.Collections.Immutable;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotService
{
    Task HandleText(string chatId, string text);
    Task HandleAudio(string chatId, string fileId, long? fileSize, string? mimeType, string? title);
    Task HandleContact(string chatId, string phoneNumber, string firstName, string? lastName);
    Task HandleDocument(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName);
    Task HandleLocation(string chatId, double latitude, double longitude);
    Task HandlePhoto(string chatId, ImmutableArray<(string FileId, long? FileSize)> immutableArray);
    Task HandleVideo(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName);
    Task HandleVoice(string chatId, string fileId, long? fileSize, string? mimeType);
}
