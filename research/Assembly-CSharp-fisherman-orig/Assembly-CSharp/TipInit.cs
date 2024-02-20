using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TipInit : MonoBehaviour
{
	public void Init(string tipText)
	{
		this.text.text = string.Format("<color=#c3986d>" + ScriptLocalization.Get("TipText") + ":</color> {0}", tipText);
	}

	public Text text;
}
