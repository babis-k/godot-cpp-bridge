using Godot;
using System;
using System.Collections.Generic;
using GlmSharp;

public partial class ProceduralMesh : MeshInstance3D
{
    private RandomNumberGenerator rng = new();
    
    public override void _Ready()
    {
        OnRebuildMesh();
    }

    void OnRebuildMesh()
    {
        Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        
        int gridSize = 128;
        int maxVerts = gridSize*gridSize;
        int maxTris = (gridSize-1)*(gridSize-1)*6;
        var verts = new Vector3[maxVerts];
        var uvs = new Vector2[maxVerts];
        var normals = new Vector3[maxVerts];
        var indices = new int[maxTris*3];

        NativePluginBindings.GenerateTerrain(verts, normals, uvs, indices, gridSize);

        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts;
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs;
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals;
        surfaceArray[(int)Mesh.ArrayType.Index] = indices;

        var arrMesh = Mesh as ArrayMesh;
        if (arrMesh != null)
        {
            arrMesh.ClearSurfaces();
            // Create mesh surface from mesh array
            // No blendshapes, lods, or compression used.
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }
        
    }
}
