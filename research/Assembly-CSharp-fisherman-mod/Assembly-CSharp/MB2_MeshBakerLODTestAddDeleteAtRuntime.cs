using System;
using UnityEngine;

public class MB2_MeshBakerLODTestAddDeleteAtRuntime : MonoBehaviour
{
	private void Update()
	{
		Debug.Log("Adding baker");
		if (Time.frameCount == 100)
		{
			this.bp.meshBaker = this.mb1;
			MB2_LODManager.Manager().AddBaker(this.bp);
		}
		Debug.Log("Instantiate prefab");
		if (Time.frameCount == 200)
		{
			Debug.Log("b " + (this.bp == MB2_LODManager.Manager().bakers[0]));
			GameObject gameObject = Object.Instantiate<GameObject>(this.treePrefab);
			gameObject.transform.Translate(new Vector3(10f, 0f, 0f));
			Debug.Log("c " + (this.bp == MB2_LODManager.Manager().bakers[0]));
		}
		Debug.Log("Instantiate prefab");
		if (Time.frameCount == 300)
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.treePrefab);
			gameObject2.transform.Translate(new Vector3(10f, 0f, 0f));
			Debug.Log("a " + (this.bp == MB2_LODManager.Manager().bakers[0]));
		}
		Debug.Log("Remove baker");
		if (Time.frameCount == 400)
		{
			Debug.Log(this.bp == MB2_LODManager.Manager().bakers[0]);
			MB2_LODManager.Manager().RemoveBaker(this.bp);
		}
	}

	public MB3_MeshBaker mb1;

	public MB3_MeshBaker mb2;

	public GameObject treePrefab;

	private MB2_LODManager.BakerPrototype bp = new MB2_LODManager.BakerPrototype();
}
