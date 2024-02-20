using System;
using ObjectModel;
using UnityEngine;

public class FishBite : FishStateBase
{
	public override void Transition(FsmBaseState<IFishController> source)
	{
		FishBite fishBite = source as FishBite;
		this.biteAge = fishBite.biteAge;
		this.biteTotalTime = fishBite.biteTotalTime;
		this.renewedBiteAge = fishBite.renewedBiteAge;
		this._isBiteRequestSend = fishBite._isBiteRequestSend;
		base.Fish.Tackle.Fish = base.Fish;
		base.Fish.Bite();
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(true);
			base.RodSlot.Bell.HighSensitivity(true);
		}
		if (base.IsInHands && base.RodSlot.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
		{
			GameFactory.QuiverIndicator.HighSensitivity(true);
		}
	}

	protected override void onEnter()
	{
		base.Fish.Tackle.Fish = base.Fish;
		if (SettingsManager.BobberBiteSound && base.IsBellActive)
		{
			this._playSoundAt = Time.time + 2f;
		}
		this.renewedBiteAge = false;
		this.biteAge = 0f;
		this.biteTotalTime = base.Fish.BiteTime;
		if (this.biteTotalTime == 0f)
		{
			this.biteTotalTime = Random.Range(10f, 30f);
		}
		else if (this.biteTotalTime >= 5f)
		{
			float num = Random.Range(-10f, 10f);
			float num2 = this.biteTotalTime + num;
			if (num2 >= 5f && num2 <= 60f)
			{
				this.biteTotalTime = num2;
			}
		}
		base.Fish.Bite();
		if (base.Fish.FishAiIsBiteTemplate && this.biteTotalTime >= 5f)
		{
			this.biteTotalTime = base.Fish.FishAiMaxBiteTime;
		}
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(true);
			base.RodSlot.Bell.HighSensitivity(true);
		}
		if (base.IsInHands && base.RodSlot.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
		{
			GameFactory.QuiverIndicator.HighSensitivity(true);
		}
	}

	protected override Type onUpdate()
	{
		if (this._playSoundAt > 0f && Time.time > this._playSoundAt && base.IsInHands)
		{
			this._playSoundAt = -1f;
			RandomSounds.PlaySoundAtPoint("Sounds/incoming_bite3", GameFactory.Player.transform.position, 10f, false);
		}
		if (base.Fish.Behavior == FishBehavior.Go)
		{
			return typeof(FishEscape);
		}
		if (base.Fish.Behavior == FishBehavior.Hook)
		{
			return typeof(FishHooked);
		}
		if (!this.renewedBiteAge && base.Fish.FishAiIsBiting)
		{
			this.biteAge = 0f;
			this.renewedBiteAge = true;
		}
		this.biteAge += Time.deltaTime;
		if (!this._isBiteRequestSend && (base.Fish.FishAiIsBiting || this.biteAge > this.biteTotalTime * 0.7f))
		{
			this._isBiteRequestSend = true;
			base.Adapter.ConfirmFishBite(base.Fish.InstanceGuid);
		}
		if (base.Fish.FishAiEndOfBite || this.biteAge > this.biteTotalTime)
		{
			return typeof(FishSwimAway);
		}
		return null;
	}

	protected override void onExit()
	{
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(false);
		}
		if (base.IsInHands && base.NextState != typeof(FishSwimAway) && base.RodSlot.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
		{
			GameFactory.QuiverIndicator.HighSensitivity(false);
		}
	}

	private const float DELAY_BEFORE_SOUND = 2f;

	private float biteAge;

	private float biteTotalTime;

	private bool renewedBiteAge;

	private bool _isBiteRequestSend;

	private float _playSoundAt = -1f;
}
