using System;
using System.Collections.Generic;
using UnityEngine;

public class MB_TestObjectsCombining : MonoBehaviour
{
	private void Start()
	{
		this._prevObjectPos = this._initialPosition.position;
		for (int i = 0; i < this._partsToBake.Length; i++)
		{
			this._partsSet.Add(this._partsToBake[i].name);
		}
	}

	private Vector3 GenerateNextObjectPos(int sign = 1)
	{
		return this._prevObjectPos += (float)sign * this._initialPosition.TransformDirection(new Vector3(this._distBetweenObjects, 0f, 0f));
	}

	private void Update()
	{
		if (Input.GetKeyUp(277))
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this._prefabToInstantiate);
			gameObject.transform.position = this.GenerateNextObjectPos(1);
			gameObject.transform.SetParent(this._objectsRoot, true);
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				Transform child = gameObject.transform.GetChild(i);
				for (int j = 0; j < child.childCount; j++)
				{
					Transform child2 = child.GetChild(j);
					if (this._partsSet.Contains(child2.name))
					{
						list.Add(child2.gameObject);
					}
				}
			}
			this._baker.AddDeleteGameObjects(list.ToArray(), null, true);
			this._baker.Apply(null);
		}
		if (Input.GetKeyUp(127) && this._objectsRoot.childCount > 0)
		{
			List<GameObject> list2 = new List<GameObject>();
			GameObject gameObject2 = this._objectsRoot.GetChild(this._objectsRoot.childCount - 1).gameObject;
			for (int k = 0; k < gameObject2.transform.childCount; k++)
			{
				Transform child3 = gameObject2.transform.GetChild(k);
				for (int l = 0; l < child3.childCount; l++)
				{
					Transform child4 = child3.GetChild(l);
					if (this._partsSet.Contains(child4.name))
					{
						list2.Add(child4.gameObject);
					}
				}
			}
			this._baker.AddDeleteGameObjects(null, list2.ToArray(), true);
			this._baker.Apply(null);
			Object.Destroy(gameObject2);
			this.GenerateNextObjectPos(-1);
		}
	}

	[SerializeField]
	private MB3_MeshBaker _baker;

	[SerializeField]
	private GameObject _prefabToInstantiate;

	[SerializeField]
	private GameObject[] _partsToBake;

	[SerializeField]
	private Transform _objectsRoot;

	[SerializeField]
	private Transform _initialPosition;

	[SerializeField]
	private float _distBetweenObjects;

	private Vector3 _prevObjectPos;

	private HashSet<string> _partsSet = new HashSet<string>();
}
