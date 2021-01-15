using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class FloraData : UpdatableData
{
	public int seedFlora;
	public int seedForTypes;
	public int seedForDensityOfFlora;

	public NoiseScale noiseScale;

	[Range(0, 1)]
	public float percentageOfForest = 0.1f;
	[Range(0, 10)]
	public int maxBushes = 1;

	public HeightOfFlora heightOfFlora;
	public DensityOfFlora densityOfFlora;
	public ModelsOfFlora floraModels;
	public SizeOfModels sizeOfModels;
}

[System.Serializable]
public struct NoiseScale
{
	[Range(1, 500)]
	public int Flora;
	[Range(1, 500)]
	public int Types;
	[Range(1, 500)]
	public int DensityOfFlora;
}

[System.Serializable]
public struct HeightOfFlora
{
	[Range(0f, 1f)]
	public float Underwater;
	[Range(0f, 1f)]
	public float Beach;
	[Range(0f, 1f)]
	public float Ground;
	[Range(0f, 1f)]
	public float Mountain;
}

[System.Serializable]
public struct DensityOfFlora
{
	[Range(0, 100)]
	public float Underwater;
	[Range(0, 100)]
	public float Ground;
	[Range(0, 100)]
	public float Mountain;
	[Range(0, 100)]
	public float Grass;
	[Range(0, 100)]
	public float Bushes;
	[Range(0, 100)]
	public float Rare;
}


[System.Serializable]
public struct ModelsOfFlora
{
	public GameObject[] Underwater;	//���������
	public GameObject[] TreesT1;	//������� ���� 1
	public GameObject[] TreesT1R;	//������� ���� 1 ������
	public GameObject[] TreesT2;	//������� ���� 2
	public GameObject[] TreesT2R;	//������� ���� 2 ������
	public GameObject[] TreesT1W;	//������� ���� 1 ������
	public GameObject[] TreesT1WR;	//������� ���� 1 ������ ������
	public GameObject[] TreesT2W;	//������� ���� 2 ������
	public GameObject[] TreesT2WR;	//������� ���� 2 ������ ������
	public GameObject[] Grass;		//�����
	public GameObject[] Bushes;		//�����
}

[System.Serializable]
public struct SizeOfModels
{
	[Range(0.1f, 5f)]
	public float Seaweed;
	[Range(0.1f, 5f)]
	public float Trees;
	[Range(0.1f, 5f)]
	public float Grass;
	[Range(0.1f, 5f)]
	public float Buches;
}