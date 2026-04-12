using SearchTrees.Core.Interfaces;
using SearchTrees.Core.Oop;

namespace SearchTrees.Tests.Correctness;

public class BinarySearchTreeTests : DynamicIndexTestsBase
{
    protected override IDynamicIndex<int, int> CreateIndex()
    {
        return new BinarySearchTree<int, int>();
    }
}