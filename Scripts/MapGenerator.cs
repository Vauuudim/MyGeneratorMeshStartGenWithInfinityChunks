using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, Mesh, MeshAndFlora};
	public DrawMode drawMode;
	public readonly int[] mapChunkSize = { 47, 71, 95, 119, 143, 167, 191, 215, 239 };
	[Range(0, 8)]
	public int chunkSizeIndex = 0;
	[Range(0, 6)]
	public int levelOfDetail;

	public NoiseData noiseData;
	public FloraData floraData;
	public TextureData textureData;
	public FloraModels floraModels;
	public FloraMaterials floraMaterials;
	public Material terrainMaterial;
	public bool randomNoiseMesh;
	public bool randomNoiseFlora;
	public bool randomNoiseForTypesFlora;
	public bool randomNoiseForDensityOfFlora;
	public bool autoUpdate;
	void OnValuesUpdated()
	{
		if (!Application.isPlaying)
		{
			DrawMapInEditor ();
		}
	}

	void OnTextureValuesUpdated()
	{
		textureData.ApplyToMaterial (terrainMaterial);
	}

	public void DrawMapInEditor()
	{
		MapData mapData = GenerateMapData (Vector2.zero);
		MapDisplay display = FindObjectOfType<MapDisplay> ();

		AnimationCurve heightCurve = new AnimationCurve(noiseData.meshHeightCurve.keys);
		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
		int borderedSize = mapData.heightMap.GetLength(0);
		int meshSize = borderedSize - 2 * meshSimplificationIncrement;
		int meshSizeUnsimplified = borderedSize - 2;
		float topLeftX = (meshSizeUnsimplified - 1) / -2f;
		float topLeftZ = (meshSizeUnsimplified - 1) / 2f;
		int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;
		int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
		int meshVertexIndex = 0;
		int borderVertexIndex = -1;
		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
			{
				bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

				if (isBorderVertex)
				{
					vertexIndicesMap[x, y] = borderVertexIndex;
					borderVertexIndex--;
				}
				else
				{
					vertexIndicesMap[x, y] = meshVertexIndex;
					meshVertexIndex++;
				}
			}
		}

		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
		}
		else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh(mapData.heightMap, noiseData.meshHeightMultiplier, noiseData.useFlatShading, verticesPerLine, borderedSize, meshSimplificationIncrement, vertexIndicesMap, heightCurve, topLeftX, topLeftZ, meshSize, meshSizeUnsimplified));
		}
		else if (drawMode == DrawMode.MeshAndFlora)
		{
			display.DrawMesh(MeshAndFloraGenerator.GenerateTerrainMeshAndFlora(mapData.heightMap, noiseData.meshHeightMultiplier, noiseData.meshHeightCurve, levelOfDetail, noiseData.useFlatShading, verticesPerLine, borderedSize, meshSimplificationIncrement, vertexIndicesMap, heightCurve, topLeftX, topLeftZ, meshSize, meshSizeUnsimplified, floraModels.floraModels, mapData.noiseMapFlora, floraData.heightOfFlora, floraData.densityOfFlora, mapData.noiseMapForTypes, mapData.noiseMapForDensityOfFlora, floraData.percentageOfForest, noiseData.uniformScale, floraData.sizeOfModels, floraData.maxBushes, floraMaterials));
		}
	}

	public MapData GenerateMapData(Vector2 centre)
	{
		if (randomNoiseMesh == true)
		{
			noiseData.seed = UnityEngine.Random.Range(1, 999);
		}
		if (randomNoiseFlora == true)
		{
			floraData.seedFlora = noiseData.seed + UnityEngine.Random.Range(1, 999);
		}
		if (randomNoiseForTypesFlora == true)
		{
			floraData.seedForTypes = noiseData.seed + UnityEngine.Random.Range(-999, -1);
		}
		if (randomNoiseForDensityOfFlora == true)
		{
			floraData.seedForDensityOfFlora = floraData.seedFlora + UnityEngine.Random.Range(1, 999);
		}
		float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize[chunkSizeIndex] + 2, mapChunkSize[chunkSizeIndex] + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
		float[,] noiseMapFlora = Noise.GenerateNoiseMap(mapChunkSize[chunkSizeIndex] + 2, mapChunkSize[chunkSizeIndex] + 2, floraData.seedFlora, floraData.noiseScale.Flora, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
		float[,] noiseMapForTypes = Noise.GenerateNoiseMap(mapChunkSize[chunkSizeIndex] + 2, mapChunkSize[chunkSizeIndex] + 2, floraData.seedForTypes, floraData.noiseScale.Types, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
		float[,] noiseMapForDensityOfFlora = Noise.GenerateNoiseMap(mapChunkSize[chunkSizeIndex] + 2, mapChunkSize[chunkSizeIndex] + 2, floraData.seedForDensityOfFlora, floraData.noiseScale.DensityOfFlora, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
		textureData.UpdateMeshHeights (terrainMaterial, noiseData.minHeight, noiseData.maxHeight);

		return new MapData (noiseMap, noiseMapFlora, noiseMapForTypes, noiseMapForDensityOfFlora);
	}

	void OnValidate()
	{
		if (noiseData != null)
		{
			noiseData.OnValuesUpdated -= OnValuesUpdated;
			noiseData.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureData != null)
		{
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
	}
}

public struct MapData
{
	public readonly float[,] heightMap;
	public readonly float[,] noiseMapFlora;
	public readonly float[,] noiseMapForTypes;
	public readonly float[,] noiseMapForDensityOfFlora;
	public MapData (float[,] heightMap, float[,] noiseMapFlora, float[,] noiseMapForTypes, float[,] noiseMapForDensityOfFlora)
	{
		this.heightMap = heightMap;
		this.noiseMapFlora = noiseMapFlora;
		this.noiseMapForTypes = noiseMapForTypes;
		this.noiseMapForDensityOfFlora = noiseMapForDensityOfFlora;
	}
}
