using System;
using UnityEngine;

public class MB2_TestUpdate : MonoBehaviour
{
	private void Start()
	{
		this.meshbaker.AddDeleteGameObjects(this.objsToMove, null, true);
		this.meshbaker.AddDeleteGameObjects(new GameObject[] { this.objWithChangingUVs }, null, true);
		MeshFilter meshFilter = this.objWithChangingUVs.GetComponent<MeshFilter>();
		this.m = meshFilter.sharedMesh;
		this.uvs = this.m.uv;
		this.meshbaker.Apply(null);
		this.multiMeshBaker.AddDeleteGameObjects(this.objsToMove, null, true);
		this.multiMeshBaker.AddDeleteGameObjects(new GameObject[] { this.objWithChangingUVs }, null, true);
		meshFilter = this.objWithChangingUVs.GetComponent<MeshFilter>();
		this.m = meshFilter.sharedMesh;
		this.uvs = this.m.uv;
		this.multiMeshBaker.Apply(null);
	}

	private void LateUpdate()
	{
		this.meshbaker.UpdateGameObjects(this.objsToMove, false, true, true, true, false, false, false, false, false);
		Vector2[] array = this.m.uv;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Mathf.Sin(Time.time) * this.uvs[i];
		}
		this.m.uv = array;
		this.meshbaker.UpdateGameObjects(new GameObject[] { this.objWithChangingUVs }, true, true, true, true, true, false, false, false, false);
		this.meshbaker.Apply(false, true, true, true, true, false, false, false, false, false, false, null);
		this.multiMeshBaker.UpdateGameObjects(this.objsToMove, false, true, true, true, false, false, false, false, false);
		array = this.m.uv;
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = Mathf.Sin(Time.time) * this.uvs[j];
		}
		this.m.uv = array;
		this.multiMeshBaker.UpdateGameObjects(new GameObject[] { this.objWithChangingUVs }, true, true, true, true, true, false, false, false, false);
		this.multiMeshBaker.Apply(false, true, true, true, true, false, false, false, false, false, false, null);
	}

	public MB3_MeshBaker meshbaker;

	public MB3_MultiMeshBaker multiMeshBaker;

	public GameObject[] objsToMove;

	public GameObject objWithChangingUVs;

	private Vector2[] uvs;

	private Mesh m;
}
