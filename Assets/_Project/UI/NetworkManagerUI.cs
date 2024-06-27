using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
	[SerializeField] private Button _hostButton;
	[SerializeField] private Button _clientButton;

	private void Awake()
	{
		_hostButton.onClick.AddListener(call: (() =>
		{
			NetworkManager.Singleton.StartHost();
		}
		));
		_clientButton.onClick.AddListener(call: (() =>
		{
			NetworkManager.Singleton.StartClient();
		}
		));
	}
}
