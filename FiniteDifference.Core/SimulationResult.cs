namespace FiniteDifference.Core;

public sealed class SimulationResult
{
    public SimulationResult(GridParameters parameters, double[] values)
    {
        parameters.Validate();

        int expectedLength = parameters.TimeNodeCount * parameters.SpaceNodeCount;
        if (values.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Expected {expectedLength} values for the grid, but got {values.Length}.",
                nameof(values));
        }

        Parameters = parameters;
        Values = values;
    }

    public GridParameters Parameters { get; }

    public double[] Values { get; }

    public int TimeNodeCount => Parameters.TimeNodeCount;

    public int SpaceNodeCount => Parameters.SpaceNodeCount;

    public double this[int timeIndex, int spaceIndex] => Values[GetOffset(timeIndex, spaceIndex)];

    public ReadOnlySpan<double> GetTimeLayer(int timeIndex)
    {
        ValidateIndices(timeIndex, 0);
        return Values.AsSpan(timeIndex * SpaceNodeCount, SpaceNodeCount);
    }

    public double ComputeChecksum()
    {
        double checksum = 0d;

        for (int i = 0; i < Values.Length; i++)
        {
            checksum = (checksum * 31d) + Values[i];
        }

        return checksum;
    }

    public int GetOffset(int timeIndex, int spaceIndex)
    {
        ValidateIndices(timeIndex, spaceIndex);
        return (timeIndex * SpaceNodeCount) + spaceIndex;
    }

    private void ValidateIndices(int timeIndex, int spaceIndex)
    {
        if ((uint)timeIndex >= (uint)TimeNodeCount)
        {
            throw new ArgumentOutOfRangeException(nameof(timeIndex), timeIndex, "Invalid time index.");
        }

        if ((uint)spaceIndex >= (uint)SpaceNodeCount)
        {
            throw new ArgumentOutOfRangeException(nameof(spaceIndex), spaceIndex, "Invalid space index.");
        }
    }
}
