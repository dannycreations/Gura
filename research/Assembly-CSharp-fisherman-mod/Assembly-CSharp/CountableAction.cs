using System;
using Assets.Scripts.UI._2D.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class CountableAction : MonoBehaviour
{
	internal void Start()
	{
		this._ii = base.GetComponent<InventoryItemComponent>();
		this.OnInventoryUpdated();
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
	}

	private void OnInventoryUpdated()
	{
		string text = InventoryHelper.ItemCountStr(this._ii.InventoryItem);
		this.CountableValue.text = text;
		this.CountableImage.SetActive(!string.IsNullOrEmpty(text));
	}

	public GameObject CountableImage;

	public Text CountableValue;

	private InventoryItemComponent _ii;
}
