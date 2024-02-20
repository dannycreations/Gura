using System;
using UnityEngine;

public class RebootMessageHandler : MonoBehaviour
{
	private void Start()
	{
		PhotonConnectionFactory.Instance.OnFarmRebootTriggered += this.Instance_OnFarmRebootTriggered;
		PhotonConnectionFactory.Instance.OnFarmRebootCanceled += this.Instance_OnFarmRebootCanceled;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnFarmRebootTriggered -= this.Instance_OnFarmRebootTriggered;
		PhotonConnectionFactory.Instance.OnFarmRebootCanceled -= this.Instance_OnFarmRebootCanceled;
	}

	private void Instance_OnFarmRebootCanceled()
	{
		if (GameFactory.Message != null)
		{
			GameFactory.Message.ShowRebootCanceledMessage();
		}
	}

	private void Instance_OnFarmRebootTriggered(TimeSpan rebootIn, int expectedDownTime)
	{
		if (GameFactory.Message != null)
		{
			GameFactory.Message.ShowRebootMessage(rebootIn, expectedDownTime);
		}
	}
}
