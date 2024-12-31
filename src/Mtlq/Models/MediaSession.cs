using System.Text.Json.Serialization;

namespace Mtlq.Models;

public readonly record struct MediaSession
{
    public required string Source { get; init; }
    public required string Title { get; init; }
    public required string Artist { get; init; }
    public required string CurrentTime { get; init; }
    public required string TotalTime { get; init; }
    public required PlaybackStatus Status { get; init; }
}

public enum PlaybackStatus
{
    Unknown,
    Playing,
    Paused,
    Stopped
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Serialization,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(MediaSession[]))]
internal partial class MediaJsonContext : JsonSerializerContext
{
}
