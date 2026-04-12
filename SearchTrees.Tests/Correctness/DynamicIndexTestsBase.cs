using SearchTrees.Core.Interfaces;

namespace SearchTrees.Tests.Correctness;

/// <summary>
/// Базовый класс для тестирования любой структуры данных, 
/// реализующей IDynamicIndex (BST, B-Tree OOP, B-Tree DOP).
/// </summary>
public abstract class DynamicIndexTestsBase
{
    // Фабричный метод, который должны реализовать наследники
    protected abstract IDynamicIndex<int, int> CreateIndex();

    [Fact]
    public void EmptyIndex_Contains_ReturnsFalse()
    {
        var index = CreateIndex();

        Assert.False(index.Contains(10));
        Assert.False(index.TryGetValue(10, out int val));
        Assert.Equal(0, val);
        Assert.Equal(0, index.Count);
    }

    [Fact]
    public void Insert_SingleElement_CanBeFound()
    {
        var index = CreateIndex();

        index.Insert(42, 100);

        Assert.True(index.Contains(42));
        Assert.True(index.TryGetValue(42, out int val));
        Assert.Equal(100, val);
        Assert.Equal(1, index.Count);
    }

    [Fact]
    public void Insert_DuplicateKey_UpdatesValue()
    {
        var index = CreateIndex();

        index.Insert(10, 100);
        index.Insert(10, 200);

        Assert.True(index.TryGetValue(10, out int val));
        Assert.Equal(200, val);
        Assert.Equal(1, index.Count);
    }

    [Fact]
    public void Insert_MultipleElements_CanBeFound()
    {
        var index = CreateIndex();
        var data = new[] { 50, 20, 80, 10, 30, 70, 90 };

        foreach (var key in data)
        {
            index.Insert(key, key * 10);
        }

        Assert.Equal(data.Length, index.Count);

        foreach (var key in data)
        {
            Assert.True(index.TryGetValue(key, out int val));
            Assert.Equal(key * 10, val);
        }

        Assert.False(index.Contains(999));
    }

    [Fact]
    public void Insert_LargeRandomDataset_CorrectlyStoresAndRetrieves()
    {
        var index = CreateIndex();
        int count = 10_000;

        // Генерируем случайные уникальные ключи
        var rnd = new Random(42);
        var keys = Enumerable.Range(1, count).OrderBy(x => rnd.Next()).ToArray();

        // Act - Вставка
        for (int i = 0; i < keys.Length; i++)
        {
            index.Insert(keys[i], keys[i] * 2);
        }

        // Assert - Проверка
        Assert.Equal(count, index.Count);

        for (int i = 0; i < keys.Length; i++)
        {
            Assert.True(index.TryGetValue(keys[i], out int val));
            Assert.Equal(keys[i] * 2, val);
        }
    }
}