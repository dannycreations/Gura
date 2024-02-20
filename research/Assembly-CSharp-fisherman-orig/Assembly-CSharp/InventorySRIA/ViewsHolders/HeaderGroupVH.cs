using System;
using frame8.Logic.Misc.Other.Extensions;
using InventorySRIA.Models;
using UnityEngine.UI;

namespace InventorySRIA.ViewsHolders
{
	public class HeaderGroupVH : BaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.forwarding = this.root.GetComponent<DropMeForwarding>();
			this.root.GetComponentAtPath("Text", out this.GroupName);
			this.root.GetComponentAtPath("Icon", out this.GroupIcon);
			this.root.GetComponentAtPath("btnExpand", out this.ExpandPanel);
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(HeaderGroupModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			this._Model = null;
			HeaderGroupModel headerGroupModel = model as HeaderGroupModel;
			this._Model = headerGroupModel;
			this.ExpandPanel.IsExpanded = this._Model.Opened;
			this.ExpandPanel.Refresh();
			this.forwarding.DropMe = this._Model.Storage;
			this.GroupName.text = this._Model.GroupName;
			this.GroupIcon.text = this._Model.GroupIcon;
		}

		private HeaderGroupModel _Model;

		private DropMeForwarding forwarding;

		private Text GroupName;

		private Text GroupIcon;

		public ExpandPanel ExpandPanel;
	}
}
