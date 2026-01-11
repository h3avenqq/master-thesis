using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace OopVsFpExperiment;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ParadigmBenchmark>();
    }
}

#region ParadigmBenchmark

[MemoryDiagnoser]
public class ParadigmBenchmark 
{
    [Params(1_000, 100_000, 1_000_000, 100_000_000)]
    public int N;

    private List<BankAccountClass> _oopAccounts;

    private List<BankAccountRecord> _fpAccounts;

    private double[] _rawBalances;

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);

        _oopAccounts = new List<BankAccountClass>(N);
        _fpAccounts = new List<BankAccountRecord>(N);
        _rawBalances = new double[N];

        for (int i = 0; i < N; i++)
        {
            double balance = rand.NextDouble() * 10000;
            
            _oopAccounts.Add(new BankAccountClass { Balance = balance });
            
            _fpAccounts.Add(new BankAccountRecord(balance));

            _rawBalances[i] = balance;
        }
    }

    [Benchmark(Baseline = true)]
    public void Scenario_OOP_Mutation()
    {
        foreach (var account in _oopAccounts)
        {
            account.ApplyInterest(1.05);
        }
    }

    [Benchmark]
    public List<BankAccountRecord> Scenario_FP_Naive_Linq()
    {
        return _fpAccounts
            .Select(a => a with { Balance = a.Balance * 1.05 })
            .ToList();
    }

    [Benchmark]
    public void Scenario_DataOriented_Span()
    {
        Span<double> balances = _rawBalances.AsSpan();
        
        for (int i = 0; i < balances.Length; i++)
        {
            balances[i] *= 1.05;
        }
    }
}

public class BankAccountClass
{
    public double Balance { get; set; }
    
    public void ApplyInterest(double rate)
    {
        Balance *= rate;
    }
}

public record BankAccountRecord(double Balance);

#endregion


#region BillionBenchmark

[MemoryDiagnoser]
public class BillionBenchmark
{
    [Params(1_000_000_000)]
    public int N;

    private double[] _rawBalances;

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine("Allocating 8GB array... Please wait.");
        
        try 
        {
            // Выделяем огромный массив
            _rawBalances = new double[N];
            
            // Заполняем параллельно, чтобы не ждать вечность
            var rand = new Random(42);
            Parallel.For(0, N, i =>
            {
                _rawBalances[i] = rand.NextDouble() * 10000;
            });
            
            Console.WriteLine("Allocation done. Starting benchmarks.");
        }
        catch (OutOfMemoryException)
        {
            Console.WriteLine("!!! ОШИБКА: НЕ ХВАТИЛО ПАМЯТИ !!!");
            Console.WriteLine("Тест на 1 миллиард требует ~8 ГБ свободной RAM.");
            throw;
        }
    }

    [Benchmark(Baseline = true)]
    public void SingleThread_Span()
    {
        Span<double> balances = _rawBalances.AsSpan();
        for (int i = 0; i < balances.Length; i++)
        {
            balances[i] *= 1.05;
        }
    }

    [Benchmark]
    public void MultiThread_Parallel()
    {
        Parallel.For(0, _rawBalances.Length, i =>
        {
            _rawBalances[i] *= 1.05;
        });
    }
}

#endregion
