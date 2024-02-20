using System;
using System.Collections.Generic;
using UnityEngine;

public class ManualUpdater : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			IManualUpdate component = base.transform.GetChild(i).GetComponent<IManualUpdate>();
			if (component != null)
			{
				this._all.Add(component);
			}
		}
		if (this._all.Count == 0)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		for (int i = this._lastUpdatedIndex + 1; i < this._closest.Count; i++)
		{
			if (this._closest[i].IsPossibleToActivate)
			{
				this._closest[i].ManualUpdate();
			}
			if (Time.realtimeSinceStartup - realtimeSinceStartup > this._updateMaxDuration)
			{
				this._lastUpdatedIndex = i;
				return;
			}
		}
		if (this._nextSearchingAt < realtimeSinceStartup)
		{
			this._lastUpdatedIndex = -1;
			this._nextSearchingAt = realtimeSinceStartup + this._delayBetweenClosestObjectsSearching;
			int lastIndexInAll = this._lastIndexInAll;
			float num = Time.realtimeSinceStartup - realtimeSinceStartup;
			for (;;)
			{
				IManualUpdate manualUpdate = this._all[this._lastIndexInAll];
				if (manualUpdate.Choosen)
				{
					if (!manualUpdate.IsPossibleToActivate || !manualUpdate.IsCloseEnough)
					{
						this._closest.Remove(manualUpdate);
						manualUpdate.Choosen = false;
					}
				}
				else if (this._closest.Count < this._zonesMaxCount && manualUpdate.IsPossibleToActivate && manualUpdate.IsCloseEnough)
				{
					this._closest.Add(manualUpdate);
					manualUpdate.Choosen = true;
				}
				this._lastIndexInAll++;
				if (this._lastIndexInAll == this._all.Count)
				{
					this._lastIndexInAll = 0;
				}
				if (Time.realtimeSinceStartup - realtimeSinceStartup > this._updateMaxDuration)
				{
					break;
				}
				if (this._lastIndexInAll == lastIndexInAll)
				{
					goto Block_12;
				}
			}
			return;
			Block_12:;
		}
		else if (this._lastIndexInAll >= 0)
		{
			for (int j = 0; j <= this._lastUpdatedIndex; j++)
			{
				if (this._closest[j].IsPossibleToActivate)
				{
					this._closest[j].ManualUpdate();
				}
				if (Time.realtimeSinceStartup - realtimeSinceStartup > this._updateMaxDuration)
				{
					this._lastUpdatedIndex = j;
					return;
				}
			}
			this._lastUpdatedIndex = -1;
		}
	}

	private void OnDestroy()
	{
		this._all.Clear();
	}

	[Tooltip("Maximum time budget for Update function")]
	[SerializeField]
	private float _updateMaxDuration = 0.001f;

	[Tooltip("Min delay between next searching for closest objects")]
	[SerializeField]
	private float _delayBetweenClosestObjectsSearching = 1f;

	[SerializeField]
	private int _zonesMaxCount = 50;

	private List<IManualUpdate> _all = new List<IManualUpdate>();

	private List<IManualUpdate> _closest = new List<IManualUpdate>();

	private int _lastIndexInAll;

	private int _lastUpdatedIndex = -1;

	private float _nextSearchingAt = -1f;
}
