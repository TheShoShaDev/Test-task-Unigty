using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeField : NetworkBehaviour
{
	[SerializeField] private GameObject _cubePrefab;
	[SerializeField] private GameObject _cubePlacePrefab;

	[SerializeField] private List<CubePlace> _field = new List<CubePlace>();

	public readonly List<int> FilledCellsIndex = new List<int>();

	public override void OnNetworkSpawn()
	{
		if(!IsServer)
		{
			enabled = false;
			return;
		}

		for (int i = 0; i < 6; i++)
		{
			int Index = Random.Range(0, _field.Count);
			GameObject block = Instantiate(_cubePrefab,GetFreeCell().position, Quaternion.identity);
			block.GetComponent<NetworkObject>().Spawn(true);

		}
	}

	private Transform GetFreeCell()
	{
		int Index = Random.Range(0, _field.Count);

		if (_field[Index].IsFree)
		{
			_field[Index].SetBusy();
			FilledCellsIndex.Add(Index);
			return _field[Index].SpawnPos;
		}
		else
		{
			return GetFreeCell();
		}
	}
}
