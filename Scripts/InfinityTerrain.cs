using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfinityTerrain : MonoBehaviour
{
	public enum DrawMode {Mesh, MeshAndFlora };
	public DrawMode drawMode;
	public readonly int[] chunkSize = { 47, 71, 95, 119, 143, 167, 191, 215, 239 };
	[Range(0, 8)]
	public int chunkSizeIndex = 0;
	int levelOfDetail = 0;

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

	[Range(0, 1000)]
	public float viewDistance = 100;
	public Transform viewer;
	public bool water;
	public GameObject waterObject;

	public static Vector2 viewerPosition;
	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
	GameObject chunksParent;

	void Start()
	{
		//textureData.ApplyToMaterial(terrainMaterial);
		chunksParent = new GameObject("Chunks");
		chunksParent.transform.position = new Vector3(0, 0, 0);
		chunksParent.isStatic = true;

		if (randomNoiseMesh == true)
		{
			noiseData.seed = UnityEngine.Random.Range(1, 999);
		}
		if (randomNoiseFlora == true)
		{
			floraData.seedFlora = UnityEngine.Random.Range(1, 999);
		}
		if (randomNoiseForTypesFlora == true)
		{
			floraData.seedForTypes = UnityEngine.Random.Range(1, 999);
		}
		if (randomNoiseForDensityOfFlora == true)
		{
			floraData.seedForDensityOfFlora = UnityEngine.Random.Range(1, 999);
		}
		if (noiseData.seed == floraData.seedFlora)
			floraData.seedFlora = noiseData.seed + UnityEngine.Random.Range(1, 999);
		if (floraData.seedFlora == floraData.seedForTypes)
			floraData.seedForTypes = floraData.seedFlora + UnityEngine.Random.Range(1, 999);

		chunksVisibleInViewDst = Mathf.RoundToInt(viewDistance / chunkSize[chunkSizeIndex]); //кол-во чанков вокруг viewer
	}

	void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
	}

	void UpdateVisibleChunks()
	{

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
		{
			terrainChunksVisibleLastUpdate[i].SetVisible(false);
		}
		terrainChunksVisibleLastUpdate.Clear();

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize[chunkSizeIndex]);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize[chunkSizeIndex]);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
					{
						terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
					}
				}
				else
				{
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize[chunkSizeIndex], transform, noiseData, floraData, textureData, terrainMaterial, levelOfDetail, chunksParent, viewDistance, drawMode, water, waterObject,floraModels, floraMaterials)) ;
				}

			}
		}
	}

	public class TerrainChunk
	{
		GameObject chunk = new GameObject("Chunk");
		Vector2 position;
		Bounds bounds;

		NoiseData noiseData;
		FloraData floraData;
		TextureData textureData;
		Material terrainMaterial;
		int chunkSize;
		float maxViewDst;
		public TerrainChunk(Vector2 coord, int size, Transform parent, NoiseData noiseData, FloraData floraData, TextureData textureData, Material terrainMaterial, int levelOfDetail, GameObject chunks, float maxViewDst, DrawMode drawMode, bool water, GameObject waterObject, FloraModels floraModels, FloraMaterials floraMaterials)
		{
			this.noiseData = noiseData;
			this.floraData = floraData;
			this.textureData = textureData;
			this.terrainMaterial = terrainMaterial;
			this.chunkSize = size;
			this.maxViewDst = maxViewDst;

			GameObject meshObject;
			meshObject = new GameObject("Mesh");
			meshObject.isStatic = true;
			GameObject flora;
			chunk.isStatic = true;

			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);

			MapData mapData = GenerateMapData(new Vector2(position.x, position.y));
			//MapDisplay display = FindObjectOfType<MapDisplay>();
			MeshData meshData;

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

			if (drawMode == DrawMode.Mesh)
			{
				meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, noiseData.meshHeightMultiplier, noiseData.useFlatShading, verticesPerLine, borderedSize, meshSimplificationIncrement, vertexIndicesMap, heightCurve, topLeftX, topLeftZ, meshSize, meshSizeUnsimplified);
				if (water == true)
				{
					GameObject waterPlane = GameObject.Instantiate(waterObject, new Vector3(0, 10.5f, 0), Quaternion.identity) as GameObject;
					waterPlane.transform.localScale = new Vector3(0.01f * size, 1, 0.01f * size);
					waterPlane.transform.SetParent(chunk.transform);
					waterPlane.isStatic = true;
				}
			}
			else
			{
				meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, noiseData.meshHeightMultiplier, noiseData.useFlatShading, verticesPerLine, borderedSize, meshSimplificationIncrement, vertexIndicesMap, heightCurve, topLeftX, topLeftZ, meshSize, meshSizeUnsimplified);
				flora = FloraGenerator.GenerateFlora(mapData.heightMap, noiseData.meshHeightMultiplier, heightCurve, levelOfDetail, noiseData.useFlatShading, borderedSize, meshSimplificationIncrement, vertexIndicesMap, topLeftX, topLeftZ, meshSize, meshSizeUnsimplified, floraModels.floraModels, mapData.noiseMapFlora, floraData.heightOfFlora, floraData.densityOfFlora, mapData.noiseMapForTypes, mapData.noiseMapForDensityOfFlora, floraData.percentageOfForest, noiseData.uniformScale, floraData.sizeOfModels, floraData.maxBushes, floraMaterials);
				flora.transform.SetParent(chunk.transform);
				flora.isStatic = true;
				if (water == true)
				{
					GameObject waterPlane = GameObject.Instantiate(waterObject, new Vector3(0, 10.5f, 0), Quaternion.identity) as GameObject;
					waterPlane.transform.localScale = new Vector3(0.01f * size, 1, 0.01f * size);
					waterPlane.transform.SetParent(chunk.transform);
					waterPlane.isStatic = true;
				}
			}
			meshObject.transform.SetParent(chunk.transform);
			MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
			MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
			MeshCollider meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = terrainMaterial;
			meshFilter.sharedMesh = meshData.CreateMesh();
			meshCollider.sharedMesh = meshFilter.sharedMesh;
			meshFilter.transform.localScale = Vector3.one * noiseData.uniformScale;
			chunk.transform.SetParent(chunks.transform);
			chunk.transform.position = new Vector3(position.x, 0, position.y);

			SetVisible(false);
		}

		public void UpdateTerrainChunk()
		{
			float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			bool visible;
			if (viewerDstFromNearestEdge <= maxViewDst)
			{
				visible = true;
			}
			else
            {
				visible = false;
            }
			SetVisible(visible);
		}

		public void SetVisible(bool visible)
		{
			chunk.SetActive(visible);
		}

		public bool IsVisible()
		{
			return chunk.activeSelf;
		}
		MapData GenerateMapData(Vector2 centre)
		{
			float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize + 3, chunkSize + 3, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
			float[,] noiseMapFlora = Noise.GenerateNoiseMap(chunkSize + 3, chunkSize + 3, floraData.seedFlora, floraData.noiseScale.Flora, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
			float[,] noiseMapForTypes = Noise.GenerateNoiseMap(chunkSize + 3, chunkSize + 3, floraData.seedForTypes, floraData.noiseScale.Types, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
			float[,] noiseMapForDensityOfFlora = Noise.GenerateNoiseMap(chunkSize + 3, chunkSize + 3, floraData.seedForDensityOfFlora, floraData.noiseScale.DensityOfFlora, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode, noiseData.falloff, noiseData.sizeFalloff, noiseData.multy);
			textureData.UpdateMeshHeights(terrainMaterial, noiseData.minHeight, noiseData.maxHeight);

			return new MapData(noiseMap, noiseMapFlora, noiseMapForTypes, noiseMapForDensityOfFlora);
		}
		struct MapData
		{
			public readonly float[,] heightMap;
			public readonly float[,] noiseMapFlora;
			public readonly float[,] noiseMapForTypes;
			public readonly float[,] noiseMapForDensityOfFlora;
			public MapData(float[,] heightMap, float[,] noiseMapFlora, float[,] noiseMapForTypes, float[,] noiseMapForDensityOfFlora)
			{
				this.heightMap = heightMap;
				this.noiseMapFlora = noiseMapFlora;
				this.noiseMapForTypes = noiseMapForTypes;
				this.noiseMapForDensityOfFlora = noiseMapForDensityOfFlora;
			}
		}
	}
}
