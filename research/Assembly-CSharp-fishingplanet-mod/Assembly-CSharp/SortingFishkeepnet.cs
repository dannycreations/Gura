using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class SortingFishkeepnet : MonoBehaviour
{
	public string TextCaption { get; set; }

	public void SortingInventoryAction(SortType sortType, string textCaption)
	{
		if (sortType == this.SortTypeInstance)
		{
			return;
		}
		this.TextCaption = textCaption;
		base.transform.Find("Text").GetComponent<Text>().text = string.Format("<b>{0}</b>", this.TextCaption);
		this.SortTypeInstance = sortType;
		this.updateContentItems.SortCurrentContent(sortType);
	}

	private void Start()
	{
		this.SortingInventoryAction(SortType.Name, ScriptLocalization.Get("SortNameMenu"));
	}

	[HideInInspector]
	public SortType SortTypeInstance = SortType.Type;

	public FishKeepnetInit updateContentItems;
}
