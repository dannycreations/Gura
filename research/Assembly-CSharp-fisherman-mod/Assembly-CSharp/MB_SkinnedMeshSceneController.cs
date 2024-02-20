using System;
using UnityEngine;

public class MB_SkinnedMeshSceneController : MonoBehaviour
{
	private void Start()
	{
		GameObject gameObject = Object.Instantiate<GameObject>(this.workerPrefab);
		gameObject.transform.position = new Vector3(1.31f, 0.985f, -0.25f);
		Animation component = gameObject.GetComponent<Animation>();
		component.wrapMode = 2;
		component.cullingType = 0;
		component.Play("run");
		GameObject[] array = new GameObject[] { gameObject.GetComponentInChildren<SkinnedMeshRenderer>().gameObject };
		this.skinnedMeshBaker.AddDeleteGameObjects(array, null, true);
		this.skinnedMeshBaker.Apply(null);
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Add/Remove Sword", new GUILayoutOption[0]))
		{
			if (this.swordInstance == null)
			{
				Transform transform = this.SearchHierarchyForBone(this.targetCharacter.transform, "RightHandAttachPoint");
				this.swordInstance = Object.Instantiate<GameObject>(this.swordPrefab);
				this.swordInstance.transform.parent = transform;
				this.swordInstance.transform.localPosition = Vector3.zero;
				this.swordInstance.transform.localRotation = Quaternion.identity;
				this.swordInstance.transform.localScale = Vector3.one;
				GameObject[] array = new GameObject[] { this.swordInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				this.skinnedMeshBaker.AddDeleteGameObjects(array, null, true);
				this.skinnedMeshBaker.Apply(null);
			}
			else if (this.skinnedMeshBaker.CombinedMeshContains(this.swordInstance.GetComponentInChildren<MeshRenderer>().gameObject))
			{
				GameObject[] array2 = new GameObject[] { this.swordInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				this.skinnedMeshBaker.AddDeleteGameObjects(null, array2, true);
				this.skinnedMeshBaker.Apply(null);
				Object.Destroy(this.swordInstance);
				this.swordInstance = null;
			}
		}
		if (GUILayout.Button("Add/Remove Hat", new GUILayoutOption[0]))
		{
			if (this.hatInstance == null)
			{
				Transform transform2 = this.SearchHierarchyForBone(this.targetCharacter.transform, "HeadAttachPoint");
				this.hatInstance = Object.Instantiate<GameObject>(this.hatPrefab);
				this.hatInstance.transform.parent = transform2;
				this.hatInstance.transform.localPosition = Vector3.zero;
				this.hatInstance.transform.localRotation = Quaternion.identity;
				this.hatInstance.transform.localScale = Vector3.one;
				GameObject[] array3 = new GameObject[] { this.hatInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				this.skinnedMeshBaker.AddDeleteGameObjects(array3, null, true);
				this.skinnedMeshBaker.Apply(null);
			}
			else if (this.skinnedMeshBaker.CombinedMeshContains(this.hatInstance.GetComponentInChildren<MeshRenderer>().gameObject))
			{
				GameObject[] array4 = new GameObject[] { this.hatInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				this.skinnedMeshBaker.AddDeleteGameObjects(null, array4, true);
				this.skinnedMeshBaker.Apply(null);
				Object.Destroy(this.hatInstance);
				this.hatInstance = null;
			}
		}
		if (GUILayout.Button("Add/Remove Glasses", new GUILayoutOption[0]))
		{
			if (this.glassesInstance == null)
			{
				Transform transform3 = this.SearchHierarchyForBone(this.targetCharacter.transform, "NoseAttachPoint");
				this.glassesInstance = Object.Instantiate<GameObject>(this.glassesPrefab);
				this.glassesInstance.transform.parent = transform3;
				this.glassesInstance.transform.localPosition = Vector3.zero;
				this.glassesInstance.transform.localRotation = Quaternion.identity;
				this.glassesInstance.transform.localScale = Vector3.one;
				GameObject[] array5 = new GameObject[] { this.glassesInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				this.skinnedMeshBaker.AddDeleteGameObjects(array5, null, true);
				this.skinnedMeshBaker.Apply(null);
			}
			else if (this.skinnedMeshBaker.CombinedMeshContains(this.glassesInstance.GetComponentInChildren<MeshRenderer>().gameObject))
			{
				GameObject[] array6 = new GameObject[] { this.glassesInstance.GetComponentInChildren<MeshRenderer>().gameObject };
				this.skinnedMeshBaker.AddDeleteGameObjects(null, array6, true);
				this.skinnedMeshBaker.Apply(null);
				Object.Destroy(this.glassesInstance);
				this.glassesInstance = null;
			}
		}
	}

	public Transform SearchHierarchyForBone(Transform current, string name)
	{
		if (current.name.Equals(name))
		{
			return current;
		}
		for (int i = 0; i < current.childCount; i++)
		{
			Transform transform = this.SearchHierarchyForBone(current.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public GameObject swordPrefab;

	public GameObject hatPrefab;

	public GameObject glassesPrefab;

	public GameObject workerPrefab;

	public GameObject targetCharacter;

	public MB3_MeshBaker skinnedMeshBaker;

	private GameObject swordInstance;

	private GameObject glassesInstance;

	private GameObject hatInstance;
}
