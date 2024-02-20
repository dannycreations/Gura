using System;
using I2.Loc;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DeEquip : EquipBase, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	protected override void DblClick(PointerEventData eventData)
	{
		this.UnEquip();
	}

	private void Awake()
	{
		this.trigger = base.transform.parent.GetComponentInChildren<EventTriggerCustom>();
		if (this.trigger != null)
		{
			this.trigger.OnSelected.AddListener(new UnityAction<BaseEventData>(this.OnSelect));
			this.trigger.OnDeselected.AddListener(new UnityAction<BaseEventData>(this.OnDeselect));
		}
	}

	private void OnDestroy()
	{
		if (this.trigger != null)
		{
			this.trigger.OnSelected.RemoveListener(new UnityAction<BaseEventData>(this.OnSelect));
			this.trigger.OnDeselected.RemoveListener(new UnityAction<BaseEventData>(this.OnDeselect));
		}
	}

	private void Highlight()
	{
		if (this._iiComponent != null && ShowDetailedDollInfo.Instance != null)
		{
			ShowDetailedDollInfo.Instance.Highlight(this._iiComponent, true);
		}
	}

	private void Dehighlight()
	{
		if (this._iiComponent != null && ShowDetailedDollInfo.Instance != null)
		{
			ShowDetailedDollInfo.Instance.Highlight(this._iiComponent, false);
		}
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		this.Highlight();
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		this.Dehighlight();
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		this.Highlight();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		this.Dehighlight();
	}

	public void UnEquip()
	{
		DeEquip.<UnEquip>c__AnonStorey0 <UnEquip>c__AnonStorey = new DeEquip.<UnEquip>c__AnonStorey0();
		<UnEquip>c__AnonStorey.$this = this;
		if (this._iiComponent.InventoryItem == null)
		{
			return;
		}
		<UnEquip>c__AnonStorey.storages = base.Storages;
		ActiveStorage activeStorage = InitStorages.Instance.ActiveStorage;
		int i = 0;
		while (i < <UnEquip>c__AnonStorey.storages.Length)
		{
			if (<UnEquip>c__AnonStorey.storages[i].storage == activeStorage.storage)
			{
				this.DranNDropStartChangeActiveStorage();
				if (<UnEquip>c__AnonStorey.storages[i].DragNDropTypeInst == null || !Array.Exists<int>(<UnEquip>c__AnonStorey.storages[i].typeId, (int x) => x == <UnEquip>c__AnonStorey.storages[i].DragNDropTypeInst.CurrentActiveTypeId))
				{
					break;
				}
				this.DranNDropEnd(<UnEquip>c__AnonStorey.storages[i]);
				return;
			}
			else
			{
				i++;
			}
		}
		for (int j = 0; j < <UnEquip>c__AnonStorey.storages.Length; j++)
		{
			if (<UnEquip>c__AnonStorey.storages[j].storage != activeStorage.storage && <UnEquip>c__AnonStorey.storages[j].DragNDropTypeInst != null && Array.Exists<int>(<UnEquip>c__AnonStorey.storages[j].typeId, (int x) => x == <UnEquip>c__AnonStorey.$this._dragMeComponent.typeId))
			{
				GameFactory.Message.ShowMessage(ScriptLocalization.Get("MovedToHomeStorageMessage"), base.transform.root.gameObject, 4f, true);
				this.Move(<UnEquip>c__AnonStorey.storages[j]);
				return;
			}
		}
	}

	private EventTriggerCustom trigger;
}
