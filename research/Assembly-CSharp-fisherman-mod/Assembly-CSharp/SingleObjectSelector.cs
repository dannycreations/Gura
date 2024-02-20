using System;
using UnityEngine;

public class SingleObjectSelector : MonoBehaviour
{
	private MobileObject CurObj
	{
		get
		{
			return (this._curIndex == -1) ? null : this._objects[this._curIndex];
		}
	}

	private void Awake()
	{
		for (int i = 0; i < this._objects.Length; i++)
		{
			this._objects[i].Deactivate();
		}
	}

	private void Start()
	{
		this.Select(Random.Range(0f, 1f));
	}

	private void Select(float initPos = 0f)
	{
		if (this.CurObj != null)
		{
			MobileObject curObj = this.CurObj;
			curObj.EDisabled = (Action<MobileObject>)Delegate.Remove(curObj.EDisabled, new Action<MobileObject>(this.OnObjectDisabled));
		}
		this._curIndex = Random.Range(0, this._objects.Length);
		MobileObject mobileObject = this._objects[this._curIndex];
		mobileObject.EDisabled = (Action<MobileObject>)Delegate.Combine(mobileObject.EDisabled, new Action<MobileObject>(this.OnObjectDisabled));
		this._objects[this._curIndex].Launch(initPos);
		this.EObjectSelected(this._objects[this._curIndex]);
	}

	private void Update()
	{
		if (this._nextLaunchAt > 0f && this._nextLaunchAt < Time.time)
		{
			this._nextLaunchAt = -1f;
			this.Select(0f);
		}
	}

	private void OnObjectDisabled(MobileObject obj)
	{
		this._nextLaunchAt = Time.time + Random.Range(this._minDelay, this._maxDelay);
	}

	private void OnDestroy()
	{
		if (this.CurObj != null)
		{
			MobileObject curObj = this.CurObj;
			curObj.EDisabled = (Action<MobileObject>)Delegate.Remove(curObj.EDisabled, new Action<MobileObject>(this.OnObjectDisabled));
			this._curIndex = -1;
		}
	}

	[SerializeField]
	private MobileObject[] _objects;

	[SerializeField]
	private float _minDelay;

	[SerializeField]
	private float _maxDelay;

	[SerializeField]
	private bool _debugCameraAttached;

	public Action<MobileObject> EObjectSelected = delegate
	{
	};

	private int _curIndex = -1;

	private float _nextLaunchAt = -1f;

	private bool _wasCameraAttached;
}
