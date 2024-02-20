using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class SortingInventory : MonoBehaviour
{
	public string TextCaption { get; set; }

	private void Start()
	{
		this.SortType = SortType.LevelLow;
		base.transform.Find("Text").GetComponent<Text>().text = string.Format("<b>{0}</b>", ScriptLocalization.Get("SortLevelLowestMenu"));
	}

	public void SortingInventoryAction(SortType sortType, string textCaption)
	{
		if (sortType == this.SortType)
		{
			return;
		}
		this.TextCaption = textCaption;
		base.transform.Find("Text").GetComponent<Text>().text = string.Format("<b>{0}</b>", this.TextCaption);
		this.SortType = sortType;
		this.updateContentItems.SortCurrentContent(sortType);
	}

	[HideInInspector]
	public SortType SortType = SortType.LevelLow;

	public UpdateContentItems updateContentItems;
}
