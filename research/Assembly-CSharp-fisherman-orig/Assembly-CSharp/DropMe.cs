using System;
using Assets.Scripts.UI._2D.Common;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public virtual InventoryItem DragNDropContent
	{
		get
		{
			return this._dragNDropContent;
		}
		set
		{
			this._dragNDropContent = value;
		}
	}

	protected virtual void Awake()
	{
		if (this.containerImage != null)
		{
			this.normalColor = this.containerImage.color;
		}
		this.WaitOp.OnStopWaiting += this._wOp_OnStopWaiting;
	}

	protected virtual void OnEnable()
	{
		if (this.DragNDropTypeInst == null && base.GetComponent<DragMe>() != null)
		{
			this.DragNDropTypeInst = base.GetComponent<DragMe>().DranNDropType;
		}
	}

	protected virtual void OnDestroy()
	{
		this.WaitOp.OnStopWaiting -= this._wOp_OnStopWaiting;
	}

	public virtual void OnDrop(PointerEventData data)
	{
		this.containerImage.color = this.normalColor;
		if (this.receivingImage == null || this.DragNDropTypeInst == null || !Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId))
		{
			return;
		}
		Sprite dropSprite = this.GetDropSprite(data);
		if (dropSprite != null)
		{
			this.receivingImage.overrideSprite = dropSprite;
		}
	}

	protected InventoryItem GetDropObject(PointerEventData data)
	{
		GameObject pointerDrag = data.pointerDrag;
		if (pointerDrag == null)
		{
			return null;
		}
		DragMe component = pointerDrag.GetComponent<DragMe>();
		if (component == null)
		{
			return null;
		}
		return component.DragNDropContent.InventoryItem;
	}

	public virtual void OnPointerEnter(PointerEventData data)
	{
		if (this.containerImage == null)
		{
			return;
		}
		Sprite dropSprite = this.GetDropSprite(data);
		if (dropSprite != null)
		{
			this.containerImage.color = this.highlightColor;
		}
	}

	public virtual void OnPointerExit(PointerEventData data)
	{
		if (this.containerImage == null)
		{
			return;
		}
		this.containerImage.color = this.normalColor;
	}

	public virtual bool CanEquipNow(InventoryItem itemToEquip)
	{
		return true;
	}

	protected Sprite GetDropSprite(PointerEventData data)
	{
		GameObject pointerDrag = data.pointerDrag;
		if (pointerDrag == null)
		{
			return null;
		}
		Image component = pointerDrag.GetComponent<Image>();
		if (component == null)
		{
			return null;
		}
		return component.sprite;
	}

	private void Update()
	{
		this.Highlight();
		this.UpdateWaiting();
	}

	protected virtual void Highlight()
	{
		if (this.containerImage != null)
		{
			if (this.DragNDropTypeInst.IsActive && Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId))
			{
				this.containerImage.color = this.highlightColor;
			}
			else
			{
				this.containerImage.color = this.normalColor;
			}
		}
	}

	protected virtual void UpdateWaiting()
	{
		if (this._isInventoryProcessing)
		{
			this.WaitOp.Update();
		}
	}

	protected virtual void SubordinateItem(InventoryItem parent, InventoryItem newParent)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.SubordinateItem(parent, newParent);
	}

	protected virtual void ReplaceItem(InventoryItem item, InventoryItem replacementItem)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.ReplaceItem(item, replacementItem);
	}

	protected virtual void SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, int count)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.SplitItemAndReplace(item, replacementItem, count);
	}

	protected virtual void SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, float amount)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.SplitItemAndReplace(item, replacementItem, amount);
	}

	protected virtual void SplitItem(InventoryItem item, InventoryItem parent, int count, StoragePlaces storage)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.SplitItem(item, parent, count, storage);
	}

	protected virtual void SplitItem(InventoryItem item, InventoryItem parent, float amount, StoragePlaces storage)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.SplitItem(item, parent, amount, storage);
	}

	protected virtual void MoveItemOrCombine(InventoryItem item, InventoryItem parent, StoragePlaces storage, bool moveRelatedItems = true)
	{
		this.Subscribe();
		PhotonConnectionFactory.Instance.MoveItemOrCombine(item, parent, storage, moveRelatedItems);
	}

	protected virtual void OnInventoryMoved()
	{
		this.Unsubscribe();
		this.StopWaiting();
	}

	protected virtual void OnInventoryMoveFailure()
	{
		this.Unsubscribe();
		this.StopWaiting();
		GameFactory.Message.ShowLowerMessage(ScriptLocalization.Get("InventoryOperationFailed"), null, 2.5f, false);
	}

	protected virtual void Subscribe()
	{
		this._isInventoryProcessing = true;
		PhotonConnectionFactory.Instance.OnInventoryMoved += this.OnInventoryMoved;
		PhotonConnectionFactory.Instance.OnInventoryMoveFailure += this.OnInventoryMoveFailure;
	}

	protected virtual void Unsubscribe()
	{
		PhotonConnectionFactory.Instance.OnInventoryMoveFailure -= this.OnInventoryMoveFailure;
		PhotonConnectionFactory.Instance.OnInventoryMoved -= this.OnInventoryMoved;
	}

	protected virtual void StopWaiting()
	{
		this.WaitOp.StopWaiting(true);
	}

	private void _wOp_OnStopWaiting()
	{
		this._isInventoryProcessing = false;
	}

	public Image containerImage;

	public Image receivingImage;

	protected Color normalColor;

	public Color highlightColor = Color.yellow;

	public int[] typeId;

	public DragNDropType DragNDropTypeInst;

	protected bool _isInventoryProcessing;

	protected WaitingOperation WaitOp = new WaitingOperation();

	protected InventoryItem _dragNDropContent;
}
