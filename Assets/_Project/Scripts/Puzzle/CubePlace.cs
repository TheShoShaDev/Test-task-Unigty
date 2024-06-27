using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlace : MonoBehaviour
{
	[SerializeField] private Transform _spawnPos;
	[SerializeField] private bool _isFree = true;

	public bool IsFree => _isFree;
	public Transform SpawnPos => _spawnPos;

	public void SetBusy()
	{
		_isFree = false;
	}


}
