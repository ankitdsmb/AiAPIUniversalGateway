using System.ComponentModel.DataAnnotations;

namespace UniversalAPIGateway.Api.Contracts;

public sealed record ExecuteAiRequest(
    [property: Required]
    string? ProviderKey,
    [property: Required]
    string? Payload);
