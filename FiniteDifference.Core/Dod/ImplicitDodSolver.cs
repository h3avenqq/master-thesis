using System.Numerics;

namespace FiniteDifference.Core.Dod;

public static class ImplicitDodSolver
{
    public static SimulationResult SolveScalar(GridParameters parameters)
    {
        parameters.Validate();

        int timeNodeCount = parameters.TimeNodeCount;
        int spaceNodeCount = parameters.SpaceNodeCount;
        double ratio = parameters.DiffusionRatio;
        double a = -ratio;
        double b = 1d + (2d * ratio);
        double c = -ratio;
        double[] sourceTerms = FiniteDifferenceFormula.CreateSpaceSourceTerms(parameters);
        var values = new double[timeNodeCount * spaceNodeCount];
        var currentLayer = new double[spaceNodeCount];
        var nextLayer = new double[spaceNodeCount];
        var alpha = new double[spaceNodeCount];
        var beta = new double[spaceNodeCount];

        for (int timeIndex = 0; timeIndex < timeNodeCount - 1; timeIndex++)
        {
            alpha[0] = 0d;
            beta[0] = 0d;
            double timeBias = -2d * timeIndex * parameters.TimeStep;

            for (int spaceIndex = 1; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                double xi = currentLayer[spaceIndex] + (parameters.TimeStep * (sourceTerms[spaceIndex] + timeBias));
                double denominator = b + (c * alpha[spaceIndex - 1]);
                alpha[spaceIndex] = -a / denominator;
                beta[spaceIndex] = (xi - (c * beta[spaceIndex - 1])) / denominator;
            }

            nextLayer[spaceNodeCount - 1] = FiniteDifferenceFormula.RightBoundaryValue(timeIndex + 1, parameters);

            for (int spaceIndex = spaceNodeCount - 2; spaceIndex >= 0; spaceIndex--)
            {
                nextLayer[spaceIndex] = (alpha[spaceIndex] * nextLayer[spaceIndex + 1]) + beta[spaceIndex];
            }

            nextLayer.AsSpan().CopyTo(values.AsSpan((timeIndex + 1) * spaceNodeCount, spaceNodeCount));
            Swap(ref currentLayer, ref nextLayer);
        }

        return new SimulationResult(parameters, values);
    }

    public static SimulationResult SolveSimd(GridParameters parameters)
    {
        parameters.Validate();

        int timeNodeCount = parameters.TimeNodeCount;
        int spaceNodeCount = parameters.SpaceNodeCount;
        int vectorSize = Vector<double>.Count;
        double ratio = parameters.DiffusionRatio;
        double a = -ratio;
        double b = 1d + (2d * ratio);
        double c = -ratio;
        double[] sourceTerms = FiniteDifferenceFormula.CreateSpaceSourceTerms(parameters);
        var values = new double[timeNodeCount * spaceNodeCount];
        var currentLayer = new double[spaceNodeCount];
        var nextLayer = new double[spaceNodeCount];
        var alpha = new double[spaceNodeCount];
        var beta = new double[spaceNodeCount];
        var rhs = new double[spaceNodeCount];
        var timeStep = new Vector<double>(parameters.TimeStep);
        int lastVectorStart = spaceNodeCount - vectorSize - 1;

        for (int timeIndex = 0; timeIndex < timeNodeCount - 1; timeIndex++)
        {
            alpha[0] = 0d;
            beta[0] = 0d;
            double timeBias = -2d * timeIndex * parameters.TimeStep;
            var timeBiasVector = new Vector<double>(timeBias);
            int spaceIndex = 1;

            for (; spaceIndex <= lastVectorStart; spaceIndex += vectorSize)
            {
                var current = new Vector<double>(currentLayer, spaceIndex);
                var source = new Vector<double>(sourceTerms, spaceIndex);
                var result = current + (timeStep * (source + timeBiasVector));
                result.CopyTo(rhs, spaceIndex);
            }

            for (; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                rhs[spaceIndex] = currentLayer[spaceIndex] + (parameters.TimeStep * (sourceTerms[spaceIndex] + timeBias));
            }

            for (spaceIndex = 1; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                double denominator = b + (c * alpha[spaceIndex - 1]);
                alpha[spaceIndex] = -a / denominator;
                beta[spaceIndex] = (rhs[spaceIndex] - (c * beta[spaceIndex - 1])) / denominator;
            }

            nextLayer[spaceNodeCount - 1] = FiniteDifferenceFormula.RightBoundaryValue(timeIndex + 1, parameters);

            for (spaceIndex = spaceNodeCount - 2; spaceIndex >= 0; spaceIndex--)
            {
                nextLayer[spaceIndex] = (alpha[spaceIndex] * nextLayer[spaceIndex + 1]) + beta[spaceIndex];
            }

            nextLayer.AsSpan().CopyTo(values.AsSpan((timeIndex + 1) * spaceNodeCount, spaceNodeCount));
            Swap(ref currentLayer, ref nextLayer);
        }

        return new SimulationResult(parameters, values);
    }

    private static void Swap(ref double[] left, ref double[] right) => (left, right) = (right, left);
}
