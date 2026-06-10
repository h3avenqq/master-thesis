namespace FiniteDifference.Core.Original;

public static class OriginalImplicitSolver
{
    public static SimulationResult Solve(GridParameters parameters)
    {
        parameters.Validate();

        int timeNodeCount = parameters.TimeNodeCount;
        int spaceNodeCount = parameters.SpaceNodeCount;
        double[,] solution = new double[timeNodeCount, spaceNodeCount];

        for (int spaceIndex = 0; spaceIndex < spaceNodeCount; spaceIndex++)
        {
            solution[0, spaceIndex] = 0d;
        }

        for (int timeIndex = 0; timeIndex < timeNodeCount - 1; timeIndex++)
        {
            var alpha = new double[spaceNodeCount];
            var beta = new double[spaceNodeCount];
            var a = new double[spaceNodeCount];
            var b = new double[spaceNodeCount];
            var c = new double[spaceNodeCount];
            var xi = new double[spaceNodeCount];

            alpha[0] = 0d;
            beta[0] = 0d;

            for (int spaceIndex = 1; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                a[spaceIndex] = A(parameters);
                b[spaceIndex] = B(parameters);
                c[spaceIndex] = C(parameters);
                xi[spaceIndex] = Xi(solution[timeIndex, spaceIndex], parameters, spaceIndex, timeIndex);

                alpha[spaceIndex] = Alpha(a[spaceIndex], b[spaceIndex], c[spaceIndex], alpha[spaceIndex - 1]);
                beta[spaceIndex] = Beta(
                    xi[spaceIndex],
                    b[spaceIndex],
                    c[spaceIndex],
                    alpha[spaceIndex - 1],
                    beta[spaceIndex - 1]);
            }

            solution[timeIndex + 1, spaceNodeCount - 1] =
                FiniteDifferenceFormula.RightBoundaryValue(timeIndex + 1, parameters);

            for (int spaceIndex = spaceNodeCount - 2; spaceIndex >= 0; spaceIndex--)
            {
                solution[timeIndex + 1, spaceIndex] =
                    (alpha[spaceIndex] * solution[timeIndex + 1, spaceIndex + 1]) + beta[spaceIndex];
            }
        }

        return new SimulationResult(parameters, Flatten(solution));
    }

    private static double A(GridParameters parameters) => -parameters.DiffusionRatio;

    private static double B(GridParameters parameters) => 1d + (2d * parameters.DiffusionRatio);

    private static double C(GridParameters parameters) => -parameters.DiffusionRatio;

    private static double Xi(double value, GridParameters parameters, int spaceIndex, int timeIndex) =>
        value + FiniteDifferenceFormula.SourceTerm(timeIndex, spaceIndex, parameters);

    private static double Alpha(double a, double b, double c, double previousAlpha) =>
        -a / (b + (c * previousAlpha));

    private static double Beta(double xi, double b, double c, double previousAlpha, double previousBeta) =>
        (xi - (c * previousBeta)) / (b + (c * previousAlpha));

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
