using SearchTrees.Core.Interfaces;

namespace SearchTrees.Core.Arrays;

/// <summary>
/// Статический индекс на основе отсортированных плоских массивов (SoA: Structure of Arrays).
/// Идеален по расходу памяти (0 накладных расходов), но на больших объемах
/// страдает от промахов кэша при прыжках бинарного поиска.
/// </summary>
public class FlatBinarySearch<TKey, TValue> : ISearchIndex<TKey, TValue>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    private readonly TKey[] _keys;
    private readonly TValue[] _values;

    public int Count => _keys.Length;

    // Конструктор принимает несортированные данные и подготавливает структуру
    public FlatBinarySearch(TKey[] keys, TValue[] values)
    {
        if (keys.Length != values.Length)
            throw new ArgumentException("Длины массивов ключей и значений должны совпадать.");

        // Создаем копии, чтобы не мутировать исходные данные
        _keys = new TKey[keys.Length];
        _values = new TValue[values.Length];
        Array.Copy(keys, _keys, keys.Length);
        Array.Copy(values, _values, values.Length);

        // Сортируем ключи, синхронно перемещая элементы в массиве значений
        Array.Sort(_keys, _values);
    }

    public bool Contains(TKey key) => TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue value)
    {
        int left = 0;
        int right = _keys.Length - 1;

        // Классический бинарный поиск
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int cmp = key.CompareTo(_keys[mid]);

            if (cmp == 0)
            {
                value = _values[mid];
                return true;
            }

            if (cmp < 0)
                right = mid - 1;
            else
                left = mid + 1;
        }

        value = default;
        return false;
    }
}