using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSortFishnet : MonoBehaviour
{
	public void ChangeSortTypeClick()
	{
		this.SortingFishnet.SortingInventoryAction(this.sortType, base.transform.Find("Text").GetComponent<Text>().text);
	}

	public SortType sortType;

	public SortingFishkeepnet SortingFishnet;
}
