using System;
using UnityEngine;

namespace ArtMode
{
	public class DebugPlayerCreator : MonoBehaviour
	{
		private void Awake()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this._playerPrefab);
			gameObject.name = this._playerPrefab.name;
			DebugCharacterController component = gameObject.GetComponent<DebugCharacterController>();
			component.Init(this._playerPrefab);
		}

		[SerializeField]
		private GameObject _playerPrefab;
	}
}
