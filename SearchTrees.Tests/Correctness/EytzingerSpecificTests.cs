using SearchTrees.Core.Arrays;

namespace SearchTrees.Tests.Correctness;

public class EytzingerSpecificTests
{
    [Fact]
    public void Constructor_MismatchedArrays_ThrowsArgumentException()
    {
        // Arrange
        var keys = new int[] { 1, 2, 3 };
        var vals = new int[] { 1, 2 }; // Длина 2 вместо 3

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EytzingerSearch<int, int>(keys, vals));
    }

    [Fact]
    public void BuildEytzinger_StructureIsValid()
    {
        // Проверка того, что корень (индекс 0) стал медианой
        // Для 3 элементов {1, 2, 3} в BFS порядке будет: 2, 1, 3
        var keys = new int[] { 1, 2, 3 };
        var vals = new int[] { 10, 20, 30 };
            
        var index = new EytzingerSearch<int, int>(keys, vals);

        // Индекс 0 должен содержать 2 (медиана)
        Assert.True(index.TryGetValue(2, out int val));
        Assert.Equal(20, val);
    }
}