using SearchTrees.Core.Interfaces;

namespace SearchTrees.Tests.Correctness;

public abstract class StaticIndexTestsBase
{
    // Фабричный метод инициализации индекса готовым набором данных
    protected abstract ISearchIndex<int, int> CreateIndex(int[] keys, int[] values);

    [Fact]
    public void EmptyIndex_Contains_ReturnsFalse()
    {
        var index = CreateIndex(Array.Empty<int>(), Array.Empty<int>());

        Assert.False(index.Contains(10));
        Assert.False(index.TryGetValue(10, out int val));
        Assert.Equal(0, val);
        Assert.Equal(0, index.Count);
    }

    [Fact]
    public void SingleElement_CanBeFound()
    {
        var index = CreateIndex(new[] { 42 }, new[] { 100 });

        Assert.True(index.Contains(42));
        Assert.True(index.TryGetValue(42, out int val));
        Assert.Equal(100, val);
        Assert.Equal(1, index.Count);
    }

    [Fact]
    public void MultipleElements_AllFoundCorrectly()
    {
        int[] keys = { 50, 20, 80, 10, 30, 70, 90 };
        int[] values = { 500, 200, 800, 100, 300, 700, 900 };

        var index = CreateIndex(keys, values);

        Assert.Equal(keys.Length, index.Count);

        for (int i = 0; i < keys.Length; i++)
        {
            Assert.True(index.TryGetValue(keys[i], out int val));
            Assert.Equal(values[i], val);
        }
    }

    [Fact]
    public void MissingElements_NotFound()
    {
        int[] keys = { 10, 20, 30, 40, 50 };
        int[] values = { 1, 2, 3, 4, 5 };

        var index = CreateIndex(keys, values);

        // Проверяем границы: меньше минимума, больше максимума, и дырки между ключами
        int[] missingKeys = { 5, 15, 25, 55, 100 };

        foreach (var mk in missingKeys)
        {
            Assert.False(index.Contains(mk));
            Assert.False(index.TryGetValue(mk, out _));
        }
    }

    [Fact]
    public void LargeRandomDataset_CorrectlyStoresAndRetrieves()
    {
        int count = 10_000;
        var rnd = new Random(42);

        // Генерируем массив уникальных ключей и перемешиваем
        int[] keys = Enumerable.Range(1, count).OrderBy(x => rnd.Next()).ToArray();
        int[] values = keys.Select(k => k * 2).ToArray();

        // Act - Создание индекса (внутри конструктора данные должны отсортироваться/переложиться)
        var index = CreateIndex(keys, values);

        // Assert - Проверка
        Assert.Equal(count, index.Count);

        for (int i = 0; i < keys.Length; i++)
        {
            Assert.True(index.TryGetValue(keys[i], out int val));
            Assert.Equal(keys[i] * 2, val);
        }
    }
}