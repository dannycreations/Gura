using System;
using UnityEngine;

[Serializable]
public abstract class TutorialTriggerContainer : MonoBehaviour, ITutorialTrigger
{
	public virtual bool IsTriggering()
	{
		return this._isTriggering;
	}

	public virtual void ActivateTrigger()
	{
		this._isTriggering = true;
	}

	public virtual void DeactivateTrigger()
	{
		this._isTriggering = false;
	}

	protected bool IsHookIdleTimeout()
	{
		DateTime? dateTime = this.idleTime;
		return dateTime != null && DateTime.Now.Subtract(this.idleTime.Value).TotalSeconds >= 5.0;
	}

	protected DateTime? GetHookIdleTime()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y >= 0.0046f)
		{
			this.idleTime = null;
		}
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y < 0.0046f)
		{
			DateTime? dateTime = this.idleTime;
			if (dateTime == null && GameFactory.Player.Tackle.Hook.IsIdle)
			{
				this.idleTime = new DateTime?(DateTime.Now);
			}
		}
		return this.idleTime;
	}

	protected bool _isTriggering;

	private DateTime? idleTime;

	private const double timeoutInSeconds = 3.0;
}
