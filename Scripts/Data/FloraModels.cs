using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class FloraModels : UpdatableData
{
	public ModelsOfFlora floraModels;
}

[System.Serializable]
public struct ModelsOfFlora
{
	public GameObject[] Underwater; //���������
	public GameObject[] TreesT1;    //������� ���� 1
	public GameObject[] TreesT1R;   //������� ���� 1 ������
	public GameObject[] TreesT2;    //������� ���� 2
	public GameObject[] TreesT2R;   //������� ���� 2 ������
	public GameObject[] TreesT1W;   //������� ���� 1 ������
	public GameObject[] TreesT1WR;  //������� ���� 1 ������ ������
	public GameObject[] TreesT2W;   //������� ���� 2 ������
	public GameObject[] TreesT2WR;  //������� ���� 2 ������ ������
	public GameObject[] Grass;      //�����
	public GameObject[] Bushes;     //�����
}