using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class QuiteClick : MonoBehaviour
{
	public void OnClick()
	{
		DisconnectServerAction.IsQuitDisconnect = true;
		GracefulDisconnectHandler.Disconnect();
	}

	public void SkipTutorial()
	{
		MenuHelpers.Instance.SkipTutorial();
	}
}
