using SearchTrees.Core.Interfaces;
using SearchTrees.Core.Oop;

namespace SearchTrees.Tests.Correctness;

public class BTreeOopDegree2Tests : DynamicIndexTestsBase
{
    protected override IDynamicIndex<int, int> CreateIndex()
    {
        // Минимально возможная степень. 
        // Узлы будут делиться очень часто (каждые 3 элемента).
        // Отличный стресс-тест для логики SplitChild.
        return new BTreeOop<int, int>(degree: 2);
    }
}

public class BTreeOopDegree10Tests : DynamicIndexTestsBase
{
    protected override IDynamicIndex<int, int> CreateIndex()
    {
        // Более реалистичная степень для in-memory деревьев.
        return new BTreeOop<int, int>(degree: 10);
    }
}
    
// Специфичный тест именно для B-дерева, проверяющий валидацию
public class BTreeOopSpecificTests
{
    [Fact]
    public void Constructor_DegreeLessThan2_ThrowsArgumentException()
    {
        Assert.Throws<System.ArgumentException>(() => new BTreeOop<int, int>(1));
    }
}