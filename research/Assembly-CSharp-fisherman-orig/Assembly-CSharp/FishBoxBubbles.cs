using System;
using UnityEngine;

public class FishBoxBubbles : FishBoxActivity
{
	public override bool ManualUpdate()
	{
		if (base.ManualUpdate())
		{
			if (this._nextBubbleAt > 0f && this._nextBubbleAt < Time.time)
			{
				this.CreateBubble(null);
			}
			return true;
		}
		return false;
	}

	protected override void StartAction()
	{
		base.StartAction();
		Vector3 zoneRandomPos = base.GetZoneRandomPos();
		if ((GameFactory.Player.transform.position - zoneRandomPos).magnitude < this._distToPlayerToDisable)
		{
			this.StopAction();
			return;
		}
		this.CreateBubble(new Vector3?(zoneRandomPos));
	}

	protected override void StopAction()
	{
		base.StopAction();
		this._nextBubbleAt = -1f;
	}

	private void CreateBubble(Vector3? actionPos = null)
	{
		this._nextBubbleAt = Time.time + this._timeBetweenBubbles;
		if (GameFactory.Water != null)
		{
			Vector3 vector = ((actionPos == null) ? base.GetZoneRandomPos() : actionPos.Value);
			GameFactory.Water.AddWaterDisturb(vector, this._bubblesRadius, this._bubblesType);
			if (this._enableSplashes)
			{
				FishBoxActivity.PlayWaterContactEffect(vector, this._splashPrefabName, this._splashSize, this._splashSound);
			}
		}
	}

	[SerializeField]
	private float _timeBetweenBubbles;

	[SerializeField]
	private float _bubblesRadius;

	[SerializeField]
	private WaterDisturbForce _bubblesType;

	[SerializeField]
	private bool _enableSplashes;

	[SerializeField]
	private string _splashPrefabName = "2D/Splashes/pSplash_universal";

	[SerializeField]
	private string _splashSound = "Sounds/Actions/Lure/Lure_Out";

	[SerializeField]
	private float _splashSize = 0.1f;

	private float _nextBubbleAt = -1f;
}
