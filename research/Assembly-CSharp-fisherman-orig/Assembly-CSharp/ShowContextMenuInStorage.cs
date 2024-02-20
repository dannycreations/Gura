using System;
using ObjectModel;
using UnityEngine;

public class ShowContextMenuInStorage : ShowContextMenu
{
	private void Start()
	{
		this.RootPanel = base.transform.parent.parent.parent.parent.parent.parent.gameObject;
	}

	public override GameObject ContentPrefab
	{
		get
		{
			return this.ContentMenuPrefab;
		}
	}

	protected override void Show()
	{
		base.Show();
		int num = 0;
		this._content.GetComponent<InventoryItemComponent>().InventoryItem = base.GetComponent<InventoryItemComponent>().InventoryItem;
		if (base.GetComponent<InventoryItemComponent>().Storage == StoragePlaces.Storage || !PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(this._content.GetComponent<InventoryItemComponent>().InventoryItem, null, StoragePlaces.Storage, true))
		{
			this._content.transform.Find("ToStorage").gameObject.SetActive(false);
			num++;
		}
		if (base.GetComponent<InventoryItemComponent>().Storage == StoragePlaces.Equipment || !PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(this._content.GetComponent<InventoryItemComponent>().InventoryItem, null, StoragePlaces.Equipment, true))
		{
			this._content.transform.Find("ToEquipment").gameObject.SetActive(false);
			num++;
		}
		if (base.GetComponent<InventoryItemComponent>().Storage == StoragePlaces.CarEquipment || !PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(this._content.GetComponent<InventoryItemComponent>().InventoryItem, null, StoragePlaces.CarEquipment, true))
		{
			this._content.transform.Find("ToCar").gameObject.SetActive(false);
			num++;
		}
		float num2 = (float)num * 17.5f;
		this._context.GetComponent<RectTransform>().sizeDelta = new Vector2(this._context.GetComponent<RectTransform>().sizeDelta.x, this._context.GetComponent<RectTransform>().sizeDelta.y - num2 + 6f);
		this._content.GetComponent<RectTransform>().sizeDelta = new Vector2(this._content.GetComponent<RectTransform>().sizeDelta.x, this._content.GetComponent<RectTransform>().sizeDelta.y - num2 + 4f);
		this._content.GetComponent<EventAction>().ActionCalled += this.CloseMenu_ActionCalled;
	}

	private void CloseMenu_ActionCalled(object sender, EventArgs e)
	{
		this._context.GetComponent<ContextMenuAction>().Hide();
	}

	public GameObject ContentMenuPrefab;
}
