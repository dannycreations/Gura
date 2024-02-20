using System;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class StorageBoxesPopupInit : MonoBehaviour
{
	public void OnEnable()
	{
		this.OnInventoryUpdated();
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
	}

	public void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
	}

	private void OnInventoryUpdated()
	{
		base.GetComponent<Button>().interactable = PhotonConnectionFactory.Instance.Profile.Inventory.CurrentInventoryCapacity < Inventory.MaxInventoryCapacity;
	}

	public void ShowAvailableStorageBoxes()
	{
		this._messageBox = MenuHelpers.Instance.ShowBuyProductsOfTypeWindow(null, ScriptLocalization.Get("StorageBoxPopupLabel"), 5);
	}

	public void ClosePopup()
	{
		this._messageBox.GetComponent<AlphaFade>().HidePanel();
	}

	private GameObject _messageBox;
}
