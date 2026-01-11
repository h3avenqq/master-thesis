using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<NBodyBenchmark>();
    }
}
[MemoryDiagnoser]
public class NBodyBenchmark
{
    [Params(1_000, 5_000, 10_000, 100_000, 1_000_000)]
    public int N;

    private float _dt;
    
    private List<Particle> _oopParticles;

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

        _oopParticles = new List<Particle>();
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

            _velX[i] = 0;
            _velY[i] = 0;
            _velZ[i] = 0;
            
            _oopParticles.Add(new Particle
            {
                Position = new Vector3(_posX[i], _posY[i], _posZ[i]),
                Velocity = new Vector3(_velX[i], _velY[i], _velZ[i])
            });
        }
    }
    
    [Benchmark(Baseline = true)]
    public void Scenario_OOP()
    {
        for (var i = 0; i < N; i++)
        {
            Vector3 acceleration = Vector3.Zero;
            var p1 = _oopParticles[i];

            // 1. Считаем сумму сил (Read intensive)
            for (var j = 0; j < N; j++)
            {
                if (i == j) continue;
                
                var p2 = _oopParticles[j];
                float dist = Vector3.Distance(p1.Position, p2.Position);
                
                // Простая модель гравитации/силы: F ~ 1/dist (упрощенно)
                // Vector3 уже оптимизирован аппаратно!
                acceleration += new Vector3(1f / dist); 
            }

            // 2. Один раз обновляем позицию (Write once)
            p1.Velocity += acceleration * _dt;
            p1.Position += p1.Velocity * _dt;
        }
    }
    
    [Benchmark]
    public void Scenario_DOP_Scalar()
    {
        // Берем спаны один раз перед циклом (Bound check elimination)
        var posX = _posX.AsSpan();
        var posY = _posY.AsSpan();
        var posZ = _posZ.AsSpan();
        
        var velX = _velX.AsSpan();
        var velY = _velY.AsSpan();
        var velZ = _velZ.AsSpan();
        
        for (var i = 0; i < N; i++)
        {
            float ax = 0, ay = 0, az = 0;
            float p1x = posX[i];
            float p1y = posY[i];
            float p1z = posZ[i];

            // 1. Внутренний цикл - самая горячая точка
            for (var j = 0; j < N; j++)
            {
                if (i == j) continue;

                // Ручная математика (быстрее чем Math.Pow)
                float dx = posX[j] - p1x;
                float dy = posY[j] - p1y;
                float dz = posZ[j] - p1z;
                
                // Квадрат расстояния
                float distSq = dx*dx + dy*dy + dz*dz;
                
                // Корень - самая дорогая операция
                // MathF.Sqrt быстрее чем Math.Sqrt (работает с float)
                float dist = MathF.Sqrt(distSq);
                
                // Сила
                float f = 1f / dist;

                // Накапливаем ускорение (пока скалярно)
                ax += f;
                ay += f;
                az += f;
            }
            
            // 2. Обновляем состояние (Запись в память 1 раз на частицу)
            velX[i] += ax * _dt;
            velY[i] += ay * _dt;
            velZ[i] += az * _dt;
            
            posX[i] += velX[i] * _dt;
            posY[i] += velY[i] * _dt;
            posZ[i] += velZ[i] * _dt;
        }
    }
}

public class Particle
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }

    public float CalculateForce(Particle other) => 1f / Vector3.Distance(Position, other.Position);
}