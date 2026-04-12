using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SearchTrees.Core.Dop.BTreeFlat;

/// <summary>
/// Узел B-дерева, жестко выровненный по кэш-линии (64 байта).
/// Использование StructLayout обеспечивает предсказуемое размещение в памяти.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
public struct BTreeNode
{
    // 4 байта
    public int KeyCount;
        
    // 4 байта (используем int как bool для выравнивания)
    public int IsLeaf; 

    // 4 * 4 = 16 байт
    public int K0, K1, K2, K3;
        
    // 4 * 4 = 16 байт
    public int V0, V1, V2, V3;

    // 5 * 4 = 20 байт (индексы потомков в массиве узлов)
    public int C0, C1, C2, C3, C4;

    // Итого: 4+4+16+16+20 = 60 байт. 
    // Компилятор добавит 4 байта паддинга до 64 (Size = 64).

    public void SetLeaf(bool leaf) => IsLeaf = leaf ? 1 : 0;
    public bool GetIsLeaf() => IsLeaf == 1;
    
    public int FindIndexSimd(int key)
    {
        // 1. Создаем векторы
        var keysVector = Vector128.Create(K0, K1, K2, K3);
        var targetVector = Vector128.Create(key);

        // 2. Векторное сравнение. Возвращает вектор из 0 (false) или -1 (true) для каждого элемента
        var mask = Vector128.Equals(keysVector, targetVector);

        // 3. Получаем битовую маску (4 бита, по 1 на каждый элемент)
        // .ExtractMostSignificantBits() работает максимально быстро на любой архитектуре
        uint bitMask = mask.ExtractMostSignificantBits();

        if (bitMask != 0)
        {
            // 4. BitOperations.TrailingZeroCount возвращает индекс первого установленного бита
            return System.Numerics.BitOperations.TrailingZeroCount(bitMask);
        }

        return -1;
    }
}