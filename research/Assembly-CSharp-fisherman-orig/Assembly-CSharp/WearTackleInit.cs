using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class WearTackleInit : MessageBoxBase
{
	protected override void Awake()
	{
		base.Awake();
		this._alphaFade.FastHidePanel();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	public void InitContent(List<InventoryItem> items)
	{
		this.Clear();
		this._brokenList = items;
		foreach (InventoryItem inventoryItem in items)
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.WornTacklePrefab);
			WearTacklePanelHandler component = gameObject.GetComponent<WearTacklePanelHandler>();
			component.item = inventoryItem;
			component.Repaired += this.WearTackleInit_Repaired;
			component.SetDamage();
			component.LoadableImage.Load((inventoryItem.ThumbnailBID == null) ? null : string.Format("Textures/Inventory/{0}", inventoryItem.ThumbnailBID.ToString()));
			component.NameText.text = inventoryItem.Name;
		}
	}

	private void WearTackleInit_Repaired(object sender, EventArgs e)
	{
		List<InventoryItem> list = PhotonConnectionFactory.Instance.Profile.Inventory.Where(delegate(InventoryItem x)
		{
			if (x.IsRepairable)
			{
				int? maxDurability = x.MaxDurability;
				float? num3 = ((maxDurability == null) ? null : new float?((float)maxDurability.Value));
				float? num4 = ((num3 == null) ? null : new float?(num3.GetValueOrDefault() * 0.9f));
				if ((float)x.Durability < num4)
				{
					return (float)x.Durability > 0f;
				}
			}
			return false;
		}).ToList<InventoryItem>();
		List<InventoryItem> list2 = new List<InventoryItem>();
		foreach (InventoryItem inventoryItem in list)
		{
			int num;
			float num2;
			string text;
			PhotonConnectionFactory.Instance.Profile.Inventory.PreviewRepair(inventoryItem, out num, out num2, out text);
			if (num > 0)
			{
				list2.Add(inventoryItem);
			}
		}
		if (list2.Count > 0)
		{
			this.InitContent(list2);
		}
		else
		{
			this.CloseClick();
		}
	}

	private void Clear()
	{
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private void SetSizeForContent(float height)
	{
		RectTransform component = this.ContentPanel.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, Mathf.Max(height, this.ContentPanel.transform.parent.GetComponent<RectTransform>().rect.height));
		component.anchoredPosition = new Vector3(0f, 0f - height / 2f, 0f);
		GameObject gameObject = this.ContentPanel.transform.parent.Find("Scrollbar").gameObject;
		if (component.sizeDelta.y > this.ContentPanel.transform.parent.GetComponent<RectTransform>().rect.height)
		{
			gameObject.SetActive(true);
			gameObject.GetComponent<Scrollbar>().value = 1f;
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	public void RepaireAll()
	{
		if (this._brokenList == null || this._brokenList.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (InventoryItem inventoryItem in this._brokenList)
		{
			int num3;
			float num4;
			string text;
			PhotonConnectionFactory.Instance.Profile.Inventory.PreviewRepair(inventoryItem, out num3, out num4, out text);
			if (text == "SC")
			{
				num += (int)num4;
			}
			if (text == "GC")
			{
				num2 += (int)num4;
			}
		}
		bool flag = PhotonConnectionFactory.Instance.Profile.SilverCoins >= (double)num && PhotonConnectionFactory.Instance.Profile.GoldCoins >= (double)num2;
		if (flag)
		{
			string text2 = string.Format(ScriptLocalization.Get("RepaireAllItemMessage"), new object[]
			{
				num,
				MeasuringSystemManager.GetCurrencyIcon("SC"),
				"\n",
				num2,
				MeasuringSystemManager.GetCurrencyIcon("GC")
			});
			this.messageBox = this.helpers.ShowMessageSelectable(base.transform.root.gameObject, string.Empty, text2, ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, true);
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.RepaireAll_ActionCalled;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
		}
		else
		{
			string text3 = string.Format(ScriptLocalization.Get("RepaireAllHaventMoney"), new object[]
			{
				num,
				MeasuringSystemManager.GetCurrencyIcon("SC"),
				"\n",
				num2,
				MeasuringSystemManager.GetCurrencyIcon("GC")
			});
			this.messageBox = this.helpers.ShowMessage(base.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), text3, true, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
	}

	private void RepaireAll_ActionCalled(object sender, EventArgs e)
	{
		foreach (InventoryItem inventoryItem in this._brokenList)
		{
			PhotonConnectionFactory.Instance.RepairItem(inventoryItem);
			int num;
			float num2;
			string text;
			PhotonConnectionFactory.Instance.Profile.Inventory.PreviewRepair(inventoryItem, out num, out num2, out text);
			if (text == "SC")
			{
				AnalyticsFacade.WriteSpentSilver("Repair", (int)num2, 1);
			}
			if (text == "GC")
			{
				AnalyticsFacade.WriteSpentGold("Repair", (int)num2, 1);
			}
		}
		this.CompleteMessage_ActionCalled(null, null);
		this.CloseClick();
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	public void CloseClick()
	{
		this.Clear();
		this.Close();
	}

	public GameObject ContentPanel;

	public GameObject WornTacklePrefab;

	private IList<InventoryItem> _brokenList;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;
}
