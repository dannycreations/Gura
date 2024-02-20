using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageInfo : MonoBehaviour
{
	public string Message
	{
		set
		{
			this.text.text = value;
		}
	}

	public Text text;
}
