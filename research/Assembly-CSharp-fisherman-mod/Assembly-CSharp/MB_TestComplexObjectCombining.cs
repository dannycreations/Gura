using System;
using System.Collections.Generic;
using UnityEngine;

public class MB_TestComplexObjectCombining : MonoBehaviour
{
	private void Start()
	{
		this._prevObjectPos = this._initialPosition.position;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			MB3_MultiMeshBaker component = child.GetComponent<MB3_MultiMeshBaker>();
			if (component != null)
			{
				this._bakers.Add(new MB_ObjectLayer(component));
			}
		}
	}

	private Vector3 GenerateNextObjectPos(int sign = 1)
	{
		return this._prevObjectPos += (float)sign * this._initialPosition.TransformDirection(new Vector3(this._distBetweenObjects, this._initialPosition.position.y, 0f));
	}

	private void Update()
	{
		if (Input.GetKey(277))
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this._prefabToInstantiate);
			gameObject.transform.position = this.GenerateNextObjectPos(1);
			gameObject.transform.SetParent(this._objectsRoot, true);
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			for (int i = 0; i < this._bakers.Count; i++)
			{
				this._bakers[i].AddModel(gameObject);
			}
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
		}
		if (Input.GetKey(127) && this._objectsRoot.childCount > 0)
		{
			GameObject gameObject2 = this._objectsRoot.GetChild(this._objectsRoot.childCount - 1).gameObject;
			float realtimeSinceStartup3 = Time.realtimeSinceStartup;
			for (int j = 0; j < this._bakers.Count; j++)
			{
				this._bakers[j].DelModel(gameObject2);
			}
			float realtimeSinceStartup4 = Time.realtimeSinceStartup;
			Object.Destroy(gameObject2);
			this.GenerateNextObjectPos(-1);
		}
	}

	[SerializeField]
	private GameObject _prefabToInstantiate;

	[SerializeField]
	private Transform _objectsRoot;

	[SerializeField]
	private Transform _initialPosition;

	[SerializeField]
	private float _distBetweenObjects;

	private List<MB_ObjectLayer> _bakers = new List<MB_ObjectLayer>();

	private Vector3 _prevObjectPos;
}
