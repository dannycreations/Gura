using System;
using UnityEngine;

public class FishBoxSplash : FishBoxActivity
{
	protected override void StartAction()
	{
		base.StartAction();
		Vector3 zoneRandomPos = base.GetZoneRandomPos();
		if ((GameFactory.Player.transform.position - zoneRandomPos).magnitude < this._distToPlayerToDisable)
		{
			this.StopAction();
			return;
		}
		FishBoxActivity.PlayWaterContactEffect(zoneRandomPos, this._splashPrefabName, this._splashSize, this._splashSound);
	}

	[SerializeField]
	private string _splashPrefabName = "2D/Splashes/pSplash_universal";

	[SerializeField]
	private string _splashSound = "Sounds/Actions/Lure/Lure_Out";

	[SerializeField]
	private float _splashSize = 3f;
}
