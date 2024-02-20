using System;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ProductDeliveryInit : MonoBehaviour
{
	public void Init(ProfileProduct product, bool excess = false, bool nothingDelivered = false)
	{
		if (nothingDelivered)
		{
			this.Icon.gameObject.SetActive(false);
			this.Name.text = ScriptLocalization.Get("NothingDelivered");
			return;
		}
		string text = product.Name;
		if (excess)
		{
			text = string.Format(ScriptLocalization.Get("GiftPlacedToEscessTab"), "\n" + product.Name);
		}
		this.Name.text = text;
		if (product.ImageBID != null)
		{
			this.IconLoadable.Image = this.Icon;
			this.IconLoadable.Load(string.Format("Textures/Inventory/{0}", product.ImageBID));
		}
	}

	public Text Name;

	public Image Icon;

	private ResourcesHelpers.AsyncLoadableImage IconLoadable = new ResourcesHelpers.AsyncLoadableImage();
}
