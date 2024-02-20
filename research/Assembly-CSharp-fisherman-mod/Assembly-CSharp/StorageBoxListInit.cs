using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class StorageBoxListInit : MessageBoxBase
{
	internal override void Start()
	{
		base.Start();
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
	}

	public void Init(List<StoreProduct> products)
	{
		this.FirstToggle = null;
		products.Sort((StoreProduct a, StoreProduct b) => a.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow).CompareTo(b.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow)));
		foreach (StoreProduct storeProduct in products)
		{
			GameObject gameObject = GUITools.AddChild(this.ScrollContent, this.StorageBoxPrefab);
			StorageBoxItem component = gameObject.GetComponent<StorageBoxItem>();
			component.Product = storeProduct;
			component.FillData(new EventHandler<EventArgs>(this.OnSelected), false);
			component.Toggle.group = this.Group;
			if (this.FirstToggle == null)
			{
				this.FirstToggle = component.Toggle;
			}
		}
		if (this.FirstToggle != null)
		{
			this.FirstToggle.Select();
			this.FirstToggle.isOn = true;
		}
	}

	public void Buy()
	{
		if (this._selectedBoxItem != null)
		{
			this._selectedBoxItem.Buy();
		}
	}

	public void OnSelected(object sender, EventArgs e)
	{
		this._selectedBoxItem = (StorageBoxItem)sender;
		this.ConsoleBuyHelpButton.GetComponent<BorderedButton>().interactable = this._selectedBoxItem.BuyButton.enabled;
	}

	public void OnProductBought(ProfileProduct p, int count)
	{
		StorageBoxItem[] componentsInChildren = this.ScrollContent.GetComponentsInChildren<StorageBoxItem>();
		foreach (StorageBoxItem storageBoxItem in componentsInChildren)
		{
			if (this.FirstToggle == null)
			{
				this.FirstToggle = storageBoxItem.Toggle;
			}
			storageBoxItem.FillData(new EventHandler<EventArgs>(this.OnSelected), false);
		}
		if (this.FirstToggle != null)
		{
			this.FirstToggle.Select();
			this.FirstToggle.isOn = true;
		}
	}

	public GameObject ScrollContent;

	public Button CloseButton;

	public GameObject StorageBoxPrefab;

	public ToggleGroup Group;

	public Text TitleText;

	private Toggle FirstToggle;

	public GameObject ConsoleBuyHelpButton;

	private StorageBoxItem _selectedBoxItem;
}
