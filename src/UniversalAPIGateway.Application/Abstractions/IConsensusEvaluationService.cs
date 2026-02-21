namespace UniversalAPIGateway.Application.Abstractions;

public interface IConsensusEvaluationService
{
    ConsensusEvaluationResult Evaluate(IReadOnlyCollection<RoleAssessment> assessments);
}

public sealed record RoleAssessment(string Role, bool Approved, int Score, IReadOnlyCollection<string> Findings);

public sealed record ConsensusEvaluationResult(
    string ConsensusStatus,
    bool SafeToContinue,
    int ArchitectureScore,
    int CodeQualityScore,
    int QaStabilityScore,
    int PerformanceScore,
    int ConsensusConfidence,
    IReadOnlyCollection<string> Disagreements,
    IReadOnlyCollection<string> Resolutions,
    IReadOnlyCollection<string> ImprovementsApplied);
