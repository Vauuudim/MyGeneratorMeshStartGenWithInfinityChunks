using UnityEngine;
using System.Collections;

public static class MeshAndFloraGenerator
{
	public static MeshData GenerateTerrainMeshAndFlora(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading, int verticesPerLine, int borderedSize, int meshSimplificationIncrement, int[,] vertexIndicesMap, AnimationCurve heightCurve, float topLeftX, float topLeftZ, float meshSize, float meshSizeUnsimplified, ModelsOfFlora floraModels, float[,] noiseMapFlora, HeightOfFlora heightOfFlora, DensityOfFlora densityOfFlora, float[,] noiseMapForTypes, float[,] noiseMapForDensityOfFlora, float percentageOfForest, float uniformScale, SizeOfModels sizeOfModels, int maxBushes, FloraMaterials floraMaterials)
	{
		MeshData meshData = new MeshData(verticesPerLine, useFlatShading);

		GameObject inst_objParent = new GameObject("Flora");
		inst_objParent.transform.position = new Vector3(0, 0, 0);
		inst_objParent.isStatic = true;

		GameObject grass = new GameObject("Grass");
		inst_objParent.transform.position = new Vector3(0, 0, 0);
		grass.transform.SetParent(inst_objParent.transform);
		grass.AddComponent<MeshFilter>();
		grass.AddComponent<MeshRenderer>();
		grass.isStatic = true;
		int meshCombainCount = 0;
		int countMeshFilters = 70;
		MeshFilter[] meshFilters = new MeshFilter[countMeshFilters];

		for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
			{
				int vertexIndex = vertexIndicesMap[x, y];
				Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
				float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
				Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

				meshData.AddVertex(vertexPosition, percent, vertexIndex);

				if (x < borderedSize - 1 && y < borderedSize - 1)
				{
					int a = vertexIndicesMap[x, y];
					int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
					int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
					int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
					meshData.AddTriangle(a, d, c);
					meshData.AddTriangle(d, a, b);
				}

				//UNDER WATER
				if (floraModels.Underwater.Length != 0 && heightMap[x, y] <= heightOfFlora.Underwater && Random.Range(0, 100) <= densityOfFlora.Underwater * noiseMapForDensityOfFlora[x, y])
				{
					InstFlora(vertexPosition, floraModels.TreesT2WR[0 + Random.Range(0, floraModels.TreesT2WR.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
				}

				//OVER THE BEACH
				else if (heightMap[x, y] >= heightOfFlora.Beach && heightMap[x, y] <= heightOfFlora.Ground)
				{
					//GRASS
					if (floraModels.Grass.Length != 0 && Random.Range(1, 100) <= densityOfFlora.Grass)
					{
						GameObject inst_obj = InstFlora(vertexPosition, floraModels.Grass[0 + Random.Range(0, floraModels.Grass.Length)], uniformScale, sizeOfModels.Grass, grass);
						//Mesh of grass combining (FPS*2.5, Butches/4)
						meshFilters[meshCombainCount] = inst_obj.GetComponent<MeshFilter>();
						meshCombainCount++;
						if (meshCombainCount == countMeshFilters)
						{
							CombineInstance[] combine = new CombineInstance[meshCombainCount];
							for (int i = 0; i < meshCombainCount; i++)
							{
								combine[i].mesh = meshFilters[i].sharedMesh;
								combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
								meshFilters[i].gameObject.SetActive(false);
							}
							grass.GetComponent<MeshRenderer>().material = floraMaterials.floraMaterials.Grass;
							grass.transform.GetComponent<MeshFilter>().mesh = new Mesh();
							grass.transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
							grass.transform.gameObject.SetActive(true);

							grass = new GameObject("Grass");
							grass.AddComponent<MeshFilter>();
							grass.AddComponent<MeshRenderer>();
							inst_objParent.transform.position = new Vector3(0, 0, 0);
							grass.transform.SetParent(inst_objParent.transform);
							meshCombainCount = 0;
							grass.isStatic = true;
						}
						//T1
						if (floraModels.TreesT1.Length != 0 && Random.Range(1, 100) <= densityOfFlora.Ground * noiseMapForDensityOfFlora[x, y] && noiseMapForTypes[x, y] < 0.5f && noiseMapFlora[x, y] < percentageOfForest)
						{
							InstFlora(vertexPosition, floraModels.TreesT1[0 + Random.Range(0, floraModels.TreesT1.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
							for (int i = 0; i < Random.Range(1, maxBushes); i++)
							{
								if (floraModels.Bushes.Length != 0 && Random.Range(0, 100) <= densityOfFlora.Bushes)
								{
									InstFlora(vertexPosition, floraModels.Bushes[0 + Random.Range(0, floraModels.Bushes.Length)], uniformScale, sizeOfModels.Bushes, inst_objParent);
								}
							}
						}
						else if (floraModels.TreesT1R.Length != 0 && Random.Range(1, 100) <= noiseMapForDensityOfFlora[x, y] * densityOfFlora.Rare && noiseMapForTypes[x, y] < 0.5f && noiseMapFlora[x, y] < percentageOfForest)
						{
							InstFlora(vertexPosition, floraModels.TreesT1R[0 + Random.Range(0, floraModels.TreesT1R.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
						}
						//T2
						if (floraModels.TreesT2.Length != 0 && Random.Range(1, 100) <= densityOfFlora.Ground * noiseMapForDensityOfFlora[x, y] && noiseMapForTypes[x, y] > 0.5f && noiseMapFlora[x, y] < percentageOfForest)
						{
							InstFlora(vertexPosition, floraModels.TreesT2[0 + Random.Range(0, floraModels.TreesT2.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
							for (int i = 0; i < Random.Range(1, maxBushes); i++)
							{
								if (floraModels.Bushes.Length != 0 && Random.Range(0, 100) <= densityOfFlora.Bushes)
								{
									InstFlora(vertexPosition, floraModels.Bushes[0 + Random.Range(0, floraModels.Underwater.Length)], uniformScale, sizeOfModels.Bushes, inst_objParent);
								}
							}
						}
						else if (floraModels.TreesT2R.Length != 0 && Random.Range(1, 100) <= noiseMapForDensityOfFlora[x, y] * densityOfFlora.Rare && noiseMapForTypes[x, y] > 0.5f && noiseMapFlora[x, y] < percentageOfForest)
						{
							InstFlora(vertexPosition, floraModels.TreesT2R[0 + Random.Range(0, floraModels.TreesT2R.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
						}
					}
					//IN MOUNTAINS
					if (heightMap[x, y] >= heightOfFlora.Ground && heightMap[x, y] <= heightOfFlora.Mountain && noiseMapFlora[x, y] < percentageOfForest)
					{
						//T1W
						if (floraModels.TreesT1W.Length != 0 && Random.Range(1, 100) <= densityOfFlora.Mountain * noiseMapForDensityOfFlora[x, y] && noiseMapForTypes[x, y] <= 0.5f)
						{
							InstFlora(vertexPosition, floraModels.TreesT1W[0 + Random.Range(0, floraModels.TreesT1W.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
						}
						else
						if (floraModels.TreesT1WR.Length != 0 && Random.Range(1, 100) <= densityOfFlora.Ground * noiseMapForDensityOfFlora[x, y] * 0.01f * densityOfFlora.Rare)
						{
							InstFlora(vertexPosition, floraModels.TreesT1WR[0 + Random.Range(0, floraModels.TreesT1WR.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
						}
						//T2W
						if (floraModels.TreesT2W.Length != 0 && heightMap[x, y] >= heightOfFlora.Ground && heightMap[x, y] <= heightOfFlora.Mountain && Random.Range(1, 100) <= densityOfFlora.Mountain * noiseMapForDensityOfFlora[x, y] && noiseMapForTypes[x, y] > 0.5f)
						{
							InstFlora(vertexPosition, floraModels.TreesT2W[0 + Random.Range(0, floraModels.TreesT2W.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
						}
						else
						if (floraModels.TreesT2WR.Length != 0 && Random.Range(1, 100) <= densityOfFlora.Ground * noiseMapForDensityOfFlora[x, y] * 0.01f * densityOfFlora.Rare)
						{
							InstFlora(vertexPosition, floraModels.TreesT2WR[0 + Random.Range(0, floraModels.TreesT2WR.Length)], uniformScale, sizeOfModels.Trees, inst_objParent);
						}
					}
				}
				vertexIndex++;
			}
		}
		meshData.ProcessMesh();
		return meshData;
	}

	public static GameObject InstFlora(Vector3 vertexPosition, GameObject floraModel, float uniformScale, float sizeModel, GameObject parent)
	{
		Vector3 koords = (vertexPosition + new Vector3(Random.Range(0f, 0.4f), 0, Random.Range(0f, 0.4f))) * uniformScale;
		GameObject inst_obj = GameObject.Instantiate(floraModel, koords, Quaternion.Euler(Random.Range(-4, 5), Random.Range(-360, 360), Random.Range(-4, 5)));
		inst_obj.transform.localScale = new Vector3(sizeModel, sizeModel, sizeModel) * Random.Range(0.8f, 1.2f) * uniformScale;
		inst_obj.transform.SetParent(parent.transform);
		return inst_obj;
	}
}
