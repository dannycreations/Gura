using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine.EventSystems;

public class DropMeChum : DropMe
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<InventoryItem, DropMeChum.States> OnAction = delegate(InventoryItem item, DropMeChum.States states)
	{
	};

	private new void Awake()
	{
		this.Types = this.typeId.ToList<int>();
	}

	public override void OnDrop(PointerEventData data)
	{
		if (this.IsAction)
		{
			this.OnAction(base.GetDropObject(data), DropMeChum.States.Droped);
		}
	}

	public override void OnPointerEnter(PointerEventData data)
	{
		if (this.IsAction)
		{
			this.OnAction(base.GetDropObject(data), DropMeChum.States.PointerEntered);
		}
	}

	public override void OnPointerExit(PointerEventData data)
	{
		if (this.IsAction)
		{
			this.OnAction(base.GetDropObject(data), DropMeChum.States.PointerExited);
		}
	}

	public bool IsAction
	{
		get
		{
			return this.DragNDropTypeInst != null && this.Types.Contains(this.DragNDropTypeInst.CurrentActiveTypeId);
		}
	}

	protected List<int> Types;

	public enum States : byte
	{
		Droped,
		PointerEntered,
		PointerExited
	}
}
