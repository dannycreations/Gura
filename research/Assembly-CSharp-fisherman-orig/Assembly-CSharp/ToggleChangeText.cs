using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleChangeText : MonoBehaviour
{
	private void Start()
	{
		this.OnChange();
	}

	public void OnChange()
	{
		this.Text.text = ((!base.GetComponent<Toggle>().isOn) ? this.offText : this.onText);
	}

	public Text Text;

	public string onText = "\ue625";

	public string offText = "\ue626";
}
