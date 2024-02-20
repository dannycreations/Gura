using System;
using UnityEngine;

[Serializable]
public class RodBones
{
	private GameObject GetBone(string name)
	{
		if (this.transform != null)
		{
			Transform transform = TransformHelper.FindDeepChild(this.transform, name);
			return (!(transform != null)) ? null : transform.gameObject;
		}
		return null;
	}

	public GameObject RightHand
	{
		get
		{
			return this.GetBone(this.rightHandName);
		}
	}

	public GameObject LeftHand
	{
		get
		{
			return this.GetBone(this.leftHandName);
		}
	}

	public void Init(Transform transform)
	{
		this.transform = transform;
	}

	public string leftHandName;

	public string rightHandName;

	private Transform transform;
}
