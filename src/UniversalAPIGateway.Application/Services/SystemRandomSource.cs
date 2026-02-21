using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Application.Services;

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random random = Random.Shared;

    public double NextDouble() => random.NextDouble();

    public int NextInt(int maxExclusive) => random.Next(maxExclusive);
}
