using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.GridView
{
	public abstract class CellViewsHolder : AbstractViewsHolder
	{
		public override void Init(GameObject rootPrefabGO, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
		{
			throw new InvalidOperationException("A cell cannot be initialized this way. Use InitWithExistingRootPrefab(RectTransform) instead");
		}

		public virtual void InitWithExistingRootPrefab(RectTransform root)
		{
			this.root = root;
			this.ItemIndex = -1;
			this.CollectViews();
		}

		public override void CollectViews()
		{
			base.CollectViews();
			this.views = this.GetViews();
			if (this.views == this.root)
			{
				throw new UnityException("CellViewsHolder: views == root not allowed: you should have a child of root that holds all the views, as the root should always be enabled for layouting purposes");
			}
		}

		public override void MarkForRebuild()
		{
			base.MarkForRebuild();
			if (this.views)
			{
				LayoutRebuilder.MarkLayoutForRebuild(this.views);
			}
		}

		protected virtual RectTransform GetViews()
		{
			RectTransform rectTransform = this.root.Find("Views") as RectTransform;
			if (!rectTransform)
			{
				throw new UnityException("SRIA: override " + typeof(CellViewsHolder).Name + ".GetViews() and provide your own path to the child containing the views. For more info, check the Grid example scene");
			}
			return rectTransform;
		}

		public RectTransform views;
	}
}
