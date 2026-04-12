using SearchTrees.Core.Dop.BTreeFlat;
using SearchTrees.Core.Interfaces;

namespace SearchTrees.Tests.Correctness;

public class BTreeDopTests : DynamicIndexTestsBase
{
    protected override IDynamicIndex<int, int> CreateIndex() => new BTreeDop<int, int>(50000); 
}