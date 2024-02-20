using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public abstract class AbstractViewsHolder
	{
		public virtual int ItemIndex { get; set; }

		public virtual void Init(RectTransform rootPrefab, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
		{
			this.Init(rootPrefab.gameObject, itemIndex, activateRootGameObject, callCollectViews);
		}

		public virtual void Init(GameObject rootPrefabGO, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
		{
			this.root = Object.Instantiate<GameObject>(rootPrefabGO).transform as RectTransform;
			if (activateRootGameObject)
			{
				this.root.gameObject.SetActive(true);
			}
			this.ItemIndex = itemIndex;
			if (callCollectViews)
			{
				this.CollectViews();
			}
		}

		public virtual void CollectViews()
		{
		}

		public virtual void MarkForRebuild()
		{
			if (this.root)
			{
				LayoutRebuilder.MarkLayoutForRebuild(this.root);
			}
		}

		public RectTransform root;
	}
}
