using UniversalAPIGateway.Application.Abstractions;

namespace UniversalAPIGateway.Application.Services;

public sealed class ConsensusEvaluationService : IConsensusEvaluationService
{
    private static readonly string[] RequiredRoles =
    [
        "BUILDER_AI",
        "ARCHITECT_AI",
        "REVIEWER_AI",
        "QA_AI",
        "PERFORMANCE_AI",
        "PRINCIPAL_ARCHITECT_AI"
    ];

    public ConsensusEvaluationResult Evaluate(IReadOnlyCollection<RoleAssessment> assessments)
    {
        ArgumentNullException.ThrowIfNull(assessments);

        var indexed = assessments
            .GroupBy(static a => a.Role, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.Last(), StringComparer.OrdinalIgnoreCase);

        var missingRoles = RequiredRoles.Where(role => !indexed.ContainsKey(role)).ToArray();
        if (missingRoles.Length > 0)
        {
            throw new InvalidOperationException($"Consensus requires all roles. Missing: {string.Join(", ", missingRoles)}");
        }

        var disagreements = indexed.Values
            .Where(static x => !x.Approved)
            .Select(static x => $"{x.Role} reported blocking issues")
            .ToArray();

        var status = disagreements.Length == 0 ? "APPROVED" : "REJECTED";
        var safeToContinue = disagreements.Length == 0;

        var architectureScore = ResolveRoleScore(indexed, "ARCHITECT_AI");
        var codeQualityScore = ResolveRoleScore(indexed, "REVIEWER_AI");
        var qaStabilityScore = ResolveRoleScore(indexed, "QA_AI");
        var performanceScore = ResolveRoleScore(indexed, "PERFORMANCE_AI");

        var confidence = checked((int)Math.Round(indexed.Values.Average(static x => x.Score), MidpointRounding.AwayFromZero));

        var improvements = indexed.Values
            .SelectMany(static x => x.Findings)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var resolutions = disagreements.Length == 0
            ? new[] { "No blocking disagreements remained after principal architect arbitration" }
            : new[] { "Return to BUILDER_AI for mandatory refactor until all roles approve" };

        return new ConsensusEvaluationResult(
            status,
            safeToContinue,
            architectureScore,
            codeQualityScore,
            qaStabilityScore,
            performanceScore,
            Math.Clamp(confidence, 0, 100),
            disagreements,
            resolutions,
            improvements);
    }

    private static int ResolveRoleScore(IReadOnlyDictionary<string, RoleAssessment> assessments, string role)
    {
        if (!assessments.TryGetValue(role, out var assessment))
        {
            return 0;
        }

        return Math.Clamp(assessment.Score, 0, 100);
    }
}
