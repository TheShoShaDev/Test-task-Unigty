using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlaceInteractible : MonoBehaviour
{
	[SerializeField] private bool _isFree = true;
	[SerializeField] private Transform _placePos;

	[SerializeField] private CubeFieldInteractible _field;

	private Block _currentBlock;

	public bool IsFree => _isFree;
	public Transform PlacePos => _placePos;

	public void SetBusy(Block Block)
	{
		_currentBlock = Block;
		_isFree = false;
		_field.AddIndex(this);
	}

	public void SetFree(Block Block)
	{
		_currentBlock = null;
		_isFree = true;
		_field.RemoveIndex(this);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.CompareTag("Cube"))
		{
			SetBusy(collision.gameObject.GetComponent<Block>());
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.collider.CompareTag("Cube"))
		{
			SetFree(collision.gameObject.GetComponent<Block>());
		}
	}

}
