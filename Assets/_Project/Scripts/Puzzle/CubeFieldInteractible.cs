using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CubeFieldInteractible : MonoBehaviour
{
	[SerializeField] private List<int> _busyPlaces = new List<int>();
	[SerializeField] private List<CubePlaceInteractible> _field = new List<CubePlaceInteractible>();

	[SerializeField] private CubeField _cubeField;

	public void AddIndex(CubePlaceInteractible CubePlaceInteractible)
	{
		int Index = _field.IndexOf(CubePlaceInteractible);
		_busyPlaces.Add(Index);

		if(CompareLists(_busyPlaces, _cubeField.FilledCellsIndex))
		{
			Debug.Log("Условная победа");
		}
	}


	public void RemoveIndex(CubePlaceInteractible CubePlaceInteractible)
	{
		int Index = _field.IndexOf(CubePlaceInteractible);
		_busyPlaces = _busyPlaces.Where(x => x != Index).ToList();
	}

	private bool CompareLists(List<int> List, List<int> ReferenceList)
	{
		if (List.Count != ReferenceList.Count)
		{
			return false;
		}

		List.Sort();
		ReferenceList.Sort();

		for (int i = 0; i < List.Count; i++)
		{
			if (List[i] != ReferenceList[i])
			{
				return false;
			}
		}
		return true;

	}
}
