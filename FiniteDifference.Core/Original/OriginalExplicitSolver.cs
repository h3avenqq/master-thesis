namespace FiniteDifference.Core.Original;

public static class OriginalExplicitSolver
{
    public static SimulationResult Solve(GridParameters parameters)
    {
        parameters.Validate();

        int timeNodeCount = parameters.TimeNodeCount;
        int spaceNodeCount = parameters.SpaceNodeCount;
        double timeStep = parameters.TimeStep;
        double spaceStep = parameters.SpaceStep;
        double[,] solution = new double[timeNodeCount, spaceNodeCount];

        for (int spaceIndex = 0; spaceIndex < spaceNodeCount; spaceIndex++)
        {
            solution[0, spaceIndex] = 0d;
        }

        for (int timeIndex = 0; timeIndex < timeNodeCount - 1; timeIndex++)
        {
            for (int spaceIndex = 1; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                solution[timeIndex + 1, spaceIndex] = solution[timeIndex, spaceIndex]
                    + (timeStep / (spaceStep * spaceStep))
                    * (solution[timeIndex, spaceIndex + 1]
                        - (2d * solution[timeIndex, spaceIndex])
                        + solution[timeIndex, spaceIndex - 1])
                    + FiniteDifferenceFormula.SourceTerm(timeIndex, spaceIndex, parameters);
            }

            solution[timeIndex + 1, 0] = FiniteDifferenceFormula.LeftBoundaryValue();
            solution[timeIndex + 1, spaceNodeCount - 1] =
                FiniteDifferenceFormula.RightBoundaryValue(timeIndex + 1, parameters);
        }

        return new SimulationResult(parameters, Flatten(solution));
    }

    private static double[] Flatten(double[,] source)
    {
        int timeNodeCount = source.GetLength(0);
        int spaceNodeCount = source.GetLength(1);
        var values = new double[timeNodeCount * spaceNodeCount];

        for (int timeIndex = 0; timeIndex < timeNodeCount; timeIndex++)
        {
            int rowOffset = timeIndex * spaceNodeCount;
            for (int spaceIndex = 0; spaceIndex < spaceNodeCount; spaceIndex++)
            {
                values[rowOffset + spaceIndex] = source[timeIndex, spaceIndex];
            }
        }

        return values;
    }
}
