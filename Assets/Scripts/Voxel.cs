using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public static int ChunkW = 16;
    public static int ChunkH = 128;
    public static int WorldSizeInChunks = 50;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkW; }
    }

    public static int ViewDistanceInChunks = 5;

    public static int TexturesAtlasSizeInBlocks = 4;
    public static float NormalizedBloxkTextureSize
    {
        get { return 1f / (float)TexturesAtlasSizeInBlocks; }
    }

    public static Vector3[] voxelVerts = new Vector3[8]{

    new Vector3(0,0,0),
    new Vector3(1,0,0),
    new Vector3(1,1,0),
    new Vector3(0,1,0),
    new Vector3(0,0,1),
    new Vector3(1,0,1),
    new Vector3(1,1,1),
    new Vector3(0,1,1),

    };

    public static Vector3[] faceCheck = new Vector3[6]
    {
        new Vector3(0,0,-1),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(0,-1,0),
        new Vector3(-1,0,0),
        new Vector3(1,0,0),
    };

    public static int[,] voxelTris= new int[6,4]{

        {0, 3, 1, 2}, // Back 
	    {5, 6, 4, 7}, // Front 
	    {3, 7, 2, 6}, // Top 
	    {1, 5, 0, 4}, // Bottom 
	    {4, 7, 0, 3}, // Left 
	    {1, 2, 5, 6}, // Right 

    };

    public static readonly Vector2[] voxelUvs = new Vector2[6] {

        new Vector2 (0,0),
        new Vector2 (0,1),
        new Vector2 (1,0),
        new Vector2 (1,0),
        new Vector2 (0,1),
        new Vector2 (1,1),

    };
}
