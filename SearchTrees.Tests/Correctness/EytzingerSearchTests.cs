using SearchTrees.Core.Arrays;
using SearchTrees.Core.Interfaces;

namespace SearchTrees.Tests.Correctness;

public class EytzingerSearchTests : StaticIndexTestsBase
{
    protected override ISearchIndex<int, int> CreateIndex(int[] keys, int[] values)
    {
        return new EytzingerSearch<int, int>(keys, values);
    }
}