using System;
using System.Collections.Generic;
using UnityEngine;

public class LazyManualUpdater : MonoBehaviour
{
	public static LazyManualUpdater Instance { get; private set; }

	private void Awake()
	{
		if (LazyManualUpdater.Instance != null)
		{
			LogHelper.Error("We decide to have only one copy of LazyManualUpdater per scene! Object {0} will be not forceUpdated when necessary", new object[] { base.name });
			return;
		}
		LazyManualUpdater.Instance = this;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			ILazyManualUpdate component = base.transform.GetChild(i).GetComponent<ILazyManualUpdate>();
			if (component != null)
			{
				this._all.Add(component);
			}
		}
		if (this._all.Count == 0)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			this._updateMaxDuration = Mathf.Min(this._updateMaxDuration, (float)this._all.Count);
		}
	}

	public void ForceUpdate()
	{
		this._timer = 0f;
		this._forceUpdate = true;
		this.Update();
		this._forceUpdate = false;
	}

	private void Update()
	{
		if (GameFactory.IsPlayerInitialized && !this._wasInitialized)
		{
			this._wasInitialized = true;
			for (int i = 0; i < this._all.Count; i++)
			{
				this._all[i].Init(GameFactory.Player.transform);
			}
			this.ForceUpdate();
		}
		this._timer -= Time.deltaTime;
		if (this._timer > 0f)
		{
			return;
		}
		this._timer = this._delayBetweenUpdates;
		int lastIndex = this._lastIndex;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		do
		{
			this._all[this._lastIndex].ManualUpdate();
			this._lastIndex++;
			if (this._lastIndex == this._all.Count)
			{
				this._lastIndex = 0;
			}
			if (this._lastIndex == lastIndex)
			{
				break;
			}
		}
		while (Time.realtimeSinceStartup - realtimeSinceStartup < this._updateMaxDuration || this._forceUpdate);
	}

	private void OnDestroy()
	{
		this._all.Clear();
	}

	[SerializeField]
	private float _updateMaxDuration = 0.001f;

	[SerializeField]
	private float _delayBetweenUpdates = 4.5f;

	private List<ILazyManualUpdate> _all = new List<ILazyManualUpdate>();

	private int _lastIndex;

	private bool _wasInitialized;

	private float _timer;

	private bool _forceUpdate;
}
