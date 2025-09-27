using Godot;
using System;
using System.Collections.Generic;
using GlmSharp;

public partial class ProceduralMesh : MeshInstance3D
{
    private RandomNumberGenerator rng;
    const int gridSize = 2048;
    private Vector3[] verts;
    private Vector2[] uvs;
    private Vector3[] normals;
    private int[] indices;
    private Godot.Collections.Array surfaceArray = [];

    private float xo = 0.0f;
    private float yo = 0.0f;
    public override void _Ready()
    {
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        
        int maxVerts = gridSize*gridSize;
        int maxTris = (gridSize-1)*(gridSize-1)*6;
        verts = new Vector3[maxVerts];
        uvs = new Vector2[maxVerts];
        normals = new Vector3[maxVerts];
        indices = new int[maxTris*3];
        
        OnRebuildMesh();
        
        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts;
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs;
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals;
        surfaceArray[(int)Mesh.ArrayType.Index] = indices;

        var arrMesh = Mesh as ArrayMesh;
        if (arrMesh != null)
        {
            if (arrMesh.GetSurfaceCount() == 0)
                arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        OnRebuildMesh();
    }

    void OnRebuildMesh()
    {
        NativePluginBindings.GenerateTerrain(verts, normals, uvs, indices, gridSize, 0,Godot.Time.GetTicksMsec() * 0.001f);
    }
}
