using System;
using UnityEngine;

public class BakeTexturesAtRuntime : MonoBehaviour
{
	private void OnGUI()
	{
		GUILayout.Label("Time to bake textures: " + this.elapsedTime, new GUILayoutOption[0]);
		if (GUILayout.Button("Combine textures & build combined mesh all at once", new GUILayoutOption[0]))
		{
			MB3_MeshBaker componentInChildren = this.target.GetComponentInChildren<MB3_MeshBaker>();
			MB3_TextureBaker component = this.target.GetComponent<MB3_TextureBaker>();
			component.textureBakeResults = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
			component.resultMaterial = new Material(Shader.Find("Diffuse"));
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			component.CreateAtlases();
			this.elapsedTime = Time.realtimeSinceStartup - realtimeSinceStartup;
			componentInChildren.ClearMesh();
			componentInChildren.textureBakeResults = component.textureBakeResults;
			componentInChildren.AddDeleteGameObjects(component.GetObjectsToCombine().ToArray(), null, true);
			componentInChildren.Apply(null);
		}
		if (GUILayout.Button("Combine textures & build combined mesh using coroutine", new GUILayoutOption[0]))
		{
			Debug.Log("Starting to bake textures on frame " + Time.frameCount);
			MB3_TextureBaker component2 = this.target.GetComponent<MB3_TextureBaker>();
			component2.textureBakeResults = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
			component2.resultMaterial = new Material(Shader.Find("Diffuse"));
			component2.onBuiltAtlasesSuccess = new MB3_TextureBaker.OnCombinedTexturesCoroutineSuccess(this.OnBuiltAtlasesSuccess);
			base.StartCoroutine(component2.CreateAtlasesCoroutine(null, this.result, false, null, 0.01f));
		}
	}

	private void OnBuiltAtlasesSuccess()
	{
		Debug.Log("Calling success callback. baking meshes");
		MB3_MeshBaker componentInChildren = this.target.GetComponentInChildren<MB3_MeshBaker>();
		MB3_TextureBaker component = this.target.GetComponent<MB3_TextureBaker>();
		if (this.result.isFinished && this.result.success)
		{
			componentInChildren.ClearMesh();
			componentInChildren.textureBakeResults = component.textureBakeResults;
			componentInChildren.AddDeleteGameObjects(component.GetObjectsToCombine().ToArray(), null, true);
			componentInChildren.Apply(null);
		}
		Debug.Log("Completed baking textures on frame " + Time.frameCount);
	}

	public GameObject target;

	private float elapsedTime;

	private MB3_TextureBaker.CreateAtlasesCoroutineResult result = new MB3_TextureBaker.CreateAtlasesCoroutineResult();
}
