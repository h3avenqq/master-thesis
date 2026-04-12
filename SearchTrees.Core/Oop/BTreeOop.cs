using SearchTrees.Core.Interfaces;

namespace SearchTrees.Core.Oop;

/// <summary>
/// ООП-реализация B-дерева. 
/// Снижает глубину дерева по сравнению с BST, но создает огромное количество мелких массивов в куче,
/// что приводит к фрагментации памяти и нагрузке на Garbage Collector.
/// </summary>
public class BTreeOop<TKey, TValue> : IDynamicIndex<TKey, TValue>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    private readonly int _degree; // Минимальная степень дерева (t). Максимум ключей = 2t - 1

    private class BTreeNode
    {
        public int KeyCount;
        public TKey[] Keys;
        public TValue[] Values;
        public BTreeNode[] Children;
        public bool IsLeaf;

        public BTreeNode(int degree, bool isLeaf)
        {
            IsLeaf = isLeaf;
            // Множественные аллокации массивов при создании каждого узла!
            Keys = new TKey[2 * degree - 1];
            Values = new TValue[2 * degree - 1];
            Children = new BTreeNode[2 * degree];
        }
    }

    private BTreeNode? _root;
    public int Count { get; private set; }

    public BTreeOop(int degree)
    {
        if (degree < 2) throw new ArgumentException("Степень дерева должна быть >= 2");
        _degree = degree;
    }

    public bool Contains(TKey key) => TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_root == null)
        {
            value = default;
            return false;
        }

        return SearchInternal(_root, key, out value);
    }

    private bool SearchInternal(BTreeNode node, TKey key, out TValue value)
    {
        int i = 0;
        // Линейный поиск ключа внутри узла
        while (i < node.KeyCount && key.CompareTo(node.Keys[i]) > 0)
        {
            i++;
        }

        if (i < node.KeyCount && key.CompareTo(node.Keys[i]) == 0)
        {
            value = node.Values[i];
            return true;
        }

        if (node.IsLeaf)
        {
            value = default;
            return false;
        }

        // Переход по ссылке к потомку (Pointer Chasing -> Cache Miss)
        return SearchInternal(node.Children[i], key, out value);
    }

    public void Insert(TKey key, TValue value)
    {
        // Сначала проверяем, существует ли ключ. Если да - просто обновляем значение.
        if (_root != null && UpdateIfExists(_root, key, value))
        {
            return; // Успешно обновили, Count увеличивать не нужно
        }

        // Если ключа нет, выполняем классическую логику вставки B-дерева
        if (_root == null)
        {
            _root = new BTreeNode(_degree, true);
            _root.Keys[0] = key;
            _root.Values[0] = value;
            _root.KeyCount = 1;
            Count++;
            return;
        }

        if (_root.KeyCount == 2 * _degree - 1)
        {
            var newRoot = new BTreeNode(_degree, false);
            newRoot.Children[0] = _root;
            SplitChild(newRoot, 0, _root);
                
            int i = newRoot.Keys[0].CompareTo(key) < 0 ? 1 : 0;
            InsertNonFull(newRoot.Children[i], key, value);
            _root = newRoot;
        }
        else
        {
            InsertNonFull(_root, key, value);
        }
        Count++;
    }

    // Вспомогательный рекурсивный метод для поиска и обновления дубликата
    private bool UpdateIfExists(BTreeNode node, TKey key, TValue value)
    {
        int i = 0;
        while (i < node.KeyCount && key.CompareTo(node.Keys[i]) > 0)
        {
            i++;
        }

        if (i < node.KeyCount && key.CompareTo(node.Keys[i]) == 0)
        {
            node.Values[i] = value; // Ключ найден, обновляем значение
            return true;
        }

        if (node.IsLeaf)
        {
            return false; // Дошли до листа, ключа нет
        }

        return UpdateIfExists(node.Children[i], key, value);
    }

    private void InsertNonFull(BTreeNode node, TKey key, TValue value)
    {
        int i = node.KeyCount - 1;

        if (node.IsLeaf)
        {
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                node.Keys[i + 1] = node.Keys[i];
                node.Values[i + 1] = node.Values[i];
                i--;
            }

            node.Keys[i + 1] = key;
            node.Values[i + 1] = value;
            node.KeyCount++;
        }
        else
        {
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                i--;
            }

            i++;

            if (node.Children[i].KeyCount == 2 * _degree - 1)
            {
                SplitChild(node, i, node.Children[i]);
                if (key.CompareTo(node.Keys[i]) > 0)
                {
                    i++;
                }
            }

            InsertNonFull(node.Children[i], key, value);
        }
    }

    private void SplitChild(BTreeNode parentNode, int childIndex, BTreeNode fullChild)
    {
        var newChild = new BTreeNode(_degree, fullChild.IsLeaf);
        newChild.KeyCount = _degree - 1;

        for (int j = 0; j < _degree - 1; j++)
        {
            newChild.Keys[j] = fullChild.Keys[j + _degree];
            newChild.Values[j] = fullChild.Values[j + _degree];
        }

        if (!fullChild.IsLeaf)
        {
            for (int j = 0; j < _degree; j++)
            {
                newChild.Children[j] = fullChild.Children[j + _degree];
            }
        }

        fullChild.KeyCount = _degree - 1;

        for (int j = parentNode.KeyCount; j >= childIndex + 1; j--)
        {
            parentNode.Children[j + 1] = parentNode.Children[j];
        }

        parentNode.Children[childIndex + 1] = newChild;

        for (int j = parentNode.KeyCount - 1; j >= childIndex; j--)
        {
            parentNode.Keys[j + 1] = parentNode.Keys[j];
            parentNode.Values[j + 1] = parentNode.Values[j];
        }

        parentNode.Keys[childIndex] = fullChild.Keys[_degree - 1];
        parentNode.Values[childIndex] = fullChild.Values[_degree - 1];
        parentNode.KeyCount++;
    }

    public bool Remove(TKey key)
    {
        throw new NotSupportedException();
    }
}