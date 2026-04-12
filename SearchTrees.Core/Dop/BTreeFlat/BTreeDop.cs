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
        // 1. Пытаемся обновить. Если удалось — ничего больше не делаем
        if (UpdateIfExists(_rootIndex, key, value)) return;

        // 2. Если корень полон, сплитим
        if (_nodes[_rootIndex].KeyCount == 4)
        {
            int newRoot = AllocateNode(false);
            _nodes[newRoot].C0 = _rootIndex;
            SplitChild(newRoot, 0, _rootIndex);
            _rootIndex = newRoot;
        }

        // 3. Вставляем новый ключ
        InsertNonFull(_rootIndex, key, value);
        
        // 4. Инкремент ТОЛЬКО здесь, так как UpdateIfExists отработал выше
        Count++;
    }

    private void InsertNonFull(int nodeIdx, TKey key, TValue value)
    {
        ref BTreeNode node = ref _nodes[nodeIdx];

        if (node.GetIsLeaf())
        {
            // 1. Поиск дубликата
            for (int k = 0; k < node.KeyCount; k++)
            {
                if (key.CompareTo(GetKey(ref node, k)) == 0)
                {
                    GetValue(ref node, k) = value;
                    return;
                }
            }
            
            // 2. Сдвиг и вставка
            int i = node.KeyCount - 1;
            while (i >= 0 && key.CompareTo(GetKey(ref node, i)) < 0)
            {
                SetKeyVal(ref node, i + 1, GetKey(ref node, i), GetValue(ref node, i));
                i--;
            }

            SetKeyVal(ref node, i + 1, key, value);
            node.KeyCount++;
        }
        else
        {
            // 1. Поиск ребенка
            int i = node.KeyCount - 1;
            while (i >= 0 && key.CompareTo(GetKey(ref node, i)) < 0) i--;
            i++;

            // 2. Превентивный сплит
            if (_nodes[GetChild(ref node, i)].KeyCount == 4)
            {
                SplitChild(nodeIdx, i, GetChild(ref node, i));
                // После сплита родитель мог обновиться, перечитываем ref
                ref BTreeNode updatedNode = ref _nodes[nodeIdx];
                if (key.CompareTo(GetKey(ref updatedNode, i)) > 0) i++;
            }
            InsertNonFull(GetChild(ref node, i), key, value);
        }
    }

    private void SplitChild(int parentIdx, int i, int fullChildIdx)
    {
        int newNodeIdx = AllocateNode(_nodes[fullChildIdx].GetIsLeaf());
        ref BTreeNode parent = ref _nodes[parentIdx];
        ref BTreeNode fullChild = ref _nodes[fullChildIdx];
        ref BTreeNode newNode = ref _nodes[newNodeIdx];

        // 1. Перенос: Медиана fullChild (индекс 2) идет в родителя.
        // Ключи 0,1 остаются в fullChild. Ключи 3,4 идут в newNode.
        newNode.KeyCount = 2;
        fullChild.KeyCount = 2;

        for (int j = 0; j < 2; j++)
            SetKeyVal(ref newNode, j, GetKey(ref fullChild, j + 2), GetValue(ref fullChild, j + 2));

        if (!fullChild.GetIsLeaf())
        {
            for (int j = 0; j < 3; j++) 
                SetChild(ref newNode, j, GetChild(ref fullChild, j + 2));
        }

        // 2. Сдвиг детей родителя (ТОЛЬКО если parent.KeyCount < 4)
        // i — это индекс ребенка, который сплитится. Сдвигаем всё, что правее i.
        for (int j = parent.KeyCount; j > i; j--)
        {
            SetChild(ref parent, j + 1, GetChild(ref parent, j));
        }
    
        // 3. Ставим нового ребенка
        SetChild(ref parent, i + 1, newNodeIdx);

        // 4. Сдвиг ключей родителя
        for (int j = parent.KeyCount - 1; j >= i; j--)
        {
            SetKeyVal(ref parent, j + 1, GetKey(ref parent, j), GetValue(ref parent, j));
        }

        // 5. Поднимаем медиану
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
    private int GetChild(ref BTreeNode node, int i) 
    {
        return i switch
        {
            0 => node.C0,
            1 => node.C1,
            2 => node.C2,
            3 => node.C3,
            4 => node.C4,
            _ => throw new IndexOutOfRangeException($"Индекс потомка {i} вне диапазона.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetChild(ref BTreeNode node, int i, int val)
    {
        switch (i)
        {
            case 0: node.C0 = val; break;
            case 1: node.C1 = val; break;
            case 2: node.C2 = val; break;
            case 3: node.C3 = val; break;
            case 4: node.C4 = val; break;
            default: throw new IndexOutOfRangeException($"Индекс потомка {i} вне диапазона.");
        }
    }
    

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