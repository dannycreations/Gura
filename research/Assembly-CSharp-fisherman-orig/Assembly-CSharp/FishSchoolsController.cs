using System;
using System.Collections.Generic;
using UnityEngine;

public class FishSchoolsController : MonoBehaviour
{
	private void Awake()
	{
		List<SchoolChild> list = null;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			SchoolController component = base.transform.GetChild(i).GetComponent<SchoolController>();
			if (component != null)
			{
				this._schools.Add(component);
				if (i == 0)
				{
					component.Init(this.commonMinFishScale, this.commonMaxFishScale, this._sharedFish, true, null);
					if (this._sharedFish)
					{
						list = component.Roamers;
					}
				}
				else
				{
					component.Init(this.commonMinFishScale, this.commonMaxFishScale, this._sharedFish, !this._sharedFish, list);
				}
			}
		}
	}

	private void Update()
	{
		this._nextUpdateTimer -= Time.deltaTime;
		if (this._nextUpdateTimer > 0f)
		{
			return;
		}
		this._nextUpdateTimer = (float)this._updatesInterval;
		int num = -1;
		float num2 = this.activationDist;
		for (int i = 0; i < this._schools.Count; i++)
		{
			float magnitude = (this.player.position - this._schools[i].transform.position).magnitude;
			if (magnitude < num2)
			{
				num = i;
				num2 = magnitude;
			}
		}
		if (this._curSchoolIndex != num)
		{
			int curSchoolIndex = this._curSchoolIndex;
			if (this._curSchoolIndex != -1)
			{
				this._schools[this._curSchoolIndex].SetLogicFlag(false, false);
				this._curSchoolIndex = -1;
			}
			if (num != -1)
			{
				bool flag = this._sharedFish && (curSchoolIndex == -1 || (this._schools[num].transform.position - this._schools[curSchoolIndex].transform.position).magnitude > this._fishTeleportationDist);
				this._schools[num].SetLogicFlag(true, flag);
				this._curSchoolIndex = num;
			}
		}
	}

	[SerializeField]
	private Transform player;

	[SerializeField]
	private float activationDist;

	[SerializeField]
	private float commonMinFishScale;

	[SerializeField]
	private float commonMaxFishScale;

	[SerializeField]
	private byte _updatesInterval = 5;

	[SerializeField]
	private bool _sharedFish = true;

	[SerializeField]
	private float _fishTeleportationDist = 30f;

	private List<SchoolController> _schools = new List<SchoolController>();

	private int _curSchoolIndex = -1;

	private float _nextUpdateTimer;
}
