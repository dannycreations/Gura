using System;
using UnityEngine;
using UnityEngine.UI;

public class TurnOnScrollBar : MonoBehaviour
{
	private void Update()
	{
		if (this.ContentPanel.GetComponent<RectTransform>().rect.height > this.MaskPanel.GetComponent<RectTransform>().rect.height)
		{
			if (!this.ScrollBar.activeSelf)
			{
				this.ScrollBar.SetActive(true);
				this.ScrollBar.GetComponent<Scrollbar>().value = 1f;
			}
		}
		else if (this.ScrollBar.activeSelf)
		{
			this.ScrollBar.SetActive(false);
		}
	}

	private void OnEnable()
	{
		this.ScrollBar.GetComponent<Scrollbar>().value = 1f;
	}

	public GameObject ScrollBar;

	public GameObject ContentPanel;

	public GameObject MaskPanel;
}
