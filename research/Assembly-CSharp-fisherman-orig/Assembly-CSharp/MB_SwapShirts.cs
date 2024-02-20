using System;
using System.Collections.Generic;
using UnityEngine;

public class MB_SwapShirts : MonoBehaviour
{
	private void Start()
	{
		GameObject[] array = new GameObject[this.clothingAndBodyPartsBareTorso.Length];
		for (int i = 0; i < this.clothingAndBodyPartsBareTorso.Length; i++)
		{
			array[i] = this.clothingAndBodyPartsBareTorso[i].gameObject;
		}
		this.meshBaker.ClearMesh();
		this.meshBaker.AddDeleteGameObjects(array, null, true);
		this.meshBaker.Apply(null);
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Wear Hoodie", new GUILayoutOption[0]))
		{
			this.ChangeOutfit(this.clothingAndBodyPartsHoodie);
		}
		if (GUILayout.Button("Bare Torso", new GUILayoutOption[0]))
		{
			this.ChangeOutfit(this.clothingAndBodyPartsBareTorso);
		}
		if (GUILayout.Button("Damaged Arm", new GUILayoutOption[0]))
		{
			this.ChangeOutfit(this.clothingAndBodyPartsBareTorsoDamagedArm);
		}
	}

	private void ChangeOutfit(Renderer[] outfit)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject gameObject in this.meshBaker.meshCombiner.GetObjectsInCombined())
		{
			Renderer component = gameObject.GetComponent<Renderer>();
			bool flag = false;
			for (int i = 0; i < outfit.Length; i++)
			{
				if (component == outfit[i])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(component.gameObject);
				Debug.Log("Removing " + component.gameObject);
			}
		}
		List<GameObject> list2 = new List<GameObject>();
		for (int j = 0; j < outfit.Length; j++)
		{
			if (!this.meshBaker.meshCombiner.GetObjectsInCombined().Contains(outfit[j].gameObject))
			{
				list2.Add(outfit[j].gameObject);
				Debug.Log("Adding " + outfit[j].gameObject);
			}
		}
		this.meshBaker.AddDeleteGameObjects(list2.ToArray(), list.ToArray(), true);
		this.meshBaker.Apply(null);
	}

	public MB3_MeshBaker meshBaker;

	public Renderer[] clothingAndBodyPartsBareTorso;

	public Renderer[] clothingAndBodyPartsBareTorsoDamagedArm;

	public Renderer[] clothingAndBodyPartsHoodie;
}
