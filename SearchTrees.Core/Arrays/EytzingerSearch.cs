using SearchTrees.Core.Interfaces;

namespace SearchTrees.Core.Arrays;

/// <summary>
/// Статический индекс с раскладкой Eytzinger (BFS-дерево в плоском массиве).
/// Первые шаги поиска (верхние уровни дерева) гарантированно попадают в кэш процессора,
/// что делает этот поиск значительно быстрее классического бинарного поиска на больших объемах данных.
/// </summary>
public class EytzingerSearch<TKey, TValue> : ISearchIndex<TKey, TValue>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    private readonly TKey[] _keys;
    private readonly TValue[] _values;

    public int Count => _keys.Length;

    public EytzingerSearch(TKey[] keys, TValue[] values)
    {
        if (keys.Length != values.Length)
            throw new ArgumentException("Длины массивов ключей и значений должны совпадать.");

        // 1. Сортируем входные данные
        var sortedKeys = new TKey[keys.Length];
        var sortedValues = new TValue[values.Length];
        Array.Copy(keys, sortedKeys, keys.Length);
        Array.Copy(values, sortedValues, values.Length);
        Array.Sort(sortedKeys, sortedValues);

        // 2. Инициализируем массивы для Eytzinger раскладки
        _keys = new TKey[keys.Length];
        _values = new TValue[values.Length];

        // 3. Рекурсивно перекладываем отсортированные данные в BFS-порядок
        int index = 0;
        BuildEytzinger(0, sortedKeys, sortedValues, ref index);
    }

    private void BuildEytzinger(int i, TKey[] sortedKeys, TValue[] sortedValues, ref int index)
    {
        if (i < _keys.Length)
        {
            // Сначала обходим левое поддерево
            BuildEytzinger(2 * i + 1, sortedKeys, sortedValues, ref index);

            // Записываем корень текущего поддерева
            _keys[i] = sortedKeys[index];
            _values[i] = sortedValues[index];
            index++;

            // Затем обходим правое поддерево
            BuildEytzinger(2 * i + 2, sortedKeys, sortedValues, ref index);
        }
    }

    public bool Contains(TKey key) => TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue value)
    {
        int i = 0;
        int n = _keys.Length;

        while (i < n)
        {
            int cmp = key.CompareTo(_keys[i]);
            if (cmp == 0)
            {
                value = _values[i];
                return true;
            }

            // Безветвистый переход к потомкам: 
            // если ищем меньше (cmp < 0) -> 2*i + 1
            // если ищем больше (cmp > 0) -> 2*i + 2
            i = cmp < 0 ? 2 * i + 1 : 2 * i + 2;
        }

        value = default;
        return false;
    }
}