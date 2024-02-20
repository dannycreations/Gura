using System;
using UnityEngine;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public class BaseParamsWithPrefab : BaseParams
	{
		public float ItemPrefabSize
		{
			get
			{
				if (!this.itemPrefab)
				{
					throw new UnityException("SRIA: " + typeof(BaseParamsWithPrefab) + ": the prefab was not set. Please set it through inspector or in code");
				}
				if (this._PrefabSize == -1f)
				{
					this._PrefabSize = ((!this.scrollRect.horizontal) ? this.itemPrefab.rect.height : this.itemPrefab.rect.width);
				}
				return this._PrefabSize;
			}
		}

		public override void InitIfNeeded(ISRIA sria)
		{
			base.InitIfNeeded(sria);
			this._PrefabSize = -1f;
			this._DefaultItemSize = this.ItemPrefabSize;
		}

		public RectTransform itemPrefab;

		private float _PrefabSize = -1f;
	}
}
