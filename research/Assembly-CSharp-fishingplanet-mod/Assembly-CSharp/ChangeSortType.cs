using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSortType : MonoBehaviour
{
	public void ChangeSortTypeClick()
	{
		this.SortingInventory.SortingInventoryAction(this.sortType, base.transform.Find("Text").GetComponent<Text>().text);
	}

	public SortType sortType;

	public SortingInventory SortingInventory;
}
