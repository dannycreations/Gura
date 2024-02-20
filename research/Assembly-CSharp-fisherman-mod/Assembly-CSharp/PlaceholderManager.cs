using System;
using UnityEngine;
using UnityEngine.UI;

public class PlaceholderManager : MonoBehaviour
{
	private void Update()
	{
		if (base.transform.parent.GetComponent<InventoryItemComponent>() != null && base.transform.parent.GetComponent<InventoryItemComponent>().InventoryItem != null)
		{
			base.GetComponent<Image>().enabled = false;
		}
		else
		{
			base.GetComponent<Image>().enabled = true;
		}
	}
}
