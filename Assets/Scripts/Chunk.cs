using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk {

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    MeshCollider meshCollider;

    public ChunkCoord coord;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] voxelMap = new byte[Voxel.ChunkW,Voxel.ChunkH, Voxel.ChunkW];

    World world;

    public Chunk (ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent <MeshCollider>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * Voxel.ChunkW, 0, coord.z *  Voxel.ChunkW);
        chunkObject.name = "Chunk" + coord.x + ", " + coord.z;

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;
        
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < Voxel.ChunkH; y++)
        {
            for (int x = 0; x < Voxel.ChunkW; x++)
            {
                for (int z = 0; z < Voxel.ChunkW; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3 (x, y, z) + position);
                }
            }
        }
    }

    void CreateMeshData()
    {
        for (int y = 0; y < Voxel.ChunkH; y++)
        {
            for (int x = 0; x < Voxel.ChunkW; x++)
            {
                for (int z = 0; z < Voxel.ChunkW; z++)
                {
                    if (world.blocktypes[voxelMap[x, y, z]].isSolid)
                        AddVoxelToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > Voxel.ChunkW - 1 || y < 0 || y > Voxel.ChunkH - 1 || z < 0 || z > Voxel.ChunkW - 1)
            return false;
        else 
            return true;
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x,y,z))
            return world.blocktypes[world.GetVoxel(pos + position)].isSolid;

        return world.blocktypes[voxelMap[x, y, z]].isSolid;
    }

    void AddVoxelToChunk(Vector3 pos)
    {
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        for (int a = 0; a < 6; a++)
        {
            if (!CheckVoxel(pos + Voxel.faceCheck[a]))
            {
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(pos + Voxel.voxelVerts[Voxel.voxelTris[a, i]]);
                }

                AddTexture(world.blocktypes[blockID].GetTextureID(a));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;  
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / Voxel.TexturesAtlasSizeInBlocks;
        float x = textureID - (y * Voxel.TexturesAtlasSizeInBlocks);

        x *= Voxel.NormalizedBloxkTextureSize;
        y *= Voxel.NormalizedBloxkTextureSize;

        y = 1f - y - Voxel.NormalizedBloxkTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + Voxel.NormalizedBloxkTextureSize));
        uvs.Add(new Vector2(x + Voxel.NormalizedBloxkTextureSize, y));
        uvs.Add(new Vector2(x + Voxel.NormalizedBloxkTextureSize, y + Voxel.NormalizedBloxkTextureSize));
    }


}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null) return false;
        else if (other.x == x && other.z == z) return true;
        else return false;
    }
}

