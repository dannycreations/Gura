using System;
using UnityEngine;

public class MoveCluster : MonoBehaviour
{
	private void LateUpdate()
	{
		if (Time.frameCount % 300 == 0)
		{
			MB2_LODManager mb2_LODManager = (MB2_LODManager)Object.FindObjectOfType(typeof(MB2_LODManager));
			Vector3 vector;
			vector..ctor(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
			Vector3 vector2 = vector - this.world.position;
			this.world.position += vector2;
			mb2_LODManager.TranslateWorld(vector2);
			Debug.Log("Moving World To " + vector);
		}
	}

	public Transform world;
}
