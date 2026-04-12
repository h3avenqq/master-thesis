using SearchTrees.Core.Interfaces;

namespace SearchTrees.Core.Oop;

/// <summary>
/// Классическая ООП-реализация бинарного дерева поиска (BST).
/// Характеризуется высокими накладными расходами памяти (заголовок объекта + ссылки) 
/// и частыми промахами кэша (Pointer Chasing) при обходе графа.
/// </summary>
public class BinarySearchTree<TKey, TValue> : IDynamicIndex<TKey, TValue>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    // Узел - это ссылочный тип (class), выделяется в куче (Heap)
    private class Node
    {
        public TKey Key;
        public TValue Value;
        public Node? Left;
        public Node? Right;

        public Node(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    private Node? _root;
    public int Count { get; private set; }

    public void Insert(TKey key, TValue value)
    {
        if (_root == null)
        {
            // Аллокация объекта в куче (Gen 0)
            _root = new Node(key, value);
            Count++;
            return;
        }

        Node current = _root;
        while (true)
        {
            int cmp = key.CompareTo(current.Key);
            if (cmp == 0)
            {
                current.Value = value; // Обновление значения
                return;
            }

            if (cmp < 0)
            {
                if (current.Left == null)
                {
                    current.Left = new Node(key, value);
                    Count++;
                    return;
                }
                current = current.Left;
            }
            else
            {
                if (current.Right == null)
                {
                    current.Right = new Node(key, value);
                    Count++;
                    return;
                }
                current = current.Right;
            }
        }
    }

    public bool Contains(TKey key) => TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue value)
    {
        Node? current = _root;
        while (current != null)
        {
            // Каждый переход по current.Left/Right - это потенциальный Cache Miss
            int cmp = key.CompareTo(current.Key);
            if (cmp == 0)
            {
                value = current.Value;
                return true;
            }
            
            current = cmp < 0 ? current.Left : current.Right;
        }

        value = default;
        return false;
    }

    public bool Remove(TKey key)
    {
        throw new NotSupportedException();
    }
}