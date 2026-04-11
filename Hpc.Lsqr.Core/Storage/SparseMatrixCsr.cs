namespace Hpc.Lsqr.Core.Storage;

public class SparseMatrixCsr
{
    public double[] Values { get; init; }         // Ненулевые значения
    public int[] ColumnIndices { get; init; }    // В какой колонке лежит значение
    public int[] RowPointers { get; init; }      // Где в массиве Values начинается i-я строка
    
    public int RowCount { get; init; }
    public int ColCount { get; init; }

    public SparseMatrixCsr(int rowCount, int colCount, int nnz)
    {
        RowCount = rowCount;
        ColCount = colCount;
        Values = new double[nnz];
        ColumnIndices = new int[nnz];
        RowPointers = new int[rowCount + 1];
    }
}