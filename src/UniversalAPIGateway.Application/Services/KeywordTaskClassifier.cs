using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Domain.Entities;

namespace UniversalAPIGateway.Application.Services;

public sealed class KeywordTaskClassifier : ITaskClassifier
{
    public TaskType Classify(GatewayRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var payload = request.Payload.ToLowerInvariant();

        if (ContainsAny(payload, "transcribe", "audio", "speech", "wav", "mp3"))
        {
            return TaskType.AudioTranscription;
        }

        if (ContainsAny(payload, "image", "draw", "illustration", "logo", "render"))
        {
            return TaskType.ImageGeneration;
        }

        if (ContainsAny(payload, "summarize", "summary", "tl;dr", "condense"))
        {
            return TaskType.Summarization;
        }

        if (ContainsAny(payload, "code", "debug", "refactor", "c#", "python", "function", "compile"))
        {
            return TaskType.Coding;
        }

        if (ContainsAny(payload, "story", "poem", "creative", "brainstorm"))
        {
            return TaskType.Creative;
        }

        return TaskType.Chat;
    }

    private static bool ContainsAny(string source, params string[] needles) =>
        needles.Any(source.Contains);
}
