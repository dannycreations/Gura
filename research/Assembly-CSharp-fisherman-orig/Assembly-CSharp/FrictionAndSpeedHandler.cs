using System;
using UnityEngine;

public class FrictionAndSpeedHandler : MonoBehaviour
{
	public int FrictionCount
	{
		get
		{
			return this._currentFrictionCount;
		}
	}

	private void Update()
	{
		if (GameFactory.Player.Reel != null && GameFactory.Player.Reel.FrictionSectionsCount != this._currentFrictionCount)
		{
			this._currentFrictionCount = GameFactory.Player.Reel.FrictionSectionsCount;
			this.Speed12Friction.gameObject.SetActive(this._currentFrictionCount == 12);
			this.Speed6Friction.gameObject.SetActive(this._currentFrictionCount == 6);
			this.Speed8Friction.gameObject.SetActive(this._currentFrictionCount == 8);
		}
	}

	public void DoFade(float alpha, float time)
	{
		this.SpeedHandler.DoFade(alpha, time);
	}

	public void DoFadeSectors(float alpha, float time)
	{
		this.SpeedHandler.DoFadeSectors(alpha, time);
	}

	private Friction6SpeedHandler SpeedHandler
	{
		get
		{
			Friction6SpeedHandler friction6SpeedHandler = this.Speed6Friction;
			if (this.FrictionCount == 8)
			{
				friction6SpeedHandler = this.Speed8Friction;
			}
			else if (this.FrictionCount == 12)
			{
				friction6SpeedHandler = this.Speed12Friction;
			}
			return friction6SpeedHandler;
		}
	}

	public Friction6SpeedHandler Speed6Friction;

	public Friction8SpeedHandler Speed8Friction;

	public Friction12SpeedHandler Speed12Friction;

	private int _currentFrictionCount;
}
