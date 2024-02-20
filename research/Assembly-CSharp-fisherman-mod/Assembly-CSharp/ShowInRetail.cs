using System;
using UnityEngine;

public class ShowInRetail : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < this.RetailActions.Length; i++)
		{
			ShowInRetail.RetailAction retailAction = this.RetailActions[i];
			for (int j = 0; j < retailAction.Objects.Length; j++)
			{
				retailAction.Objects[j].SetActive(retailAction.WhatDoing == ShowInRetail.State.Show);
			}
		}
	}

	[Header("RETAIL")]
	public ShowInRetail.RetailAction[] RetailActions;

	[Header("NOT_RETAIL")]
	public ShowInRetail.RetailAction[] NonRetailActions;

	[Serializable]
	public class RetailAction
	{
		public ShowInRetail.State WhatDoing = ShowInRetail.State.Hide;

		public GameObject[] Objects;
	}

	public enum State
	{
		Show,
		Hide
	}
}
