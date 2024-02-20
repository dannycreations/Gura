using System;
using System.Collections;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CatchedItemWindow : CatchedInfoBase
{
	private void Awake()
	{
		this._catchedInfoType = CatchedInfoTypes.Item;
		base.GetComponent<CanvasGroup>().alpha = 1f;
		base.gameObject.SetActive(false);
	}

	public void Activate(string itemName, string itemType, GameFactory.RodSlot rodSlot)
	{
		this._noMorePlaceToEquip.gameObject.SetActive(false);
		this._isInPhotoMode = false;
		base.gameObject.SetActive(true);
		this._itemName.text = itemName;
		this._itemType.text = itemType;
		this.UpdateHelp();
		this._takeButton.interactable = false;
		base.StartCoroutine(this.WaitForItem(rodSlot));
	}

	private IEnumerator WaitForItem(GameFactory.RodSlot rodSlot)
	{
		while (rodSlot.Tackle.CaughtItem == null)
		{
			yield return null;
		}
		Profile p = PhotonConnectionFactory.Instance.Profile;
		GameFactory.ChatListener.OnLocalEvent(new LocalEvent
		{
			EventType = LocalEventType.ItemCaught,
			Player = PlayerProfileHelper.ProfileToPlayer(p),
			CaughtItem = rodSlot.Tackle.CaughtItem
		});
		bool isCreature = this.IsCreature(rodSlot);
		this._releaseButtonText.text = ((!isCreature) ? ScriptLocalization.Get("DiscardButton").ToUpper() : ScriptLocalization.Get("ReleaseButton").ToUpper());
		this._takeButton.interactable = p.Inventory.CanMoveOrCombineItem(rodSlot.Tackle.CaughtItem, null, StoragePlaces.Equipment);
		if (this._takeButton.interactable)
		{
			this._takeButton.interactable = !isCreature;
		}
		else if (p.Inventory.LastVerificationError.Contains("Can't equip any more items"))
		{
			this._noMorePlaceToEquip.gameObject.SetActive(!isCreature);
		}
		yield break;
	}

	public void OnTake()
	{
		base.gameObject.SetActive(false);
		this.EClose(true);
	}

	public void OnRelease()
	{
		base.gameObject.SetActive(false);
		this.EClose(false);
	}

	protected override void UpdateHelp()
	{
		this._lookHotkey.text = ((InputModuleManager.GameInputType != InputModuleManager.InputType.GamePad) ? string.Format(ScriptLocalization.Get("ExamineTrashHotkey"), this.GetColored(ScriptLocalization.Get("RMBCaption"))) : string.Format(ScriptLocalization.Get("ExamineTrashHotkey"), this.GetKeyMapping(InputControlType.RightBumper) + " + " + this.GetKeyMapping(InputControlType.RightStickRight)));
	}

	private bool IsCreature(GameFactory.RodSlot rodSlot)
	{
		return rodSlot.Tackle.CaughtItem.ItemSubType == ItemSubTypes.Creature;
	}

	[SerializeField]
	private Text _releaseButtonText;

	[SerializeField]
	private Button _takeButton;

	[SerializeField]
	private Text _itemName;

	[SerializeField]
	private Text _itemType;

	[SerializeField]
	private Text _lookHotkey;

	[SerializeField]
	private Text _noMorePlaceToEquip;
}
