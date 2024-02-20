using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Friction12SpeedHandler : Friction8SpeedHandler
{
	protected override void Awake()
	{
		base.Awake();
		List<GameObject> list = this.Sectors.ToList<GameObject>();
		list.AddRange(new GameObject[] { this.Sector9, this.Sector10, this.Sector11, this.Sector12 });
		this.Sectors = list.ToArray();
	}

	protected override void UpdateFriction()
	{
		base.UpdateFriction();
		this.Sector9.SetActive(false);
		this.Sector10.SetActive(false);
		this.Sector11.SetActive(false);
		this.Sector12.SetActive(false);
		if ((byte)this._currentFrictionSpeed > 8)
		{
			this.Sector9.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 9)
		{
			this.Sector10.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 10)
		{
			this.Sector11.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 11)
		{
			this.Sector12.SetActive(true);
		}
	}

	public GameObject Sector9;

	public GameObject Sector10;

	public GameObject Sector11;

	public GameObject Sector12;
}
