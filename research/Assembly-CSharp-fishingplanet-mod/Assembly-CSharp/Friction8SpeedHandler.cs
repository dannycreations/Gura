using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Friction8SpeedHandler : Friction6SpeedHandler
{
	protected override void Awake()
	{
		base.Awake();
		List<GameObject> list = this.Sectors.ToList<GameObject>();
		list.AddRange(new GameObject[] { this.Sector7, this.Sector8 });
		this.Sectors = list.ToArray();
	}

	protected override void UpdateFriction()
	{
		if (GameFactory.Player.Reel == null)
		{
			return;
		}
		this._currentFrictionSpeed = GameFactory.Player.Reel.CurrentFrictionSection;
		this.Sector1.SetActive(false);
		this.Sector2.SetActive(false);
		this.Sector3.SetActive(false);
		this.Sector4.SetActive(false);
		this.Sector5.SetActive(false);
		this.Sector6.SetActive(false);
		this.Sector7.SetActive(false);
		this.Sector8.SetActive(false);
		if ((byte)this._currentFrictionSpeed > 0)
		{
			this.Sector1.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 1)
		{
			this.Sector2.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 2)
		{
			this.Sector3.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 3)
		{
			this.Sector4.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 4)
		{
			this.Sector5.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 5)
		{
			this.Sector6.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 6)
		{
			this.Sector7.SetActive(true);
		}
		if ((byte)this._currentFrictionSpeed > 7)
		{
			this.Sector8.SetActive(true);
		}
	}

	public GameObject Sector7;

	public GameObject Sector8;
}
