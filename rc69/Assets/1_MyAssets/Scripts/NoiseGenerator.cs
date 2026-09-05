using UnityEngine;

/// <summary>
/// Noise Generator (Perlin Noise)
/// Hỗ trợ 2D & 3D
/// Có thể thay đổi scale, density, seed, octave, persistence, lacunarity
/// </summary>
public class NoiseGenerator
{
    public int seed = 0;

    [Header("Base Settings")]
    public float scale = 20f;          // Kích thước noise
    public float density = 0.5f;        // Ngưỡng dùng để spawn (0–1)

    [Header("Fractal Settings")]
    public int octaves = 4;
    public float persistence = 0.5f;    // Ảnh hưởng độ mạnh mỗi octave, 
    public float lacunarity = 2f;        // Tăng tần số mỗi octave

    private Vector2 offset2D;
    private Vector3 offset3D;

    public NoiseGenerator(int seed)
    {
        this.seed = seed;
        InitOffsets();
    }

    public NoiseGenerator(int seed, float scale, float density)
    {
        this.seed = seed;
        this.scale = Mathf.Max(0.0001f, scale);
        this.density = Mathf.Clamp01(density);
        InitOffsets();
    }

    private void InitOffsets()
    {
        System.Random prng = new System.Random(seed);

        offset2D = new Vector2(
            prng.Next(-100000, 100000),
            prng.Next(-100000, 100000));

        offset3D = new Vector3(
            prng.Next(-100000, 100000),
            prng.Next(-100000, 100000),
            prng.Next(-100000, 100000));
    }

    // =====================================================
    // 2D NOISE
    // =====================================================

    public float GetNoise2D(float x, float y)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + offset2D.x) / scale * frequency;
            float sampleY = (y + offset2D.y) / scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return Mathf.InverseLerp(-1, 1, noiseHeight);
    }

    /// <summary>
    /// Kiểm tra có spawn object hay không dựa trên density
    /// </summary>
    public bool IsSolid2D(float x, float y)
    {
        return GetNoise2D(x, y) > density;
    }

    // =====================================================
    // 3D NOISE (giả lập bằng Perlin 2D)
    // =====================================================

    public float GetNoise3D(float x, float y, float z)
    {
        float xy = GetNoise2D(x, y);
        float yz = GetNoise2D(y, z);
        float xz = GetNoise2D(x, z);

        float yx = GetNoise2D(y, x);
        float zy = GetNoise2D(z, y);
        float zx = GetNoise2D(z, x);

        return (xy + yz + xz + yx + zy + zx) / 6f;
    }

    public bool IsSolid3D(float x, float y, float z)
    {
        return GetNoise3D(x, y, z) > density;
    }
}
