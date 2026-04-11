namespace Hpc.Lsqr.Core.Storage;

public class SparseMatrixCsc
{
    public double[] Values { get; }      // Значения, отсортированные по колонкам
    public int[] RowIndices { get; }    // Индексы строк для каждого значения
    public int[] ColPointers { get; }   // Указатели на начало каждой колонки в Values

    public int RowCount { get; }
    public int ColCount { get; }

    public SparseMatrixCsc(int rowCount, int colCount, double[] values, int[] rowIndices, int[] colPointers)
    {
        RowCount = rowCount;
        ColCount = colCount;
        Values = values;
        RowIndices = rowIndices;
        ColPointers = colPointers;
    }

    // Метод для вычисления A^T * u
    // По сути, это обычное умножение "строк" транспонированной матрицы
    public void MultiplyTranspose(double[] u, double[] v)
    {
        for (int j = 0; j < ColCount; j++)
        {
            double sum = 0;
            int start = ColPointers[j];
            int end = ColPointers[j + 1];

            for (int k = start; k < end; k++)
            {
                // Values[k] — это элемент в j-й колонке и RowIndices[k]-й строке
                sum += Values[k] * u[RowIndices[k]];
            }
            v[j] = sum;
        }
    }
}