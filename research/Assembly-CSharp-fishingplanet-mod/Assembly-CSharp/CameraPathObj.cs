using System;
using System.Collections.Generic;
using UnityEngine;

internal class CameraPathObj : MonoBehaviour
{
	private void Awake()
	{
		this.PrepareData();
		base.gameObject.SetActive(false);
	}

	private void PrepareData()
	{
		List<CameraPoint> list = new List<CameraPoint>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			CameraPoint component = base.transform.GetChild(i).GetComponent<CameraPoint>();
			if (component != null)
			{
				list.Add(component);
			}
			else
			{
				LogHelper.Error("Child {0}.{1} has no CameraPoint component", new object[]
				{
					base.name,
					base.transform.GetChild(i).name
				});
			}
		}
		this._data = new CameraPathData(list, this._movementSpeed, this._rotationSpeed);
	}

	public CameraPathData Data
	{
		get
		{
			return this._data;
		}
	}

	[SerializeField]
	private float _movementSpeed;

	[SerializeField]
	private float _rotationSpeed;

	private CameraPathData _data;
}
