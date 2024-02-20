using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.ViewsHolders
{
	public class ExpandableVH : BaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("SimpleAvatarPanel/TitleText", out this.titleText);
			this.root.GetComponentAtPath("SimpleAvatarPanel/Panel/MaskWithImage/IconRawImage", out this.remoteImageBehaviour);
			this.expandCollapseOnClickBehaviour = this.root.GetComponent<ExpandCollapseOnClick>();
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(ExpandableModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			ExpandableModel expandableModel = model as ExpandableModel;
			this.remoteImageBehaviour.Load(expandableModel.imageURL, true, null, null);
			if (this.expandCollapseOnClickBehaviour)
			{
				this.expandCollapseOnClickBehaviour.expanded = expandableModel.expanded;
				this.expandCollapseOnClickBehaviour.nonExpandedSize = expandableModel.nonExpandedSize;
			}
		}

		public RemoteImageBehaviour remoteImageBehaviour;

		public ExpandCollapseOnClick expandCollapseOnClickBehaviour;
	}
}
