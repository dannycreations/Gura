using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PremiumPassInfoPanelInit : MonoBehaviour
{
	public void Init(StoreProduct product)
	{
		this.Desc.text = string.Format(product.Desc, "\n", "\t");
	}

	public Text Desc;
}
