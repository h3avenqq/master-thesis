using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using SearchTrees.Core.Arrays;
using SearchTrees.Core.Dop.BTreeFlat;
using SearchTrees.Core.Interfaces;
using SearchTrees.Core.Oop;

namespace SearchTrees.Benchmarks;

class Program
{
    static void Main(string[] args) => BenchmarkRunner.Run<BTreeBenchmark>();
}


[MemoryDiagnoser]
[RankColumn]
[HardwareCounters(HardwareCounter.CacheMisses)]
public class SearchReadBenchmark
{
    private int[] _keys;
    private int[] _values;
    private int[] _keysToSearch;

    private ISearchIndex<int, int> _oopBst;
    private ISearchIndex<int, int> _flatArray;
    private ISearchIndex<int, int> _eytzinger;

    [Params(1_000, 100_000, 1_000_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        _keys = Enumerable.Range(0, N).OrderBy(_ => rnd.Next()).ToArray();
        _values = _keys.Select(k => k * 2).ToArray();
            
        _keysToSearch = Enumerable.Range(0, N)
            .Select(_ => rnd.Next(0, N + (N / 2)))
            .ToArray();

        // Инициализация ООП-дерева
        var bst = new BinarySearchTree<int, int>();
        foreach (var key in _keys) 
        {
            bst.Insert(key, key * 2);
        }
        _oopBst = bst;

        // Инициализация DOD-структур
        _flatArray = new FlatBinarySearch<int, int>(_keys, _values);
        _eytzinger = new EytzingerSearch<int, int>(_keys, _values);
    }

    [Benchmark(Baseline = true)]
    public void Oop_BinarySearchTree()
    {
        foreach (var key in _keysToSearch)
        {
            _oopBst.Contains(key);
        }
    }

    [Benchmark]
    public void Dop_FlatArray()
    {
        foreach (var key in _keysToSearch)
        {
            _flatArray.Contains(key);
        }
    }

    [Benchmark]
    public void Dop_Eytzinger()
    {
        foreach (var key in _keysToSearch)
        {
            _eytzinger.Contains(key);
        }
    }
}


[MemoryDiagnoser]
[RankColumn]
[HardwareCounters(HardwareCounter.CacheMisses)]
public class BTreeBenchmark
{
    private int[] _keys;
    private int[] _keysToSearch;

    private IDynamicIndex<int, int> _oopBst;
    private IDynamicIndex<int, int> _oopBTree;
    private IDynamicIndex<int, int> _dopBTree;

    [Params(10_000, 100_000)] 
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        _keys = Enumerable.Range(0, N).OrderBy(_ => rnd.Next()).ToArray();
        _keysToSearch = _keys.OrderBy(_ => rnd.Next()).ToArray();

        // 1. ООП Базлайн: Бинарное дерево
        _oopBst = new BinarySearchTree<int, int>();
        foreach (var key in _keys) _oopBst.Insert(key, key * 2);

        // 2. ООП Базлайн: B-Tree
        _oopBTree = new BTreeOop<int, int>(3); // degree 3 = max 5 ключей
        foreach (var key in _keys) _oopBTree.Insert(key, key * 2);

        // 3. DOD B-Tree 
        _dopBTree = new BTreeDop<int, int>(N); 
        foreach (var key in _keys) _dopBTree.Insert(key, key * 2);
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    [Benchmark(Baseline = true)]
    public bool Oop_Bst()
    {
        bool result = false;
        foreach (var key in _keysToSearch)
        {
            result ^= _oopBst.Contains(key);
        }
        return result;
    }

    [Benchmark]
    public bool Oop_BTree()
    {
        bool result = false;
        foreach (var key in _keysToSearch)
        {
            result ^= _oopBTree.Contains(key);
        }
        return result;
    }

    [Benchmark]
    public bool Dop_BTree()
    {
        bool result = false;
        foreach (var key in _keysToSearch)
        {
            result ^= _dopBTree.Contains(key);
        }
        return result;
    }
}