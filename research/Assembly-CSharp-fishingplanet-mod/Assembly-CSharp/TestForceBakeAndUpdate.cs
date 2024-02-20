using System;
using UnityEngine;

public class TestForceBakeAndUpdate : MonoBehaviour
{
	private void Update()
	{
		if (Time.frameCount == 500)
		{
			float timeSinceLevelLoad = Time.timeSinceLevelLoad;
			Debug.Log("Moving player");
			this.player.position += new Vector3(0f, 0f, 250f);
			Camera.main.transform.position = this.player.position + Vector3.forward * 10f;
			Debug.Log("Forcing check");
			MB2_LODManager.Manager().checkScheduler.ForceCheckIfLODsNeedToChange();
			Debug.Log("Forcing bake");
			MB2_LODManager.Manager().ForceBakeAllDirty();
			Debug.LogError("Done, took " + (Time.timeSinceLevelLoad - timeSinceLevelLoad).ToString("F8"));
		}
	}

	public Transform player;
}
