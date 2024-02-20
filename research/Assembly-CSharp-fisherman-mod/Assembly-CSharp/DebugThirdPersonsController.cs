using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugThirdPersonsController : MonoBehaviour
{
	public Dictionary<int, Player3dController> Players { get; private set; }

	private void Start()
	{
		this.Players = new Dictionary<int, Player3dController>();
	}

	private void Update()
	{
	}

	public void OnPlayerModelEnter(int id, string name, Vector3 position)
	{
		GameObject gameObject = Object.Instantiate<GameObject>(this.playerPrefab, position, this.playerPrefab.transform.rotation);
		Player3dController component = gameObject.GetComponent<Player3dController>();
		this.Players[id] = component;
		component.setUserName(name);
	}

	public void OnPlayerModelLeave(int id)
	{
		Object.DestroyObject(this.Players[id].gameObject);
		this.Players.Remove(id);
	}

	public void onNewTransform(int id, Vector3 newPosition, Quaternion newRotation)
	{
		this.Players[id].onNewTransform(newPosition, newRotation);
	}

	public GameObject playerPrefab;
}
