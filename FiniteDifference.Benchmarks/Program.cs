using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using FiniteDifference.Core;
using FiniteDifference.Core.Dod;
using FiniteDifference.Core.Original;

namespace FiniteDifference.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(new[]
        {
            typeof(ExplicitSchemeBenchmark),
            typeof(ImplicitSchemeBenchmark)
        });
    }
}

public readonly record struct BenchmarkCase(double Dt, double H)
{
    public override string ToString() => $"dt={Dt}, h={H}";
}

public static class BenchmarkCases
{
    public static IEnumerable<BenchmarkCase> All =>
    [
        new(0.001, 0.05),
        new(0.0002, 0.02),
        new(0.00005, 0.01),
        new(0.00001, 0.005),
        new(0.000002, 0.00225)
    ];
}

[MemoryDiagnoser]
[RankColumn]
public class ExplicitSchemeBenchmark
{
    private GridParameters _parameters;

    [ParamsSource(nameof(Cases))]
    public BenchmarkCase Case { get; set; }

    public IEnumerable<BenchmarkCase> Cases => BenchmarkCases.All;

    [GlobalSetup]
    public void Setup() => _parameters = new GridParameters(Case.Dt, Case.H);

    [Benchmark(Baseline = true)]
    public double OriginalBaseline() => OriginalExplicitSolver.Solve(_parameters).ComputeChecksum();

    [Benchmark]
    public double DodScalar() => ExplicitDodSolver.SolveScalar(_parameters).ComputeChecksum();

    [Benchmark]
    public double DodSimd() => ExplicitDodSolver.SolveSimd(_parameters).ComputeChecksum();
}

[MemoryDiagnoser]
[RankColumn]
public class ImplicitSchemeBenchmark
{
    private GridParameters _parameters;

    [ParamsSource(nameof(Cases))]
    public BenchmarkCase Case { get; set; }

    public IEnumerable<BenchmarkCase> Cases => BenchmarkCases.All;

    [GlobalSetup]
    public void Setup() => _parameters = new GridParameters(Case.Dt, Case.H);

    [Benchmark(Baseline = true)]
    public double OriginalBaseline() => OriginalImplicitSolver.Solve(_parameters).ComputeChecksum();

    [Benchmark]
    public double DodScalar() => ImplicitDodSolver.SolveScalar(_parameters).ComputeChecksum();

    [Benchmark]
    public double DodSimd() => ImplicitDodSolver.SolveSimd(_parameters).ComputeChecksum();
}
