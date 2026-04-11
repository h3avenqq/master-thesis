using Hpc.Lsqr.Core.Storage;

namespace Hpc.Lsqr.Core.Converters;

public static class MatrixConverter
{
    public static SparseMatrixCsr ToCsr(int rowCount, int colCount, IEnumerable<Triplet> triplets)
    {
        // 1. Сортируем триплеты: сначала по строке, потом по колонке (обязательно для CSR!)
        var sortedTriplets = triplets
            .OrderBy(t => t.Row)
            .ThenBy(t => t.Col)
            .ToArray();

        int nnz = sortedTriplets.Length;
        var matrix = new SparseMatrixCsr(rowCount, colCount, nnz);

        int currentValuesIdx = 0;
        int currentRow = 0;

        // RowPointers[0] всегда 0
        matrix.RowPointers[0] = 0;

        for (int i = 0; i < nnz; i++)
        {
            var t = sortedTriplets[i];
            
            matrix.Values[i] = t.Value;
            matrix.ColumnIndices[i] = t.Col;

            // Если перешли на новую строку (или пропустили пустые строки)
            while (currentRow < t.Row)
            {
                currentRow++;
                matrix.RowPointers[currentRow] = i;
            }
        }

        // Заполняем остаток RowPointers для последних или пустых строк
        while (currentRow < rowCount)
        {
            currentRow++;
            matrix.RowPointers[currentRow] = nnz;
        }

        return matrix;
    }
}