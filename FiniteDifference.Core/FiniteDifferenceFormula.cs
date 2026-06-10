namespace FiniteDifference.Core;

internal static class FiniteDifferenceFormula
{
    public static double LeftBoundaryValue() => 0d;

    public static double RightBoundaryValue(int nextTimeIndex, GridParameters parameters) =>
        2d * nextTimeIndex * parameters.TimeStep;

    public static double SourceTerm(int timeIndex, int spaceIndex, GridParameters parameters)
    {
        double x = parameters.SpaceStep * (spaceIndex - 1);
        return parameters.TimeStep * ((x * (x + 1d)) - (2d * timeIndex * parameters.TimeStep));
    }

    public static double[] CreateSpaceSourceTerms(GridParameters parameters)
    {
        int spaceNodeCount = parameters.SpaceNodeCount;
        var sourceTerms = new double[spaceNodeCount];

        for (int spaceIndex = 0; spaceIndex < spaceNodeCount; spaceIndex++)
        {
            double x = parameters.SpaceStep * (spaceIndex - 1);
            sourceTerms[spaceIndex] = x * (x + 1d);
        }

        return sourceTerms;
    }
}
