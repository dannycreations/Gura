using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerProfileItemSelectable : MonoBehaviour
{
	private void Awake()
	{
		this._iiComponent = base.transform.parent.GetComponentInChildren<InventoryItemComponent>();
		this.trigger = base.transform.parent.GetComponentInChildren<EventTriggerCustom>();
		if (this.trigger != null)
		{
			this.trigger.OnSelected.AddListener(new UnityAction<BaseEventData>(this.OnSelect));
			this.trigger.OnDeselected.AddListener(new UnityAction<BaseEventData>(this.OnDeselect));
			this.trigger.PointerEnter.AddListener(new UnityAction<BaseEventData>(this.OnSelect));
			this.trigger.PointerExit.AddListener(new UnityAction<BaseEventData>(this.OnDeselect));
		}
	}

	private void OnDestroy()
	{
		if (this.trigger != null)
		{
			this.trigger.OnSelected.RemoveListener(new UnityAction<BaseEventData>(this.OnSelect));
			this.trigger.OnDeselected.RemoveListener(new UnityAction<BaseEventData>(this.OnDeselect));
			this.trigger.PointerEnter.RemoveListener(new UnityAction<BaseEventData>(this.OnSelect));
			this.trigger.PointerExit.RemoveListener(new UnityAction<BaseEventData>(this.OnDeselect));
		}
	}

	private void Highlight()
	{
		if (this._iiComponent != null && this.DollInfo != null)
		{
			this.DollInfo.Highlight(this._iiComponent, true);
		}
	}

	private void Dehighlight()
	{
		if (this._iiComponent != null && this.DollInfo != null)
		{
			this.DollInfo.Highlight(this._iiComponent, false);
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

	[SerializeField]
	private ShowDetailedDollInfo DollInfo;

	protected InventoryItemComponent _iiComponent;

	private EventTriggerCustom trigger;
}
