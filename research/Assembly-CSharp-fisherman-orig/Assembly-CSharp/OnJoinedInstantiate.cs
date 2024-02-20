using System;
using UnityEngine;

public class OnJoinedInstantiate : MonoBehaviour
{
	public void OnJoinedRoom()
	{
		if (this.PrefabsToInstantiate != null)
		{
			foreach (GameObject gameObject in this.PrefabsToInstantiate)
			{
				Debug.Log("Instantiating: " + gameObject.name);
				Vector3 vector = Vector3.up;
				if (this.SpawnPosition != null)
				{
					vector = this.SpawnPosition.position;
				}
				Vector3 vector2 = Random.insideUnitSphere;
				vector2.y = 0f;
				vector2 = vector2.normalized;
				Vector3 vector3 = vector + this.PositionOffset * vector2;
				PhotonNetwork.Instantiate(gameObject.name, vector3, Quaternion.identity, 0);
			}
		}
	}

	public Transform SpawnPosition;

	public float PositionOffset = 2f;

	public GameObject[] PrefabsToInstantiate;
}
