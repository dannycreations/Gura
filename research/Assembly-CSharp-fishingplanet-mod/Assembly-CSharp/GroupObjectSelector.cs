using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupObjectSelector : MonoBehaviour
{
	public void FreezeRequest()
	{
		this._isFrozen = true;
		if (this._inZoneObjects.Count == 0)
		{
			this.EFrozen();
		}
	}

	public void UnFreeze()
	{
		this._isFrozen = false;
		for (int i = 0; i < this._visibleObjects.Count; i++)
		{
			this._visibleObjects[i].UnFreeze();
		}
		for (int j = 0; j < this._skippedObject.Count; j++)
		{
			this._inZoneObjects.Add(this._skippedObject[j]);
		}
		this._skippedObject.Clear();
	}

	private void Awake()
	{
		if (this._freezeTrigger != null)
		{
			SimpleTrigger freezeTrigger = this._freezeTrigger;
			freezeTrigger.ETriggerEnter = (Action<Collider>)Delegate.Combine(freezeTrigger.ETriggerEnter, new Action<Collider>(this.OnObjEnter));
			SimpleTrigger freezeTrigger2 = this._freezeTrigger;
			freezeTrigger2.ETriggerExit = (Action<Collider>)Delegate.Combine(freezeTrigger2.ETriggerExit, new Action<Collider>(this.OnObjExit));
		}
		for (int i = 0; i < this._pool.Count; i++)
		{
			this._pool[i].Deactivate();
		}
		if (this._fromSplineStartOnly)
		{
			this._nextUpdateAt = Time.time + Random.Range(this._smallGroupMinDelay, this._smallGroupMaxDelay);
		}
		else
		{
			int num = 0;
			while ((float)num < this._groupSize)
			{
				this.Select(Random.Range(0f, 1f));
				num++;
			}
		}
	}

	private void Update()
	{
		if (this._nextUpdateAt > 0f && this._nextUpdateAt < Time.time)
		{
			this.Select(0f);
			this._nextUpdateAt = (((float)this._visibleObjects.Count >= this._groupSize || this._pool.Count <= 0) ? (-1f) : (Time.time + Random.Range(this._smallGroupMinDelay, this._smallGroupMaxDelay)));
		}
	}

	private void Select(float prc = 0f)
	{
		if ((float)this._visibleObjects.Count < this._groupSize && this._pool.Count > 0)
		{
			int num = Random.Range(0, this._pool.Count);
			MobileObject mobileObject = this._pool[num];
			this._pool.RemoveAt(num);
			this._visibleObjects.Add(mobileObject);
			MobileObject mobileObject2 = mobileObject;
			mobileObject2.EDisabled = (Action<MobileObject>)Delegate.Combine(mobileObject2.EDisabled, new Action<MobileObject>(this.OnObjDisabled));
			mobileObject.Flag = false;
			mobileObject.Launch(prc);
			MobileObject mobileObject3;
			if (!this._pathObjects.ContainsKey(mobileObject.Group))
			{
				this._pathObjects[mobileObject.Group] = new LinkedList<MobileObject>();
				mobileObject3 = null;
			}
			else
			{
				LinkedList<MobileObject> linkedList = this._pathObjects[mobileObject.Group];
				mobileObject3 = ((linkedList.Count <= 0) ? null : linkedList.Last.Value);
			}
			this._pathObjects[mobileObject.Group].AddLast(mobileObject);
			mobileObject.LeadingObj = mobileObject3;
		}
	}

	private void OnObjDisabled(MobileObject mobileObject)
	{
		mobileObject.EDisabled = (Action<MobileObject>)Delegate.Remove(mobileObject.EDisabled, new Action<MobileObject>(this.OnObjDisabled));
		this._pool.Add(mobileObject);
		LinkedList<MobileObject> linkedList = this._pathObjects[mobileObject.Group];
		if (linkedList.First.Value != mobileObject)
		{
			LogHelper.Error("Incorrect processing order for {0}", new object[] { mobileObject.name });
			return;
		}
		linkedList.RemoveFirst();
		if (linkedList.Count > 0)
		{
			linkedList.First.Value.LeadingObj = null;
		}
		this._visibleObjects.Remove(mobileObject);
		if (this._nextUpdateAt < 0f)
		{
			this._nextUpdateAt = Time.time + Random.Range(this._minDelay, this._maxDelay);
		}
	}

	private void OnObjEnter(Collider obj)
	{
		MobileObject component = obj.gameObject.GetComponent<MobileObject>();
		if (component != null)
		{
			component.Flag = true;
			if (this._isFrozen)
			{
				component.Freeze();
				this._skippedObject.Add(obj);
			}
			else
			{
				this._inZoneObjects.Add(obj);
			}
		}
	}

	private void OnObjExit(Collider obj)
	{
		MobileObject component = obj.gameObject.GetComponent<MobileObject>();
		if (component != null)
		{
			this._inZoneObjects.Remove(obj);
			if (this._inZoneObjects.Count == 0 && this._isFrozen)
			{
				this.EFrozen();
			}
		}
	}

	private void OnDestroy()
	{
		this._pathObjects.Clear();
		this._inZoneObjects.Clear();
		if (this._freezeTrigger != null)
		{
			SimpleTrigger freezeTrigger = this._freezeTrigger;
			freezeTrigger.ETriggerEnter = (Action<Collider>)Delegate.Remove(freezeTrigger.ETriggerEnter, new Action<Collider>(this.OnObjEnter));
			SimpleTrigger freezeTrigger2 = this._freezeTrigger;
			freezeTrigger2.ETriggerExit = (Action<Collider>)Delegate.Remove(freezeTrigger2.ETriggerExit, new Action<Collider>(this.OnObjExit));
		}
		for (int i = 0; i < this._visibleObjects.Count; i++)
		{
			MobileObject mobileObject = this._visibleObjects[i];
			mobileObject.EDisabled = (Action<MobileObject>)Delegate.Remove(mobileObject.EDisabled, new Action<MobileObject>(this.OnObjDisabled));
		}
		this._visibleObjects.Clear();
		this._skippedObject.Clear();
	}

	[SerializeField]
	private List<MobileObject> _pool;

	[SerializeField]
	private float _groupSize;

	[SerializeField]
	private float _minDelay;

	[SerializeField]
	private float _maxDelay;

	[SerializeField]
	private float _smallGroupMinDelay;

	[SerializeField]
	private float _smallGroupMaxDelay;

	[SerializeField]
	private bool _fromSplineStartOnly;

	[SerializeField]
	private SimpleTrigger _freezeTrigger;

	public Action EFrozen = delegate
	{
	};

	private List<MobileObject> _visibleObjects = new List<MobileObject>();

	private float _nextUpdateAt = -1f;

	private bool _isFrozen;

	private HashSet<Collider> _inZoneObjects = new HashSet<Collider>();

	private List<Collider> _skippedObject = new List<Collider>();

	private Dictionary<string, LinkedList<MobileObject>> _pathObjects = new Dictionary<string, LinkedList<MobileObject>>();
}
