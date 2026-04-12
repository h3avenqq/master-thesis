using System.Runtime.CompilerServices;
using SearchTrees.Core.Interfaces;

namespace SearchTrees.Core.Dop.BTreeFlat;

public class BTreeDop<TKey, TValue> : IDynamicIndex<TKey, TValue>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
    private BTreeNode[] _nodes;
    private int _freeIndex;
    private int _rootIndex;

    public int Count { get; private set; }

    public BTreeDop(int capacity)
    {
        _nodes = new BTreeNode[capacity];
        _rootIndex = AllocateNode(true);
    }

    private int AllocateNode(bool isLeaf)
    {
        if (_freeIndex >= _nodes.Length)
            throw new OutOfMemoryException("Пул узлов исчерпан");

        int index = _freeIndex++;
        _nodes[index].SetLeaf(isLeaf);
        _nodes[index].KeyCount = 0;
        return index;
    }

    public void Insert(TKey key, TValue value)
    {
        if (_nodes[_rootIndex].KeyCount == 4)
        {
            int newRoot = AllocateNode(false);
            _nodes[newRoot].C0 = _rootIndex;
            SplitChild(newRoot, 0, _rootIndex);
            _rootIndex = newRoot;
        }

        if (InsertNonFull(_rootIndex, key, value))
        {
            Count++;
        }
    }

    private bool InsertNonFull(int nodeIdx, TKey key, TValue value)
    {
        ref BTreeNode node = ref _nodes[nodeIdx];
        int i = 0;

        // Находим позицию
        while (i < node.KeyCount && key.CompareTo(GetKey(ref node, i)) > 0) i++;

        // Если нашли дубликат — обновляем и выходим
        if (i < node.KeyCount && key.CompareTo(GetKey(ref node, i)) == 0)
        {
            GetValue(ref node, i) = value;
            return false; // Ключ обновлен, не увеличиваем Count
        }

        if (node.GetIsLeaf())
        {
            // Сдвигаем и вставляем (идем с конца)
            for (int j = node.KeyCount - 1; j >= i; j--)
            {
                SetKeyVal(ref node, j + 1, GetKey(ref node, j), GetValue(ref node, j));
            }
            SetKeyVal(ref node, i, key, value);
            node.KeyCount++;
            return true; // Вставлен новый ключ
        }
        else
        {
            // Превентивный сплит
            if (_nodes[GetChild(ref node, i)].KeyCount == 4)
            {
                SplitChild(nodeIdx, i, GetChild(ref node, i));
                ref BTreeNode updatedNode = ref _nodes[nodeIdx];
            
                // Если медиана оказалась меньше ключа, идем в правого ребенка
                if (key.CompareTo(GetKey(ref updatedNode, i)) > 0) i++;
                else if (key.CompareTo(GetKey(ref updatedNode, i)) == 0)
                {
                    // Если медиана случайно совпала с ключом - обновляем
                    GetValue(ref updatedNode, i) = value;
                    return false;
                }
            }
            return InsertNonFull(GetChild(ref node, i), key, value);
        }
    }

    private void SplitChild(int parentIdx, int i, int fullChildIdx)
    {
        int newNodeIdx = AllocateNode(_nodes[fullChildIdx].GetIsLeaf());
        ref BTreeNode parent = ref _nodes[parentIdx];
        ref BTreeNode fullChild = ref _nodes[fullChildIdx];
        ref BTreeNode newNode = ref _nodes[newNodeIdx];

        // Левый узел оставляет 2 ключа (0, 1)
        fullChild.KeyCount = 2;
        // Правый узел получает 1 ключ (3)
        newNode.KeyCount = 1;

        // Перенос ключа 3 в правый узел
        SetKeyVal(ref newNode, 0, GetKey(ref fullChild, 3), GetValue(ref fullChild, 3));

        // Перенос детей в правый узел (если не лист)
        if (!fullChild.GetIsLeaf())
        {
            SetChild(ref newNode, 0, GetChild(ref fullChild, 3));
            SetChild(ref newNode, 1, GetChild(ref fullChild, 4));
        }

        // Сдвиг детей родителя
        for (int j = parent.KeyCount; j > i; j--)
            SetChild(ref parent, j + 1, GetChild(ref parent, j));
    
        SetChild(ref parent, i + 1, newNodeIdx);

        // Сдвиг ключей родителя
        for (int j = parent.KeyCount - 1; j >= i; j--)
            SetKeyVal(ref parent, j + 1, GetKey(ref parent, j), GetValue(ref parent, j));

        // Поднимаем медиану (ключ 2)
        SetKeyVal(ref parent, i, GetKey(ref fullChild, 2), GetValue(ref fullChild, 2));
        parent.KeyCount++;
    }

    // --- Вспомогательные методы Unsafe ---
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref TKey GetKey(ref BTreeNode node, int i) => ref Unsafe.Add(ref Unsafe.As<int, TKey>(ref node.K0), i);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref TValue GetValue(ref BTreeNode node, int i) =>
        ref Unsafe.Add(ref Unsafe.As<int, TValue>(ref node.V0), i);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetKeyVal(ref BTreeNode node, int i, TKey k, TValue v)
    {
        GetKey(ref node, i) = k;
        GetValue(ref node, i) = v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetChild(ref BTreeNode node, int i) => 
        Unsafe.Add(ref node.C0, i);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetChild(ref BTreeNode node, int i, int val) => 
        Unsafe.Add(ref node.C0, i) = val;
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool UpdateIfExists(int nodeIdx, TKey key, TValue value)
    {
        ref BTreeNode node = ref _nodes[nodeIdx];
    
        // 1. Поиск ключа в текущем узле
        int i = 0;
        while (i < node.KeyCount && key.CompareTo(GetKey(ref node, i)) > 0) i++;

        if (i < node.KeyCount && key.CompareTo(GetKey(ref node, i)) == 0)
        {
            GetValue(ref node, i) = value;
            return true;
        }

        // 2. Если не лист - спускаемся
        if (node.GetIsLeaf()) return false;
    
        // ВАЖНО: i - это индекс ребенка, в которого мы идем
        return UpdateIfExists(GetChild(ref node, i), key, value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return SearchRecursive(_rootIndex, key, out value);
    }
    
    private bool SearchRecursive(int nodeIdx, TKey key, out TValue value)
    {
        ref BTreeNode node = ref _nodes[nodeIdx];
        int i = 0;
        while (i < node.KeyCount && key.CompareTo(GetKey(ref node, i)) > 0) i++;

        if (i < node.KeyCount && key.CompareTo(GetKey(ref node, i)) == 0)
        {
            value = GetValue(ref node, i);
            return true;
        }

        if (node.GetIsLeaf())
        {
            value = default;
            return false;
        }

        return SearchRecursive(GetChild(ref node, i), key, out value);
    }
    

    public bool Contains(TKey key) => TryGetValue(key, out _);
    public bool Remove(TKey key) => throw new NotSupportedException();
}