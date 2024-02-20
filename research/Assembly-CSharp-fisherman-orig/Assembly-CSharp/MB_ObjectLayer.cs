using System;
using System.Collections.Generic;
using UnityEngine;

public class MB_ObjectLayer
{
	public MB_ObjectLayer(MB3_MultiMeshBaker baker)
	{
		this._baker = baker;
		List<GameObject> objectsToCombine = baker.GetObjectsToCombine();
		for (int i = 0; i < objectsToCombine.Count; i++)
		{
			this._partsSet.Add(objectsToCombine[i].name);
		}
		this._objectsRoot = new GameObject().transform;
		this._objectsRoot.SetParent(baker.transform, false);
	}

	public void AddModel(GameObject factory)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < factory.transform.childCount; i++)
		{
			Transform child = factory.transform.GetChild(i);
			for (int j = 0; j < child.childCount; j++)
			{
				Transform child2 = child.GetChild(j);
				if (this._partsSet.Contains(child2.name))
				{
					list.Add(child2.gameObject);
				}
			}
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		this._baker.AddDeleteGameObjects(list.ToArray(), null, true);
		this._baker.Apply(null);
		float realtimeSinceStartup2 = Time.realtimeSinceStartup;
	}

	public void DelModel(GameObject deletingObject)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < deletingObject.transform.childCount; i++)
		{
			Transform child = deletingObject.transform.GetChild(i);
			for (int j = 0; j < child.childCount; j++)
			{
				Transform child2 = child.GetChild(j);
				if (this._partsSet.Contains(child2.name))
				{
					list.Add(child2.gameObject);
				}
			}
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		this._baker.AddDeleteGameObjects(null, list.ToArray(), true);
		this._baker.Apply(null);
		float realtimeSinceStartup2 = Time.realtimeSinceStartup;
	}

	private MB3_MultiMeshBaker _baker;

	private Transform _objectsRoot;

	private HashSet<string> _partsSet = new HashSet<string>();
}
