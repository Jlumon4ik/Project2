using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class World : MonoBehaviour
{
    int seed;
    public BiomAttributs biome;

    public Material material;
    public BlockType[] blocktypes;

    public Transform player;
    public Vector3 spawnPosition;

    Chunk[,] chunks = new Chunk[Voxel.WorldSizeInChunks, Voxel.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;


    private void Start()
    {
        UnityEngine.Random.InitState(seed);

        spawnPosition = new Vector3((Voxel.WorldSizeInChunks * Voxel.ChunkW) / 2f, Voxel.ChunkH + 2f, (Voxel.WorldSizeInChunks * Voxel.ChunkW) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistans();
    }

    void GenerateWorld()
    {
        for (int x = (Voxel.WorldSizeInChunks / 2) - Voxel.ViewDistanceInChunks; x < (Voxel.WorldSizeInChunks / 2) + Voxel.ViewDistanceInChunks; x++)
        {
            for (int z = (Voxel.WorldSizeInChunks / 2) - Voxel.ViewDistanceInChunks; z < (Voxel.WorldSizeInChunks / 2) + Voxel.ViewDistanceInChunks; z++)
            {
                CreatNewChunk(x, z);
            }
        }

        player.position = spawnPosition;
    }

    void CheckViewDistans()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - Voxel.ViewDistanceInChunks; x < coord.x + Voxel.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - Voxel.ViewDistanceInChunks; z < coord.z + Voxel.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld (x, z))
                {
                    if (chunks[x, z] == null)
                        CreatNewChunk(x, z);
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }
                for (int i  = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals (new ChunkCoord(x, z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        foreach(ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;
    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / Voxel.ChunkW);
        int z = Mathf.FloorToInt(pos.z / Voxel.ChunkW);
        return new ChunkCoord(x, z);
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (!IsVoxelInWorld(pos))
            return 0;

        if (yPos == 0)
            return 3;

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0; 

        if (yPos == terrainHeight)
            voxelValue = 1;
        else if (yPos < terrainHeight && yPos > terrainHeight - 7)
            voxelValue = 5;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        if (voxelValue == 2)
        {
            foreach(Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }
        return voxelValue;
    }


    void CreatNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld(int x, int z)
    {
        if (x > 0 && x < Voxel.WorldSizeInChunks - 1 && z > 0 && z < Voxel.WorldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld (Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < Voxel.WorldSizeInVoxels && pos.y >= 0 && pos.y < Voxel.ChunkH && pos.z >= 0 && pos.z < Voxel.WorldSizeInVoxels)
            return true;
        else return false;
    }

}



[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backTexture;
    public int frontTexture;
    public int topTexture;
    public int bottomTexture;
    public int leftTexture;
    public int rightTexture;

    public int GetTextureID (int faceIndex)
    {
        switch(faceIndex)
        {
            case 0:
                return backTexture;
            case 1:
                return frontTexture;
            case 2:
                return topTexture;
            case 3:
                return bottomTexture;
            case 4:
                return leftTexture;
            case 5:
                return rightTexture;
            default:
                Debug.Log("Error GetTextureID");
                return 0;
        }
    }
}
