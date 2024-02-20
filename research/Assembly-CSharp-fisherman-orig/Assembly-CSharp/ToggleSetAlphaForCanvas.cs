using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSetAlphaForCanvas : MonoBehaviour
{
	private void Update()
	{
		if (base.GetComponent<Toggle>().isOn)
		{
			this.content.gameObject.SetActive(true);
		}
		else
		{
			this.content.gameObject.SetActive(false);
		}
	}

	public Canvas content;

	public int alpha = 1;
}
