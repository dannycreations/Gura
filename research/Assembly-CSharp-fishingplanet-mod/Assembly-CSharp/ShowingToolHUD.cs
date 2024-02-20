using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ShowingToolHUD : MonoBehaviour
{
	private void Start()
	{
		this.ContentPanel.FastHidePanel();
	}

	private void Update()
	{
		if (GameFactory.Player != null && GameFactory.Player.CurFirework != null)
		{
			if (!this.ContentPanel.IsShow && !this.ContentPanel.IsShowing)
			{
				this.ContentPanel.ShowPanel();
			}
			this.ShowInfoPanel();
		}
		else if (this.ContentPanel.IsShow || this.ContentPanel.IsShowing)
		{
			this.ContentPanel.HidePanel();
		}
	}

	private void ShowInfoPanel()
	{
		InventoryItem item = GameFactory.Player.CurFirework.Item;
		if (item.InstanceId != null && (item.InstanceId.Value != this._itemId || item.Count != this._itemCounts))
		{
			this._itemId = item.InstanceId.Value;
			this._itemCounts = item.Count;
			this.LureImg.Image = this.Lure.GetComponent<Image>();
			this.LureImg.Load((item.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", item.ThumbnailBID.ToString()));
			this.Info.GetComponent<Text>().text = item.Name;
			this.LureCount.GetComponent<Text>().text = item.Count.ToString();
		}
	}

	public AlphaFade ContentPanel;

	public GameObject Lure;

	private ResourcesHelpers.AsyncLoadableImage LureImg = new ResourcesHelpers.AsyncLoadableImage();

	public GameObject LureCount;

	public GameObject Info;

	private int _itemCounts;

	private Guid _itemId;
}
