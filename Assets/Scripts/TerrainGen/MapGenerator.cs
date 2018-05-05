﻿using UnityEngine;
using System;
using TerrainGenData;


//I think we can delete colormap?
public class MapGenerator : MonoBehaviour
{
    //[Tooltip("Set the name of the device to use.")]

    public DataForTerrain terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    public static WebcamTextureController webcamController;
    private static FaceDetection face;
    public GameObject Water;


    [Range(0, 1)]
    public float minGreyValue;
    [Range(0, 1)]
    public float noiseWeight;

    [Range(0, 6)]
    public int levelOfDetail;

    //need to create getters? need to make it so these values can't be changed once they're set in Start()    

    private int mapWidth;
    private int mapHeight;

    private int mapChunkWidth;
    private int mapChunkHeight;

    private int numChunkWidth;
    private int numChunkHeight;

    public int MapChunkWidth
    {
        get
        {
            return mapChunkWidth;
        }
    }

    public int MapChunkHeight
    {
        get
        {
            return mapChunkHeight;
        }
    }

    public int MapWidth
    {
        get
        {
            return mapWidth;
        }
    }

    public int MapHeight
    {
        get
        {
            return mapHeight;
        }
    }
    public int NumChunkWidth
    {
        get
        {
            return numChunkWidth;
        }
    }

    public int NumChunkHeight
    {
        get
        {
            return numChunkHeight;
        }
    }

    private float[,] chunkNoiseMap;
    private float[,] fullNoiseMap;
    private float[,] heightMap;

    private int test;

    private void Start()
    {       
        webcamController = gameObject.GetComponent(typeof(WebcamTextureController)) as WebcamTextureController;

        face = gameObject.GetComponent(typeof(FaceDetection)) as FaceDetection;

        webcamController.Initialize();

        InitializeMapSizes();

        InitializeChunkSizes();

        InitializeNumOfChunks();

        fullNoiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, noiseData.offset, noiseData.normalizeMode);

        textureData.ApplyToMaterial(terrainMaterial);        
    }

    private void InitializeMapSizes()
    {
        mapWidth = webcamController.WebcamWidth;
        mapHeight = webcamController.WebcamHeight;
    }

    private void InitializeChunkSizes()
    {
        mapChunkWidth = mapWidth;
        mapChunkHeight = mapHeight;

        while (mapChunkWidth > 250 || mapChunkHeight > 250)
        {
            if (mapChunkWidth % 2 != 0)
                Debug.Log("Width " + mapChunkWidth + " is not evenly divisble by 2");

            mapChunkWidth /= 2;

            if (mapChunkHeight % 2 != 0)
                Debug.Log("Height " + mapChunkHeight + " is not evenly divisble by 2");

            mapChunkHeight /= 2;
        }

        //I forget why, but Sebastion explains in a video that these variables need to be chunk size + 1
        mapChunkWidth++;
        mapChunkHeight++;        
    }

    private void InitializeNumOfChunks()
    {
        numChunkWidth = mapWidth / (mapChunkWidth - 1);
        numChunkHeight = mapHeight / (mapChunkHeight - 1);
    }

    public MeshData RequestMeshData(Vector2 chunkPosition)
    {
        heightMap = GenerateMapData(chunkPosition);

        return MeshGenerator.GenerateTerrainMesh(heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail);
    }

    float[,] GenerateMapData(Vector2 chunkPosition)
    {
        if (Application.isPlaying)
        {   
            chunkNoiseMap = NoiseGenerator.LerpNoiseMapWithTextureToNoiseChunk(face.FaceTexture, fullNoiseMap, noiseWeight, minGreyValue, mapChunkWidth, mapChunkHeight, chunkPosition);
        }

        else
        {
            chunkNoiseMap = NoiseGenerator.GenerateNoiseMap(1, 1, 1, 1, 1, 1, 1, new Vector2(1, 1), noiseData.normalizeMode);
            Debug.Log("Generate map data error");
        }

        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

        return chunkNoiseMap;
    }
    void OnValuesUpdate()
    {
        UpdateWaterHeight();
    }
    public void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);

        UpdateWaterHeight();
    }
    public void UpdateWaterHeight()
    {
        if (Water == null)
            Water = GameObject.Find("WaterProDaytime");

        float newHeight = textureData.layers[1].startHeight * terrainData.meshHeightMultiplier * terrainData.uniformScale;
        Vector3 curPos = Water.transform.position;
        Water.transform.position = new Vector3(curPos.x, newHeight, curPos.z);
    }

    public void UpdateFullNoiseMap()
    {
        if (noiseData.Updated)
        {
            fullNoiseMap = NoiseGenerator.GenerateNoiseMap(webcamController.WebcamTex.width, webcamController.WebcamTex.height, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, noiseData.offset, noiseData.normalizeMode);
            noiseData.Updated = false;
        }
    }

    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdate;
            terrainData.OnValuesUpdated += OnValuesUpdate;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdate;
            noiseData.OnValuesUpdated += OnValuesUpdate;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}