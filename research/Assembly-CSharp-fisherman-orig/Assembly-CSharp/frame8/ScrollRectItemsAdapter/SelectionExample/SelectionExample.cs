using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.SelectionExample
{
	public class SelectionExample : GridAdapter<MyGridParams, MyCellViewsHolder>, LongClickableItem.IItemLongClickListener, ExpandCollapseOnClick.ISizeChangesHandler, ILazyListSimpleDataManager<BasicModel>
	{
		public LazyList<BasicModel> Data { get; private set; }

		private bool SelectionMode
		{
			get
			{
				return this._SelectionMode;
			}
			set
			{
				if (this._SelectionMode != value)
				{
					this.SetSelectionMode(value);
					this.RefreshSelectionStateForVisibleCells();
					this.UpdateSelectionActionButtons();
				}
			}
		}

		protected override void Awake()
		{
			this.Data = new LazyList<BasicModel>(new Func<int, BasicModel>(this.CreateNewModel), 0);
			base.Awake();
		}

		protected override void Start()
		{
			DrawerCommandPanel.Instance.Init(this, true, false, true, false, true);
			base.Start();
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.OnItemCountChangeRequested;
			DrawerCommandPanel.Instance.AddItemRequested += this.OnAddItemRequested;
			DrawerCommandPanel.Instance.RemoveItemRequested += this.OnRemoveItemRequested;
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
			this._Params.deleteButton.onClick.AddListener(delegate
			{
				if (!this.PlayPreDeleteAnimation())
				{
					this.DeleteSelectedItems();
				}
			});
			this._Params.cancelButton.onClick.AddListener(delegate
			{
				this.SelectionMode = false;
			});
		}

		protected override void Update()
		{
			base.Update();
			if (Input.GetKeyUp(27))
			{
				this.SelectionMode = false;
			}
		}

		public override void ChangeItemsCount(ItemCountChangeMode changeMode, int cellsCount, int indexIfAppendingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (this._SelectionMode)
			{
				this.SetSelectionMode(false);
			}
			this.UpdateSelectionActionButtons();
			base.ChangeItemsCount(changeMode, cellsCount, indexIfAppendingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
		}

		public override void Refresh(bool contentPanelEndEdgeStationary, bool keepVelocity = false)
		{
			this._CellsCount = this.Data.Count;
			base.Refresh(DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, keepVelocity);
		}

		protected override CellGroupViewsHolder<MyCellViewsHolder> CreateViewsHolder(int itemIndex)
		{
			CellGroupViewsHolder<MyCellViewsHolder> cellGroupViewsHolder = base.CreateViewsHolder(itemIndex);
			for (int i = 0; i < cellGroupViewsHolder.ContainingCellViewsHolders.Length; i++)
			{
				MyCellViewsHolder cellVH = cellGroupViewsHolder.ContainingCellViewsHolders[i];
				cellVH.toggle.onValueChanged.AddListener(delegate(bool _)
				{
					this.OnCellToggled(cellVH);
				});
				cellVH.longClickableComponent.longClickListener = this;
			}
			return cellGroupViewsHolder;
		}

		protected override void UpdateCellViewsHolder(MyCellViewsHolder viewsHolder)
		{
			BasicModel basicModel = this.Data[viewsHolder.ItemIndex];
			viewsHolder.UpdateViews(basicModel);
			this.UpdateSelectionState(viewsHolder, basicModel);
		}

		protected override void OnBeforeRecycleOrDisableCellViewsHolder(MyCellViewsHolder viewsHolder, int newItemIndex)
		{
			viewsHolder.views.localScale = Vector3.one;
			viewsHolder.expandCollapseComponent.expanded = true;
		}

		private void UpdateSelectionState(MyCellViewsHolder viewsHolder, BasicModel model)
		{
			viewsHolder.longClickableComponent.gameObject.SetActive(!this._SelectionMode);
			viewsHolder.toggle.gameObject.SetActive(this._SelectionMode);
			viewsHolder.toggle.isOn = model.isSelected;
		}

		private void SetSelectionMode(bool active)
		{
			this._SelectionMode = active;
			DrawerCommandPanel.Instance.addRemoveOnePanel.Interactable = !this._SelectionMode;
			for (int i = 0; i < this._CellsCount; i++)
			{
				this.Data[i].isSelected = false;
			}
			this.waitingForItemsToBeDeleted = false;
		}

		private void UpdateSelectionActionButtons()
		{
			if (!this._SelectionMode)
			{
				this._Params.deleteButton.interactable = false;
			}
			this._Params.cancelButton.interactable = this._SelectionMode;
		}

		private bool PlayPreDeleteAnimation()
		{
			int numVisibleCells = this.GetNumVisibleCells();
			for (int i = 0; i < numVisibleCells; i++)
			{
				MyCellViewsHolder cellViewsHolder = this.GetCellViewsHolder(i);
				BasicModel basicModel = this.Data[cellViewsHolder.ItemIndex];
				if (basicModel.isSelected)
				{
					if (cellViewsHolder != null)
					{
						this.waitingForItemsToBeDeleted = true;
						cellViewsHolder.expandCollapseComponent.sizeChangesHandler = this;
						cellViewsHolder.expandCollapseComponent.OnClicked();
					}
				}
			}
			return this.waitingForItemsToBeDeleted;
		}

		private void RefreshSelectionStateForVisibleCells()
		{
			int numVisibleCells = this.GetNumVisibleCells();
			for (int i = 0; i < numVisibleCells; i++)
			{
				MyCellViewsHolder cellViewsHolder = this.GetCellViewsHolder(i);
				this.UpdateSelectionState(cellViewsHolder, this.Data[cellViewsHolder.ItemIndex]);
			}
		}

		private void OnCellToggled(MyCellViewsHolder cellVH)
		{
			BasicModel basicModel = this.Data[cellVH.ItemIndex];
			basicModel.isSelected = cellVH.toggle.isOn;
			cellVH.views.localScale = ((!basicModel.isSelected) ? Vector3.one : this.SELECTED_SCALE);
			if (cellVH.toggle.isOn)
			{
				this._Params.deleteButton.interactable = true;
			}
			else
			{
				foreach (BasicModel basicModel2 in this.Data.AsEnumerableForExistingItems)
				{
					if (basicModel2.isSelected)
					{
						return;
					}
				}
				this._Params.deleteButton.interactable = false;
			}
		}

		public void OnItemLongClicked(LongClickableItem longClickedItem)
		{
			this.SetSelectionMode(true);
			this.RefreshSelectionStateForVisibleCells();
			this.UpdateSelectionActionButtons();
			if (this._Params.autoSelectFirstOnSelectionMode)
			{
				int numVisibleCells = base.GetNumVisibleCells();
				for (int i = 0; i < numVisibleCells; i++)
				{
					MyCellViewsHolder cellViewsHolder = base.GetCellViewsHolder(i);
					if (cellViewsHolder.longClickableComponent == longClickedItem)
					{
						BasicModel basicModel = this.Data[cellViewsHolder.ItemIndex];
						basicModel.isSelected = true;
						this.UpdateSelectionState(cellViewsHolder, basicModel);
						break;
					}
				}
			}
		}

		public bool HandleSizeChangeRequest(RectTransform rt, float newSize)
		{
			rt.localScale = this.SELECTED_SCALE * newSize;
			return true;
		}

		public void OnExpandedStateChanged(RectTransform rt, bool expanded)
		{
			if (expanded)
			{
				return;
			}
			if (this.waitingForItemsToBeDeleted)
			{
				this.waitingForItemsToBeDeleted = false;
				int numVisibleCells = this.GetNumVisibleCells();
				for (int i = 0; i < numVisibleCells; i++)
				{
					MyCellViewsHolder cellViewsHolder = this.GetCellViewsHolder(i);
					if (cellViewsHolder != null)
					{
						cellViewsHolder.expandCollapseComponent.sizeChangesHandler = null;
					}
				}
				this.DeleteSelectedItems();
			}
		}

		private void OnAddItemRequested(bool atEnd)
		{
			int num = ((!atEnd) ? 0 : base.CellsCount);
			this.LazyInsertItems(num, 1);
		}

		private void OnRemoveItemRequested(bool fromEnd)
		{
			if (base.CellsCount == 0)
			{
				return;
			}
			this.LazyRemoveItemAt((!fromEnd) ? 0 : (base.CellsCount - 1));
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			this._CurrentFreeID = 0;
			this.LazySetNewItems(newCount);
		}

		private void DeleteSelectedItems()
		{
			List<BasicModel> list = new List<BasicModel>();
			foreach (BasicModel basicModel in this.Data.AsEnumerableForExistingItems)
			{
				if (basicModel.isSelected)
				{
					list.Add(basicModel);
				}
			}
			if (list.Count > 0)
			{
				this.LazyRemoveItems(list.ToArray());
				if (this._Params.keepSelectionModeAfterDeletion)
				{
					this.SelectionMode = true;
				}
				foreach (BasicModel basicModel2 in list)
				{
					this.HandleItemDeletion(basicModel2);
				}
			}
		}

		private BasicModel CreateNewModel(int index)
		{
			return new BasicModel
			{
				id = this._CurrentFreeID++
			};
		}

		private void HandleItemDeletion(BasicModel model)
		{
			Debug.Log("Deleted with id: " + model.id);
		}

		private const float SELECTED_SCALE_FACTOR = 0.8f;

		private readonly Vector3 SELECTED_SCALE = new Vector3(0.8f, 0.8f, 1f);

		private bool _SelectionMode;

		private bool waitingForItemsToBeDeleted;

		private int _CurrentFreeID;
	}
}
