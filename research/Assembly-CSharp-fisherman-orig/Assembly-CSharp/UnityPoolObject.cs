using System;
using UnityEngine;

public class UnityPoolObject : MonoBehaviour, IPoolObject<string>
{
	public virtual string Group
	{
		get
		{
			return base.name;
		}
	}

	public Transform MyTransform
	{
		get
		{
			return this.myTransform;
		}
	}

	protected virtual void Awake()
	{
		this.myTransform = base.transform;
	}

	public virtual void SetTransform(Vector3 position, Quaternion rotation)
	{
		this.myTransform.position = position;
		this.myTransform.rotation = rotation;
	}

	public virtual void Create()
	{
		base.gameObject.SetActive(true);
	}

	public virtual void OnPush()
	{
		base.gameObject.SetActive(false);
	}

	public virtual void Push()
	{
		UnityPoolManager.Instance.Push(this.Group, this);
	}

	public void FailedPush()
	{
		Debug.Log("FailedPush");
		Object.Destroy(base.gameObject);
	}

	protected Transform myTransform;
}
