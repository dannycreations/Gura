using System;

public class EndStep25Trigger : EndTutorialTriggerContainer
{
	private void Start()
	{
		PhotonConnectionFactory.Instance.OnEndOfMissionResult += this.OnEndOfMissionResult;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnEndOfMissionResult -= this.OnEndOfMissionResult;
	}

	private void OnEndOfMissionResult(PeriodStats result)
	{
		this._isDayFinished = true;
	}

	private void Update()
	{
		if (this._isDayFinished)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	public BackToLobbyClick BackToLobbyClickInstance;

	private bool _isDayFinished;
}
