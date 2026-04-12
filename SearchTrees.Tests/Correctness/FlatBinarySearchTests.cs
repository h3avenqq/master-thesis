using SearchTrees.Core.Arrays;
using SearchTrees.Core.Interfaces;

namespace SearchTrees.Tests.Correctness;

public class FlatBinarySearchTests : StaticIndexTestsBase
{
    protected override ISearchIndex<int, int> CreateIndex(int[] keys, int[] values)
    {
        return new FlatBinarySearch<int, int>(keys, values);
    }
}