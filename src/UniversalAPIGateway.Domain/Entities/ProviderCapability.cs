namespace UniversalAPIGateway.Domain.Entities;

[Flags]
public enum ProviderCapability
{
    None = 0,
    TextGeneration = 1,
    Embeddings = 2,
    Moderation = 4,
    Translation = 8,
    SpeechToText = 16,
    TextToSpeech = 32
}
