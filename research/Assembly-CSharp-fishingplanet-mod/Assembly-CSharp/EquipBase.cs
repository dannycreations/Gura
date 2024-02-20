using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(DragMe))]
public class EquipBase : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	protected DropMeStorage[] Storages
	{
		get
		{
			return Object.FindObjectsOfType<DropMeStorage>();
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.clickCount == 2)
		{
			this.DblClick(eventData);
		}
	}

	protected virtual void Start()
	{
		this._iiComponent = base.GetComponent<InventoryItemComponent>();
		this._dragMeComponent = base.GetComponent<DragMe>();
	}

	protected virtual void OnEnable()
	{
		if (this._dragMeComponent == null)
		{
			this._dragMeComponent = base.GetComponent<DragMe>();
		}
	}

	protected virtual void Move(DropMe dm)
	{
		this.DranNDropStartChangeActiveStorage();
		this.DranNDropEnd(dm);
	}

	protected virtual void DranNDropStartChangeActiveStorage()
	{
		this._dragMeComponent.DragNDropContent = this._iiComponent;
		this._dragMeComponent.DranNDropStartChangeActiveStorage(true, this._dragMeComponent.typeId);
	}

	protected virtual void DranNDropEnd(DropMe dm)
	{
		dm.OnDrop(new PointerEventData(EventSystem.current)
		{
			pointerDrag = base.gameObject
		});
		this._dragMeComponent.DranNDropActive(false, 0);
	}

	protected virtual void DblClick(PointerEventData eventData)
	{
	}

	protected DragMe _dragMeComponent;

	protected InventoryItemComponent _iiComponent;
}
