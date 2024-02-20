using System;
using UnityEngine;

public class FishShowBig : FishStateBase
{
	public override TPMFishState State
	{
		get
		{
			return TPMFishState.ShowBig;
		}
	}

	protected override void onEnter()
	{
		GameFactory.FishSpawner.DestroyFishCam();
		this.timeSpent = 0f;
	}

	protected override Type onUpdate()
	{
		if (!base.Fish.Tackle.IsShowing)
		{
			return typeof(FishDestroy);
		}
		this.timeSpent += Time.deltaTime;
		if (this.timeSpent > 1.5f)
		{
			base.Fish.IsFrozen = true;
		}
		return null;
	}

	private Transform HandTransform
	{
		get
		{
			Transform transform = null;
			ReelTypes reelType = base.RodSlot.Reel.ReelType;
			if (reelType != ReelTypes.Spinning)
			{
				if (reelType == ReelTypes.Baitcasting)
				{
					transform = GameFactory.Player.LinePositionInRightHand;
				}
			}
			else
			{
				transform = GameFactory.Player.LinePositionInLeftHand;
			}
			return transform;
		}
	}

	private float BigFishRotationOffset
	{
		get
		{
			float num = 0f;
			ReelTypes reelType = base.RodSlot.Reel.ReelType;
			if (reelType != ReelTypes.Spinning)
			{
				if (reelType == ReelTypes.Baitcasting)
				{
					num = this.FishBaitcastRotationOffset;
				}
			}
			else
			{
				num = this.FishRegularRotationOffset;
			}
			return num;
		}
	}

	public const float FirstPhaseLength = 1.5f;

	public const float SecondPhaseLength = 1f;

	private float FishRegularRotationOffset = -30f;

	private float FishBaitcastRotationOffset = 35f;

	private float timeSpent;
}
