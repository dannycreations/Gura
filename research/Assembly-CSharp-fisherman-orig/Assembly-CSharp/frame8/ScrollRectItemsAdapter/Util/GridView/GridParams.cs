using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.GridView
{
	[Serializable]
	public class GridParams : BaseParams
	{
		private int NumCellsPerGroupHorizontally
		{
			get
			{
				return (!this.scrollRect.horizontal) ? this.numCellsPerGroup : 1;
			}
		}

		private int NumCellsPerGroupVertically
		{
			get
			{
				return (!this.scrollRect.horizontal) ? 1 : this.numCellsPerGroup;
			}
		}

		public override void InitIfNeeded(ISRIA sria)
		{
			base.InitIfNeeded(sria);
			if (!this.cellPrefab)
			{
				throw new UnityException("SRIA: " + typeof(GridParams) + ": the prefab was not set. Please set it through inspector or in code");
			}
			if (this.numCellsPerGroup < 1)
			{
				throw new UnityException("SRIA: numCellsPerGroup = " + this.numCellsPerGroup);
			}
			this._DefaultItemSize = ((!this.scrollRect.horizontal) ? this.cellPrefab.rect.height : this.cellPrefab.rect.width);
			this.CreateCellGroupPrefab();
		}

		public virtual HorizontalOrVerticalLayoutGroup GetGroupPrefab(int forGroupAtThisIndex)
		{
			if (this._TheOnlyGroupPrefab == null)
			{
				throw new UnityException("GridParams.InitIfNeeded() was not called by SRIA. Did you forget to call base.Start() in <YourAdapter>.Start()?");
			}
			return this._TheOnlyGroupPrefab;
		}

		public virtual float GetGroupWidth()
		{
			return (float)this.groupPadding.left + (this.cellPrefab.rect.width + this.contentSpacing) * (float)this.NumCellsPerGroupHorizontally - this.contentSpacing + (float)this.groupPadding.right;
		}

		public virtual float GetGroupHeight()
		{
			return (float)this.groupPadding.top + (this.cellPrefab.rect.height + this.contentSpacing) * (float)this.NumCellsPerGroupVertically - this.contentSpacing + (float)this.groupPadding.bottom;
		}

		public virtual int GetGroupIndex(int cellIndex)
		{
			return cellIndex / this.numCellsPerGroup;
		}

		public virtual int GetNumberOfRequiredGroups(int numberOfCells)
		{
			return (numberOfCells != 0) ? (this.GetGroupIndex(numberOfCells - 1) + 1) : 0;
		}

		protected void CreateCellGroupPrefab()
		{
			GameObject gameObject = new GameObject(this.scrollRect.name + "_CellGroupPrefab", new Type[] { typeof(RectTransform) });
			if (!(gameObject.transform is RectTransform))
			{
				Debug.LogException(new UnityException("SRIA: Don't call SRIA.Init() outside MonoBehaviour.Start()!"));
			}
			gameObject.SetActive(false);
			gameObject.transform.SetParent(this.scrollRect.transform, false);
			if (this.scrollRect.horizontal)
			{
				this._TheOnlyGroupPrefab = gameObject.AddComponent<VerticalLayoutGroup>();
			}
			else
			{
				this._TheOnlyGroupPrefab = gameObject.AddComponent<HorizontalLayoutGroup>();
			}
			this._TheOnlyGroupPrefab.spacing = this.contentSpacing;
			this._TheOnlyGroupPrefab.childForceExpandWidth = this.cellWidthForceExpandInGroup;
			this._TheOnlyGroupPrefab.childForceExpandHeight = this.cellHeightForceExpandInGroup;
			this._TheOnlyGroupPrefab.childAlignment = this.alignmentOfCellsInGroup;
			this._TheOnlyGroupPrefab.padding = this.groupPadding;
		}

		[Header("Grid")]
		public RectTransform cellPrefab;

		[Tooltip("The max. number of cells in a row group (for vertical ScrollView) or column group (for horizontal ScrollView)")]
		public int numCellsPerGroup;

		public TextAnchor alignmentOfCellsInGroup;

		public RectOffset groupPadding;

		public bool cellWidthForceExpandInGroup;

		public bool cellHeightForceExpandInGroup;

		private HorizontalOrVerticalLayoutGroup _TheOnlyGroupPrefab;
	}
}
