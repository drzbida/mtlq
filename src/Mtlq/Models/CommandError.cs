using System.Text.Json.Serialization;

namespace Mtlq.Models;

public readonly record struct CommandError
{
    public required string Message { get; init; }
    public required string Details { get; init; }
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Serialization,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(CommandError))]
internal partial class ErrorJsonContext : JsonSerializerContext { }
