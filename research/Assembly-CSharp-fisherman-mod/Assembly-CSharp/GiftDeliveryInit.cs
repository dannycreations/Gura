using System;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class GiftDeliveryInit : MonoBehaviour
{
	internal void Init(Player player, InventoryItem gift)
	{
		this.Title.text = string.Format(ScriptLocalization.Get("RecievedGiftText"), player.UserName);
		this.imageLdbl.Image = this.Image;
		this.imageLdbl.Load(string.Format("Textures/Inventory/{0}", gift.ThumbnailBID));
		this.Name.text = string.Concat(new object[] { gift.Name, " x<b>", gift.Count, "</b>" });
		this.Desc.text = ScriptLocalization.Get(NameResolvingHelpers.GetStorageName(gift));
	}

	public Image Image;

	private ResourcesHelpers.AsyncLoadableImage imageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Title;

	public Text Name;

	public Text Desc;
}
