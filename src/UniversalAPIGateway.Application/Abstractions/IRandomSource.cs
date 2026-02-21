namespace UniversalAPIGateway.Application.Abstractions;

public interface IRandomSource
{
    double NextDouble();

    int NextInt(int maxExclusive);
}
