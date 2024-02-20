using System;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuPress : MonoBehaviour
{
	private void OnEnable()
	{
		base.gameObject.SetActive(this.actionButton.IsActive());
	}

	public void OnClick()
	{
		if (this.actionButton.IsActive())
		{
			this.actionButton.interactable = true;
			this.actionButton.OnSubmit(null);
			this.actionButton.interactable = false;
		}
	}

	[SerializeField]
	private Button actionButton;
}
