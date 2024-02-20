using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WearTacklePanelHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> Repaired;

	internal void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
	}

	public virtual void RepairClick()
	{
		InventoryItem inventoryItem = this.item;
		int num;
		float num2;
		string text;
		PhotonConnectionFactory.Instance.Profile.Inventory.PreviewRepair(inventoryItem, out num, out num2, out text);
		string text2 = string.Format(ScriptLocalization.Get("RepaireItemMessage"), (int)num2, MeasuringSystemManager.GetCurrencyIcon(text), "\n");
		bool flag = (!(text == "SC") || PhotonConnectionFactory.Instance.Profile.SilverCoins >= (double)num2) && (!(text == "GC") || PhotonConnectionFactory.Instance.Profile.GoldCoins >= (double)num2);
		if (flag)
		{
			this.messageBox = this.helpers.ShowMessageSelectable(base.transform.root.gameObject, string.Empty, text2, ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, true);
			this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.Repaire_ActionCalled;
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CompleteMessage_ActionCalled;
		}
		else
		{
			text2 = string.Format(ScriptLocalization.Get("RepaireHaventMoney"), (int)num2, MeasuringSystemManager.GetCurrencyIcon(text), "\n");
			this.messageBox = this.helpers.ShowMessage(base.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), text2, true, false, false, null);
			UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
		}
	}

	private void Repaire_ActionCalled(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.RepairItem(this.item);
		int num;
		float num2;
		string text;
		PhotonConnectionFactory.Instance.Profile.Inventory.PreviewRepair(this.item, out num, out num2, out text);
		if (text == "SC")
		{
			AnalyticsFacade.WriteSpentSilver("Repair", (int)num2, 1);
		}
		if (text == "GC")
		{
			AnalyticsFacade.WriteSpentGold("Repair", (int)num2, 1);
		}
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void OnInventoryUpdated()
	{
		if (this.messageBox == null)
		{
			return;
		}
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if (this.Repaired != null)
		{
			this.Repaired(this, new EventArgs());
		}
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	public void SetDamage()
	{
		Sprite transparentSprite = ResourcesHelpers.GetTransparentSprite();
		float num = (float)this.item.Durability / (float)this.item.MaxDurability.Value;
		this.Durability.fillAmount = num;
		if (num < 0.33f)
		{
			this.Durability.color = ColorManager.DurabilityLowColor;
		}
		else
		{
			this.Durability.color = ((num > 0.66f) ? ColorManager.DurabilityHighColor : ColorManager.DurabilityNormalColor);
		}
		float num2 = (float)this.item.Durability / (float)this.item.MaxDurability.Value * 100f;
		string text = "#ffffffff";
		if (num2 < 33f)
		{
			text = "#ff0000ff";
		}
		else if (num2 <= 66f)
		{
			text = "#ffc300ff";
		}
		this.textDurability.text = string.Format("{0}: <color={3}>{1}</color>/{2}; ", new object[]
		{
			ScriptLocalization.Get("DurabilityCaption"),
			this.item.Durability,
			this.item.MaxDurability,
			text
		});
	}

	public InventoryItem item;

	public Image Durability;

	public ResourcesHelpers.AsyncLoadableImage LoadableImage;

	public Text NameText;

	public Text textDurability;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;
}
