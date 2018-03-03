﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColorTracking;
using UnityEngine;

public class TerrainController : MonoBehaviour
{


	public enum ImageMode
	{
		FromNoise,
		FromWebcam
	};

	public ImageMode imageMode;

	public int mapWidth;
	public int mapHeight;

	public float noiseScale;

	public int octaves;
	[Range(0, 1)] public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;


	//public Terrain _terrain;
	public int HeightMapResolution;
	
	WebCamTexture _webcamtex;
	Texture2D _TextureFromCamera;

	public string requestedDeviceName = null;

	public Texture2D[] MapTextures;
	
	private void Start()
	{
		
		if (imageMode == ImageMode.FromWebcam)
		{
			//_TextureFromCamera = new Texture2D(mapWidth, mapHeight);
	
			if (!String.IsNullOrEmpty(requestedDeviceName))
			{
				_webcamtex = new WebCamTexture(requestedDeviceName, mapWidth, mapHeight);
			}
	
			else
				_webcamtex = new WebCamTexture(mapWidth, mapHeight);
	
			_webcamtex.Play();        
		}  
		
		StartCoroutine(UpdateTerrain());
	
	}


	// Update is called once per frame
	public void GenerateTerrain()
	{
		// Get the attached terrain component
		Terrain terrain = GetComponent<Terrain>();
         
		// Get a reference to the terrain data
		TerrainData terrainData = terrain.terrainData;

		terrainData.heightmapResolution = HeightMapResolution;
		//terrainData.baseMapResolution = 1024;
		//terrainData.SetDetailResolution(1024,terrainData.detailResolution);

		terrainData.size = new Vector3(mapWidth,meshHeightMultiplier, mapHeight);
		//terrainData.SetAlphamaps();
		
		if (imageMode == ImageMode.FromNoise)
		{
			float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity,
				new Vector2(0, 0) + offset);


			terrainData.SetHeights(0, 0, noiseMap);


		} else if (imageMode == ImageMode.FromWebcam)
		{
			/*
			float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity,
				new Vector2(0, 0) + offset);
			*/
			float[,] heightMap = TextureGenerator.ConvertTextureToFloats(_webcamtex);
			terrainData.SetHeights(0, 0, heightMap);
		}



		//terrainData.
		
		//terrain.terrainData = ApplyTerrainTextures(terrainData);
		
		/*
		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
         
        for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
             {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y/(float)terrainData.alphamapHeight;
                float x_01 = (float)x/(float)terrainData.alphamapWidth;
                 
                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                        float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );
                 
                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01,x_01);
      
                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01,x_01);
                 
                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];
                 
                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
     
                // Texture[0] has constant influence
                //splatWeights[0] = 0.5f;
                 
                // Texture[1] is stronger at lower altitudes
                splatWeights[0] = Mathf.Clamp01((terrainData.heightmapHeight - height));
                 
                // Texture[2] stronger on flatter terrain
                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                // Subtract result from 1.0 to give greater weighting to flat surfaces
                splatWeights[1] = 1.0f - Mathf.Clamp01(steepness*steepness/(terrainData.heightmapHeight/5.0f));
                 
                // Texture[3] increases with height but only on surfaces facing positive Z axis 
                splatWeights[2] = height * Mathf.Clamp01(normal.z);
                 
                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();
                 
                // Loop through each terrain texture
                for(int i = 0; i<terrainData.alphamapLayers; i++){
                     
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;
                     
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
	            */
        
      
        // Finally assign the new splatmap to the terrainData:
        //terrainData.SetAlphamaps(0, 0, splatmapData);
		
		
		terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

	}

	IEnumerator UpdateTerrain()
	{

		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			if (imageMode == ImageMode.FromWebcam)
			{
				/*
				for (int y = 0; y < mapHeight; y++)
				{
					for (int x = 0; x < mapWidth; x++)
					{
						_TextureFromCamera.SetPixel(x, y, _webcamtex.GetPixel(x, y));
					}
				}
				_TextureFromCamera.Apply();
				*/
				GenerateTerrain();
			}
		}

	}


	TerrainData ApplyTerrainTextures(TerrainData terrainData)
	{
		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
         
        for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
             {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y/(float)terrainData.alphamapHeight;
                float x_01 = (float)x/(float)terrainData.alphamapWidth;
                 
                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                        float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );
                 
                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01,x_01);
      
                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01,x_01);
                 
                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];
                 
                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
     
                // Texture[0] has constant influence
                splatWeights[0] = 0.5f;
                 
                // Texture[1] is stronger at lower altitudes
                splatWeights[1] = Mathf.Clamp01((terrainData.heightmapHeight - height));
                 
                // Texture[2] stronger on flatter terrain
                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                // Subtract result from 1.0 to give greater weighting to flat surfaces
                splatWeights[2] = 1.0f - Mathf.Clamp01(steepness*steepness/(terrainData.heightmapHeight/5.0f));
                 
                // Texture[3] increases with height but only on surfaces facing positive Z axis 
                splatWeights[3] = height * Mathf.Clamp01(normal.z);
                 
                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();
                 
                // Loop through each terrain texture
                for(int i = 0; i<terrainData.alphamapLayers; i++){
                     
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;
                     
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
      
        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);


		return terrainData;
	
	}

}
