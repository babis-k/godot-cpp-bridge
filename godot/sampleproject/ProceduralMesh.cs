using Godot;
using System;
using System.Collections.Generic;
using GlmSharp;

public partial class ProceduralMesh : MeshInstance3D
{
    private RandomNumberGenerator rng = new();
    
    public override void _Ready()
    {
        OnRebuildMeshOld();
    }

    void OnRebuildMeshOld()
    {
        Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // C# arrays cannot be resized or expanded, so use Lists to create geometry.
        List<Vector3> verts = [];
        List<Vector2> uvs = [];
        List<Vector3> normals = [];
        
        float halfSize = 5.0f;
        verts.Add(new Vector3(-halfSize,rng.Randfn(),halfSize));
        verts.Add(new Vector3(halfSize,rng.Randfn(),halfSize));
        verts.Add(new Vector3(-halfSize,rng.Randfn(),-halfSize));
        verts.Add(new Vector3(halfSize,rng.Randfn(),-halfSize));
        uvs.Add(new Vector2(0,0));
        uvs.Add(new Vector2(1,0));
        uvs.Add(new Vector2(0,1));
        uvs.Add(new Vector2(1,1));
        for(int i=0;i<4;++i)
            normals.Add(new Vector3(0,1,0));
        //List<int> indices = [0, 1, 2, 3, 2, 1];
        List<int> indices = [0, 2, 1, 3, 1, 2];

        /***********************************
         * Insert code here to generate mesh.
         * *********************************/

        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        var arrMesh = Mesh as ArrayMesh;
        if (arrMesh != null)
        {
            arrMesh.ClearSurfaces();
            // Create mesh surface from mesh array
            // No blendshapes, lods, or compression used.
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }
    }

    void OnRebuildMesh()
    {
        Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // C# arrays cannot be resized or expanded, so use Lists to create geometry.
        int maxVerts = 1000;
        int maxTris = 1000;
        var verts = new Vector3[maxVerts];
        var uvs = new Vector2[maxVerts];
        var normals = new Vector3[maxVerts];
        var indices = new int[maxTris*3];

        NativePluginBindings.GenerateSomeGeometry(verts, normals, uvs, maxVerts, indices, maxTris);

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
