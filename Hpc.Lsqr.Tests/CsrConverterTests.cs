using Hpc.Lsqr.Core.Converters;
using Hpc.Lsqr.Core.Storage;

namespace Hpc.Lsqr.Tests;

public class CsrConverterTests
{
    [Fact]
    public void ToCsr_ShouldCorrectlyConvertSimpleMatrix()
    {
        // Дано: Матрица 3x3
        // [ 10,  0, 20 ]
        // [  0, 30,  0 ]
        // [  0,  0, 40 ]
        var triplets = new List<Triplet>
        {
            new(0, 0, 10.0),
            new(0, 2, 20.0),
            new(1, 1, 30.0),
            new(2, 2, 40.0)
        };

        // Когда
        var csr = MatrixConverter.ToCsr(3, 3, triplets);

        // Тогда
        Assert.Equal(new double[] { 10, 20, 30, 40 }, csr.Values);
        Assert.Equal(new int[] { 0, 2, 1, 2 }, csr.ColumnIndices);
        Assert.Equal(new int[] { 0, 2, 3, 4 }, csr.RowPointers);
    }
}