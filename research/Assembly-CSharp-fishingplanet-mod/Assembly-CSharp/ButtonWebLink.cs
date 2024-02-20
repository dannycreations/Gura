using System;
using UnityEngine;

public class ButtonWebLink : MonoBehaviour
{
	public void OnClick()
	{
		Application.OpenURL(this.URL);
	}

	public string URL;
}
