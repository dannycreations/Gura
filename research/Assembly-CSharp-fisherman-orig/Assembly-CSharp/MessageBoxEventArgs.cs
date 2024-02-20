using System;
using UnityEngine;

public class MessageBoxEventArgs : EventArgs
{
	public MessageBoxEventArgs(GameObject messageBoxPanel)
	{
		this.MessageBoxPanel = messageBoxPanel;
	}

	public GameObject MessageBoxPanel;
}
