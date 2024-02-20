using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Other.Extensions;
using InventorySRIA.Models;

namespace InventorySRIA.ViewsHolders
{
	public class ExpandableInventoryItemVH : BaseVH
	{
		public InventoryItemComponent IiComponent
		{
			get
			{
				return this.itemComponent;
			}
		}

		public override void CollectViews()
		{
			base.CollectViews();
			this.sdi = this.root.GetComponent<ShowDetailedInfo>();
			this.itemComponent = this.root.GetComponent<InventoryItemComponent>();
			this.drag = this.root.GetComponent<DragMeInventoryItem>();
			this.hid = this.root.GetComponent<HintItemId>();
			this.hid.SetRemoveOnDisable(true);
			this.forwarding = this.root.GetComponent<DropMeForwarding>();
			this.root.GetComponentAtPath("DamageIcon", out this.damageManager);
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(ExpandableInventoryItemModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			ExpandableInventoryItemModel expandableInventoryItemModel = model as ExpandableInventoryItemModel;
			this.itemComponent.Init(expandableInventoryItemModel.item, expandableInventoryItemModel.Places, expandableInventoryItemModel.ActiveRod);
			this.drag.Init(expandableInventoryItemModel.item, expandableInventoryItemModel.TypeObjectOfDragItem);
			this.sdi.Init(expandableInventoryItemModel.item);
			this.sdi.NonExpandedSize = expandableInventoryItemModel.nonExpandedSize;
			this.sdi.ExpandedSize = expandableInventoryItemModel.ExpandedSize;
			this.sdi.Expanded = expandableInventoryItemModel.expanded;
			this.hid.SetItemId(expandableInventoryItemModel.item, new List<string>
			{
				expandableInventoryItemModel.item.ItemType.ToString(),
				expandableInventoryItemModel.item.ItemSubType.ToString()
			});
			this.forwarding.DropMe = expandableInventoryItemModel.Storage;
			this.damageManager.Refresh();
		}

		public ShowDetailedInfo sdi;

		private InventoryItemComponent itemComponent;

		private DamageIconManager damageManager;

		private DragMeInventoryItem drag;

		private DropMeForwarding forwarding;

		private HintItemId hid;
	}
}
