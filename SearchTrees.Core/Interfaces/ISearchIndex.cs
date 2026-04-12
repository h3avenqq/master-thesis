namespace SearchTrees.Core.Interfaces;

/// <summary>
/// Базовый интерфейс для структур данных, поддерживающих только поиск (Read-Only).
/// </summary>
/// <typeparam name="TKey">Тип ключа</typeparam>
/// <typeparam name="TValue">Тип значения</typeparam>
public interface ISearchIndex<in TKey, TValue> 
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    /// <summary>
    /// Возвращает общее количество элементов в индексе.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Проверяет наличие ключа в индексе.
    /// </summary>
    /// <param name="key">Искомый ключ.</param>
    /// <returns>True, если ключ найден.</returns>
    bool Contains(TKey key);

    /// <summary>
    /// Выполняет поиск значения по ключу.
    /// </summary>
    /// <param name="key">Искомый ключ.</param>
    /// <param name="value">Найденное значение или default, если ключ не найден.</param>
    /// <returns>True, если поиск успешен.</returns>
    bool TryGetValue(TKey key, out TValue value);
}