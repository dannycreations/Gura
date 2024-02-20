using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class DetailsLicense : MonoBehaviour
{
	public void Show(ShopLicense item)
	{
		this.Name.text = item.Name;
		this.Description.text = item.Desc;
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}

	public Text Name;

	public Text Description;
}
