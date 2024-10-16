using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Wave : MonoBehaviour
{
    [SerializeField] private int dimension = 10;
    [SerializeField] private Octave[] octaves;

    private MeshFilter meshFilter;
    private Mesh mesh;

    NativeArray<float3> newVerts;
    NativeArray<Octave> nativeOctaves;

    void Start()
    {
        mesh = new Mesh();

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTriangles();
        mesh.uv = GenerateUVs();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        newVerts = new NativeArray<float3>(mesh.vertexCount, Allocator.Persistent);
        nativeOctaves = new NativeArray<Octave>(octaves, Allocator.Persistent);
    }

    private void OnDisable() 
    {
        newVerts.Dispose();    
        nativeOctaves.Dispose();
    }

    private int Idx(int x, int z)
    {
        return x * (dimension + 1) + z;
    }

    private int Idx(float x, float z)
    {
        return Idx((int)x, (int)z);
    }

    private Vector3[] GenerateVertices()
    {
        var verts = new Vector3[(dimension + 1) * (dimension + 1)];

        for(int x = 0; x <= dimension; x++)
            for(int z = 0; z <= dimension; z++)
                verts[Idx(x, z)] = new Vector3(x, 0, z);

        return verts;
    }

    private int[] GenerateTriangles()
    {
         var tries = new int[mesh.vertices.Length * 6];

        for(int x = 0; x < dimension; x++)
        {
            for(int z = 0; z < dimension; z++)
            {
                tries[Idx(x, z) * 6 + 0] = Idx(x, z);
                tries[Idx(x, z) * 6 + 1] = Idx(x + 1, z + 1);
                tries[Idx(x, z) * 6 + 2] = Idx(x + 1, z);
                tries[Idx(x, z) * 6 + 3] = Idx(x, z);
                tries[Idx(x, z) * 6 + 4] = Idx(x, z + 1);
                tries[Idx(x, z) * 6 + 5] = Idx(x + 1, z + 1);
            }
        }

        return tries;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[mesh.vertices.Length];

        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                var vec = new Vector2(x % 2, z % 2);
                uvs[Idx(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }



    public float GetHeight(Vector3 position)
    {
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale(position - transform.position, scale);

        // Get edge points
        Vector3 p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        Vector3 p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        Vector3 p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        Vector3 p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        // Clamp if the position is outside the plane
        p1.x = Mathf.Clamp(p1.x, 0, dimension);
        p1.z = Mathf.Clamp(p1.z, 0, dimension);
        p2.x = Mathf.Clamp(p2.x, 0, dimension);
        p2.z = Mathf.Clamp(p2.z, 0, dimension);
        p3.x = Mathf.Clamp(p3.x, 0, dimension);
        p3.z = Mathf.Clamp(p3.z, 0, dimension);
        p4.x = Mathf.Clamp(p4.x, 0, dimension);
        p4.z = Mathf.Clamp(p4.z, 0, dimension);

        var d1 = Vector3.Distance(localPos, p1);
        var d2 = Vector3.Distance(localPos, p2);
        var d3 = Vector3.Distance(localPos, p3);
        var d4 = Vector3.Distance(localPos, p4);

        var max = Mathf.Max(d1, d2, d3, d4) + Mathf.Epsilon;
        var dist = (max * 4) - (d1 + d2 + d3 + d4) + Mathf.Epsilon;

        var height = mesh.vertices[Idx(p1.x, p1.z)].y * (max - d1)
                   + mesh.vertices[Idx(p2.x, p2.z)].y * (max - d2)
                   + mesh.vertices[Idx(p3.x, p3.z)].y * (max - d3)
                   + mesh.vertices[Idx(p4.x, p4.z)].y * (max - d4);

        return height * transform.lossyScale.y / dist;
    }

    void FixedUpdate()
    {
        CalcWaveVertJob calcWaveVertJob = new CalcWaveVertJob()
        {
            dimension = dimension,
            time = Time.time,
            octaves = nativeOctaves,
            newVerts = newVerts
        };

        JobHandle calcWaveVertJobHandle = calcWaveVertJob.Schedule(mesh.vertexCount, 64);

        calcWaveVertJobHandle.Complete();

        mesh.SetVertices(calcWaveVertJob.newVerts);
        mesh.RecalculateNormals();
    }
}
