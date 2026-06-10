using FiniteDifference.Core;
using FiniteDifference.Core.Dod;
using FiniteDifference.Core.Original;

namespace FiniteDifference.Tests;

public class SolverCorrectnessTests
{
    [Theory]
    [InlineData(0.1, 0.1)]
    [InlineData(0.001, 0.1)]
    public void ExplicitDodScalar_MatchesOriginal(double dt, double h)
    {
        var parameters = new GridParameters(dt, h);

        var expected = OriginalExplicitSolver.Solve(parameters);
        var actual = ExplicitDodSolver.SolveScalar(parameters);

        AssertResultsEqual(expected, actual, 1e-12);
    }

    [Theory]
    [InlineData(0.1, 0.1)]
    [InlineData(0.001, 0.1)]
    public void ExplicitDodSimd_MatchesScalar(double dt, double h)
    {
        var parameters = new GridParameters(dt, h);

        var expected = ExplicitDodSolver.SolveScalar(parameters);
        var actual = ExplicitDodSolver.SolveSimd(parameters);

        AssertResultsEqual(expected, actual, 1e-12);
    }

    [Theory]
    [InlineData(0.1, 0.1)]
    [InlineData(0.001, 0.1)]
    public void ImplicitDodScalar_MatchesOriginal(double dt, double h)
    {
        var parameters = new GridParameters(dt, h);

        var expected = OriginalImplicitSolver.Solve(parameters);
        var actual = ImplicitDodSolver.SolveScalar(parameters);

        AssertResultsEqual(expected, actual, 1e-12);
    }

    [Theory]
    [InlineData(0.1, 0.1)]
    [InlineData(0.001, 0.1)]
    public void ImplicitDodSimd_MatchesScalar(double dt, double h)
    {
        var parameters = new GridParameters(dt, h);

        var expected = ImplicitDodSolver.SolveScalar(parameters);
        var actual = ImplicitDodSolver.SolveSimd(parameters);

        AssertResultsEqual(expected, actual, 1e-12);
    }

    [Theory]
    [InlineData(0.1, 0.1)]
    [InlineData(0.001, 0.1)]
    public void AllSolvers_KeepBoundaryConditions(double dt, double h)
    {
        var parameters = new GridParameters(dt, h);
        SimulationResult[] results =
        [
            OriginalExplicitSolver.Solve(parameters),
            OriginalImplicitSolver.Solve(parameters),
            ExplicitDodSolver.SolveScalar(parameters),
            ExplicitDodSolver.SolveSimd(parameters),
            ImplicitDodSolver.SolveScalar(parameters),
            ImplicitDodSolver.SolveSimd(parameters)
        ];

        foreach (var result in results)
        {
            for (int timeIndex = 0; timeIndex < result.TimeNodeCount; timeIndex++)
            {
                Assert.Equal(0d, result[timeIndex, 0], 12);
                Assert.Equal(2d * timeIndex * dt, result[timeIndex, result.SpaceNodeCount - 1], 12);
            }
        }
    }

    [Theory]
    [InlineData(0.1, 0.1)]
    [InlineData(0.001, 0.1)]
    public void DodAndOriginalResults_AreFinite(double dt, double h)
    {
        var parameters = new GridParameters(dt, h);
        SimulationResult[] results =
        [
            OriginalExplicitSolver.Solve(parameters),
            OriginalImplicitSolver.Solve(parameters),
            ExplicitDodSolver.SolveSimd(parameters),
            ImplicitDodSolver.SolveSimd(parameters)
        ];

        foreach (var result in results)
        {
            foreach (double value in result.Values)
            {
                Assert.False(double.IsNaN(value));
                Assert.False(double.IsInfinity(value));
            }
        }
    }

    private static void AssertResultsEqual(SimulationResult expected, SimulationResult actual, double tolerance)
    {
        Assert.Equal(expected.TimeNodeCount, actual.TimeNodeCount);
        Assert.Equal(expected.SpaceNodeCount, actual.SpaceNodeCount);
        Assert.Equal(expected.Values.Length, actual.Values.Length);

        for (int index = 0; index < expected.Values.Length; index++)
        {
            Assert.InRange(actual.Values[index], expected.Values[index] - tolerance, expected.Values[index] + tolerance);
        }
    }
}
