using System.Numerics;

namespace FiniteDifference.Core.Dod;

public static class ExplicitDodSolver
{
    public static SimulationResult SolveScalar(GridParameters parameters)
    {
        parameters.Validate();

        int timeNodeCount = parameters.TimeNodeCount;
        int spaceNodeCount = parameters.SpaceNodeCount;
        double ratio = parameters.DiffusionRatio;
        double timeStep = parameters.TimeStep;
        double[] sourceTerms = FiniteDifferenceFormula.CreateSpaceSourceTerms(parameters);
        var values = new double[timeNodeCount * spaceNodeCount];
        var currentLayer = new double[spaceNodeCount];
        var nextLayer = new double[spaceNodeCount];

        for (int timeIndex = 0; timeIndex < timeNodeCount - 1; timeIndex++)
        {
            double timeBias = -2d * timeIndex * timeStep;

            for (int spaceIndex = 1; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                nextLayer[spaceIndex] = currentLayer[spaceIndex]
                    + (ratio * (currentLayer[spaceIndex + 1] - (2d * currentLayer[spaceIndex]) + currentLayer[spaceIndex - 1]))
                    + (timeStep * (sourceTerms[spaceIndex] + timeBias));
            }

            nextLayer[0] = FiniteDifferenceFormula.LeftBoundaryValue();
            nextLayer[spaceNodeCount - 1] = FiniteDifferenceFormula.RightBoundaryValue(timeIndex + 1, parameters);

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
        double[] sourceTerms = FiniteDifferenceFormula.CreateSpaceSourceTerms(parameters);
        var values = new double[timeNodeCount * spaceNodeCount];
        var currentLayer = new double[spaceNodeCount];
        var nextLayer = new double[spaceNodeCount];
        var ratio = new Vector<double>(parameters.DiffusionRatio);
        var doubleCenter = new Vector<double>(2d);
        var timeStep = new Vector<double>(parameters.TimeStep);
        int lastVectorStart = spaceNodeCount - vectorSize - 1;

        for (int timeIndex = 0; timeIndex < timeNodeCount - 1; timeIndex++)
        {
            double timeBias = -2d * timeIndex * parameters.TimeStep;
            var timeBiasVector = new Vector<double>(timeBias);
            int spaceIndex = 1;

            for (; spaceIndex <= lastVectorStart; spaceIndex += vectorSize)
            {
                var left = new Vector<double>(currentLayer, spaceIndex - 1);
                var center = new Vector<double>(currentLayer, spaceIndex);
                var right = new Vector<double>(currentLayer, spaceIndex + 1);
                var source = new Vector<double>(sourceTerms, spaceIndex);

                var result = center
                    + (ratio * (right - (doubleCenter * center) + left))
                    + (timeStep * (source + timeBiasVector));

                result.CopyTo(nextLayer, spaceIndex);
            }

            for (; spaceIndex < spaceNodeCount - 1; spaceIndex++)
            {
                nextLayer[spaceIndex] = currentLayer[spaceIndex]
                    + (parameters.DiffusionRatio
                        * (currentLayer[spaceIndex + 1] - (2d * currentLayer[spaceIndex]) + currentLayer[spaceIndex - 1]))
                    + (parameters.TimeStep * (sourceTerms[spaceIndex] + timeBias));
            }

            nextLayer[0] = FiniteDifferenceFormula.LeftBoundaryValue();
            nextLayer[spaceNodeCount - 1] = FiniteDifferenceFormula.RightBoundaryValue(timeIndex + 1, parameters);

            nextLayer.AsSpan().CopyTo(values.AsSpan((timeIndex + 1) * spaceNodeCount, spaceNodeCount));
            Swap(ref currentLayer, ref nextLayer);
        }

        return new SimulationResult(parameters, values);
    }

    private static void Swap(ref double[] left, ref double[] right) => (left, right) = (right, left);
}
