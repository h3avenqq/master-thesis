namespace SearchTrees.Core.Interfaces;

/// <summary>
/// Интерфейс для динамических структур данных, поддерживающих изменение (Read-Write).
/// Имитирует поведение классического индекса базы данных.
/// </summary>
public interface IDynamicIndex<in TKey, TValue> : ISearchIndex<TKey, TValue>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    /// <summary>
    /// Вставляет новую пару ключ-значение. Если ключ существует, обновляет значение.
    /// </summary>
    /// <param name="key">Ключ для вставки.</param>
    /// <param name="value">Значение для связи с ключом.</param>
    void Insert(TKey key, TValue value);

    /// <summary>
    /// Удаляет элемент по ключу.
    /// </summary>
    /// <param name="key">Ключ для удаления.</param>
    /// <returns>True, если элемент был успешно удален, False если ключ не найден.</returns>
    bool Remove(TKey key);
}