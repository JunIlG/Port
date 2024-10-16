using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public struct CalcWaveVertJob : IJobParallelFor
{
    [ReadOnly] public int dimension;
    [ReadOnly] public float time;

    [ReadOnly] public NativeArray<Octave> octaves;

    [WriteOnly] public NativeArray<float3> newVerts;

    public void Execute(int index)
    {
        int x = index / (dimension + 1);
        int z = index % (dimension + 1);
        float y = 0f;

        for (int i = 0; i < octaves.Length; i++)
        {
            float perl = Mathf.PerlinNoise(x * octaves[i].scale.x / dimension, z * octaves[i].scale.y / dimension) * Mathf.PI * 2f;
            y += Mathf.Cos(perl + octaves[i].speed.magnitude * time) * octaves[i].height;
        }

        newVerts[index] = new float3(x, y, z);
    }
}

