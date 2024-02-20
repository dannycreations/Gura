using System;
using UnityEngine;
using UnityEngine.UI;

public class FatalProblemMessageInit : MonoBehaviour
{
	public void Init(string messageText)
	{
		this.Desc.text = messageText;
	}

	public void QuitClick()
	{
		DisconnectServerAction.IsQuitDisconnect = true;
		GracefulDisconnectHandler.Disconnect();
	}

	public Text Desc;
}
