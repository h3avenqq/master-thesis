using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace NBody;

class Program
{
    static void Main(string[] args) => BenchmarkRunner.Run<NBodyBenchmark>();
}

[MemoryDiagnoser]
public class NBodyBenchmark
{
    [Params(1_000, 10_000, 100_000, 1_000_000)] 
    public int N;

    private float _dt;
    private Particle[] _oopParticles;

    // Данные для DOP (Structure of Arrays)
    private float[] _posX;
    private float[] _posY;
    private float[] _posZ;
    private float[] _velX;
    private float[] _velY;
    private float[] _velZ;

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);
        _dt = 0.01f;

        _oopParticles = new Particle[N];
        _posX = new float[N];
        _posY = new float[N];
        _posZ = new float[N];
        _velX = new float[N];
        _velY = new float[N];
        _velZ = new float[N];

        for (var i = 0; i < N; i++)
        {
            _posX[i] = rand.NextSingle();
            _posY[i] = rand.NextSingle();
            _posZ[i] = rand.NextSingle();

            _oopParticles[i] = new Particle
            {
                Position = new Vector3(_posX[i], _posY[i], _posZ[i]),
                Velocity = Vector3.Zero
            };
        }
    }

    [Benchmark(Baseline = true)]
    public void Scenario_OOP()
    {
        float dt = _dt;
        for (var i = 0; i < _oopParticles.Length; i++)
        {
            Vector3 acceleration = Vector3.Zero;
            var p1 = _oopParticles[i];

            for (var j = 0; j < _oopParticles.Length; j++)
            {
                if (i == j) continue;
                
                var p2 = _oopParticles[j];
                float dist = Vector3.Distance(p1.Position, p2.Position);
                acceleration += new Vector3(1f / (dist + 0.000001f)); 
            }

            p1.Velocity += acceleration * dt;
            p1.Position += p1.Velocity * dt;
        }
    }

    [Benchmark]
    public void Scenario_DOP_Scalar()
    {
        float dt = _dt;
        var pX = _posX; var pY = _posY; var pZ = _posZ;
        var vX = _velX; var vY = _velY; var vZ = _velZ;

        for (int i = 0; i < N; i++)
        {
            float ax = 0, ay = 0, az = 0;
            float iX = pX[i], iY = pY[i], iZ = pZ[i];

            for (int j = 0; j < N; j++)
            {
                if (i == j) continue;

                float dx = pX[j] - iX;
                float dy = pY[j] - iY;
                float dz = pZ[j] - iZ;
                
                float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz + 0.000001f);
                float f = 1f / dist;

                ax += f; ay += f; az += f;
            }

            vX[i] += ax * dt; vY[i] += ay * dt; vZ[i] += az * dt;
            pX[i] += vX[i] * dt; pY[i] += vY[i] * dt; pZ[i] += vZ[i] * dt;
        }
    }

    [Benchmark]
    public void Scenario_DOP_SIMD()
    {
        float dt = _dt;
        int vectorSize = Vector<float>.Count; // 8 для AVX2, 4 для SSE
        float[] pX = _posX; float[] pY = _posY; float[] pZ = _posZ;
        float[] vX = _velX; float[] vY = _velY; float[] vZ = _velZ;

        Vector<float> epsilon = new Vector<float>(0.000001f);

        for (int i = 0; i < N; i++)
        {
            Vector<float> accX = Vector<float>.Zero;
            Vector<float> accY = Vector<float>.Zero;
            Vector<float> accZ = Vector<float>.Zero;

            Vector<float> iX = new Vector<float>(pX[i]);
            Vector<float> iY = new Vector<float>(pY[i]);
            Vector<float> iZ = new Vector<float>(pZ[i]);

            int j = 0;
            // Внутренний цикл обрабатывает сразу по 8 частиц
            for (; j <= N - vectorSize; j += vectorSize)
            {
                Vector<float> jX = new Vector<float>(pX, j);
                Vector<float> jY = new Vector<float>(pY, j);
                Vector<float> jZ = new Vector<float>(pZ, j);

                Vector<float> dx = jX - iX;
                Vector<float> dy = jY - iY;
                Vector<float> dz = jZ - iZ;

                Vector<float> distSq = (dx * dx) + (dy * dy) + (dz * dz) + epsilon;
                Vector<float> dist = Vector.SquareRoot(distSq);
                
                // 1.0f / dist
                Vector<float> f = Vector<float>.One / dist;

                accX += f; accY += f; accZ += f;
            }

            // Горизонтальное суммирование накопленного ускорения
            float totalAx = Vector.Sum(accX);
            float totalAy = Vector.Sum(accY);
            float totalAz = Vector.Sum(accZ);

            // Обработка остатка, если N не кратно vectorSize
            for (; j < N; j++)
            {
                if (i == j) continue;
                float dx = pX[j] - pX[i];
                float dy = pY[j] - pY[i];
                float dz = pZ[j] - pZ[i];
                float f = 1f / (MathF.Sqrt(dx * dx + dy * dy + dz * dz) + 0.000001f);
                totalAx += f; totalAy += f; totalAz += f;
            }

            vX[i] += totalAx * dt; vY[i] += totalAy * dt; vZ[i] += totalAz * dt;
            pX[i] += vX[i] * dt; pY[i] += vY[i] * dt; pZ[i] += vZ[i] * dt;
        }
    }
}

public class Particle
{
    public Vector3 Position;
    public Vector3 Velocity;
}