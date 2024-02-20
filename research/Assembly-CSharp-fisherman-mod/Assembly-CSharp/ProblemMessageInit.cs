using System;
using UnityEngine;
using UnityEngine.UI;

public class ProblemMessageInit : MonoBehaviour
{
	public void Init(string messageText)
	{
		this.Desc.text = messageText;
	}

	public Text Desc;
}
