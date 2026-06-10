namespace FiniteDifference.Core;

public readonly record struct GridParameters(double TimeStep, double SpaceStep)
{
    public int TimeNodeCount => (int)(1d / TimeStep) + 1;

    public int SpaceNodeCount => (int)(1d / SpaceStep) + 1;

    public double DiffusionRatio => TimeStep / (SpaceStep * SpaceStep);

    public void Validate()
    {
        if (TimeStep <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(TimeStep), TimeStep, "Time step must be positive.");
        }

        if (SpaceStep <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(SpaceStep), SpaceStep, "Space step must be positive.");
        }
    }
}
