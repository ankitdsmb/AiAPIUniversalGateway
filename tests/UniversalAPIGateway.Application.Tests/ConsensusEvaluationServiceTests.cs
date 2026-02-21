using UniversalAPIGateway.Application.Abstractions;
using UniversalAPIGateway.Application.Services;

namespace UniversalAPIGateway.Application.Tests;

public sealed class ConsensusEvaluationServiceTests
{
    private readonly ConsensusEvaluationService sut = new();

    [Fact]
    public void Evaluate_WhenAllRolesApprove_ReturnsApprovedConsensus()
    {
        var assessments = CreateAssessments(
            ("BUILDER_AI", true, 95, ["Implemented resilient provider adapter"]),
            ("ARCHITECT_AI", true, 97, ["No dependency direction violations"]),
            ("REVIEWER_AI", true, 93, ["Improved naming consistency"]),
            ("QA_AI", true, 92, ["Validated timeout and quota scenarios"]),
            ("PERFORMANCE_AI", true, 90, ["No blocking calls detected"]),
            ("PRINCIPAL_ARCHITECT_AI", true, 96, ["Approved long-term safe design"]));

        var result = sut.Evaluate(assessments);

        Assert.Equal("APPROVED", result.ConsensusStatus);
        Assert.True(result.SafeToContinue);
        Assert.Equal(97, result.ArchitectureScore);
        Assert.Equal(93, result.CodeQualityScore);
        Assert.Equal(92, result.QaStabilityScore);
        Assert.Equal(90, result.PerformanceScore);
        Assert.Equal(94, result.ConsensusConfidence);
        Assert.Empty(result.Disagreements);
        Assert.Contains("No blocking disagreements remained", result.Resolutions.Single());
        Assert.Equal(6, result.ImprovementsApplied.Count);
    }

    [Fact]
    public void Evaluate_WhenAnyRoleRejects_ReturnsRejectedConsensus()
    {
        var assessments = CreateAssessments(
            ("BUILDER_AI", true, 90, ["Implemented routing improvements"]),
            ("ARCHITECT_AI", false, 70, ["Detected boundary leak from application to infrastructure"]),
            ("REVIEWER_AI", true, 91, ["Applied SOLID refactors"]),
            ("QA_AI", false, 60, ["Timeout fallback scenario failed"]),
            ("PERFORMANCE_AI", true, 88, ["Reduced retries"]),
            ("PRINCIPAL_ARCHITECT_AI", false, 65, ["Requested mandatory rework for safety"]));

        var result = sut.Evaluate(assessments);

        Assert.Equal("REJECTED", result.ConsensusStatus);
        Assert.False(result.SafeToContinue);
        Assert.Equal(3, result.Disagreements.Count);
        Assert.Contains("ARCHITECT_AI reported blocking issues", result.Disagreements);
        Assert.Contains("QA_AI reported blocking issues", result.Disagreements);
        Assert.Contains("PRINCIPAL_ARCHITECT_AI reported blocking issues", result.Disagreements);
        Assert.Contains("Return to BUILDER_AI for mandatory refactor", result.Resolutions.Single());
    }

    [Fact]
    public void Evaluate_WhenRequiredRoleMissing_ThrowsInvalidOperationException()
    {
        var assessments = CreateAssessments(
            ("BUILDER_AI", true, 90, ["done"]),
            ("ARCHITECT_AI", true, 90, ["done"]),
            ("REVIEWER_AI", true, 90, ["done"]),
            ("QA_AI", true, 90, ["done"]),
            ("PERFORMANCE_AI", true, 90, ["done"]));

        var error = Assert.Throws<InvalidOperationException>(() => sut.Evaluate(assessments));

        Assert.Contains("PRINCIPAL_ARCHITECT_AI", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Evaluate_WhenScoresAreOutOfRange_ClampsScoresToBounds()
    {
        var assessments = CreateAssessments(
            ("BUILDER_AI", true, 140, ["done"]),
            ("ARCHITECT_AI", true, 120, ["done"]),
            ("REVIEWER_AI", true, -1, ["done"]),
            ("QA_AI", true, 101, ["done"]),
            ("PERFORMANCE_AI", true, -20, ["done"]),
            ("PRINCIPAL_ARCHITECT_AI", true, 10, ["done"]));

        var result = sut.Evaluate(assessments);

        Assert.Equal(100, result.ArchitectureScore);
        Assert.Equal(0, result.CodeQualityScore);
        Assert.Equal(100, result.QaStabilityScore);
        Assert.Equal(0, result.PerformanceScore);
        Assert.InRange(result.ConsensusConfidence, 0, 100);
    }

    private static IReadOnlyCollection<RoleAssessment> CreateAssessments(
        params (string Role, bool Approved, int Score, string[] Findings)[] values) =>
        values.Select(x => new RoleAssessment(x.Role, x.Approved, x.Score, x.Findings)).ToArray();
}
