namespace Net.Shared.Bots.Abstractions.Models;

public sealed record Text(string Value);
public sealed record Photo(string FileId, long? FileSize);
public sealed record Audio(string FileId, long? FileSize, string? Title, string? MimeType);
public sealed record Video(string FileId, long? FileSize, string? FileName, string? MimeType);
public sealed record Voice(string FileId, long? FileSize, string? MimeType);
public sealed record Document(string FileId, long? FileSize, string? FileName, string? MimeType);
public sealed record Location(double Latitude, double Longitude);
public sealed record Contact(string PhoneNumber, string FirstName, string? LastName);
