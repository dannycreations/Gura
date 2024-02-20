using System;
using System.Collections;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;
using UnityEngine.Events;

namespace frame8.ScrollRectItemsAdapter.HierarchyExample
{
	public class HierarchyExample : SRIA<MyParams, PageViewsHolder>
	{
		protected override void Start()
		{
			base.Start();
			DrawerCommandPanel.Instance.Init(this, true, false, false, false, false);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.simulateLowEndDeviceSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.galleryEffectSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.OnItemCountChangeRequested;
			ButtonsPanel buttonsPanel = DrawerCommandPanel.Instance.AddButtonsPanel("Collapse All", "ExpandAll", string.Empty, string.Empty);
			buttonsPanel.button1.onClick.AddListener(new UnityAction(this.OnCollapseAll));
			buttonsPanel.button2.onClick.AddListener(new UnityAction(this.OnExpandAll));
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		protected override PageViewsHolder CreateViewsHolder(int itemIndex)
		{
			PageViewsHolder instance = new PageViewsHolder();
			instance.Init(this._Params.itemPrefab, itemIndex, true, true);
			instance.foldoutButton.onClick.AddListener(delegate
			{
				this.OnDirectoryFoldOutClicked(instance);
			});
			return instance;
		}

		protected override void UpdateViewsHolder(PageViewsHolder newOrRecycled)
		{
			FSEntryNodeModel fsentryNodeModel = this._Params.flattenedVisibleHierarchy[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateViews(fsentryNodeModel);
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			if (this._BusyWithAnimation)
			{
				return;
			}
			int num = 0;
			this._Params.hierarchyRootNode = this.CreateRandomNodeModel(ref num, 0, true, 7);
			this._Params.flattenedVisibleHierarchy = new List<FSEntryNodeModel>(this._Params.hierarchyRootNode.children);
			this.ResetItems(this._Params.flattenedVisibleHierarchy.Count, false, false);
		}

		private void OnCollapseAll()
		{
			if (this._BusyWithAnimation)
			{
				return;
			}
			int i = 0;
			while (i < this._Params.flattenedVisibleHierarchy.Count)
			{
				FSEntryNodeModel fsentryNodeModel = this._Params.flattenedVisibleHierarchy[i];
				if (fsentryNodeModel.depth > 1)
				{
					fsentryNodeModel.expanded = false;
					this._Params.flattenedVisibleHierarchy.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			this.ResetItems(this._Params.flattenedVisibleHierarchy.Count, false, false);
		}

		private void OnExpandAll()
		{
			if (this._BusyWithAnimation)
			{
				return;
			}
			this._Params.flattenedVisibleHierarchy = this._Params.hierarchyRootNode.GetFlattenedHierarchyAndExpandAll();
			this.ResetItems(this._Params.flattenedVisibleHierarchy.Count, false, false);
		}

		private void OnDirectoryFoldOutClicked(PageViewsHolder vh)
		{
			if (this._BusyWithAnimation)
			{
				return;
			}
			FSEntryNodeModel fsentryNodeModel = this._Params.flattenedVisibleHierarchy[vh.ItemIndex];
			int num = vh.ItemIndex + 1;
			bool expanded = fsentryNodeModel.expanded;
			fsentryNodeModel.expanded = !expanded;
			if (expanded)
			{
				int i = vh.ItemIndex + 1;
				int count = this._Params.flattenedVisibleHierarchy.Count;
				while (i < count)
				{
					FSEntryNodeModel fsentryNodeModel2 = this._Params.flattenedVisibleHierarchy[i];
					if (fsentryNodeModel2.depth <= fsentryNodeModel.depth)
					{
						break;
					}
					fsentryNodeModel2.expanded = false;
					i++;
				}
				int num2 = i - num;
				if (num2 > 0)
				{
					if (this._Params.animatedFoldOut)
					{
						this.GradualRemove(num, num2);
					}
					else
					{
						this._Params.flattenedVisibleHierarchy.RemoveRange(num, num2);
						this.RemoveItems(num, num2, false, false);
					}
				}
			}
			else if (fsentryNodeModel.children.Length > 0)
			{
				if (this._Params.animatedFoldOut)
				{
					this.GradualAdd(num, fsentryNodeModel.children);
				}
				else
				{
					this._Params.flattenedVisibleHierarchy.InsertRange(num, fsentryNodeModel.children);
					this.InsertItems(num, fsentryNodeModel.children.Length, false, false);
				}
			}
		}

		private void GradualAdd(int index, FSEntryNodeModel[] children)
		{
			base.StartCoroutine(this.GradualAddOrRemove(index, children.Length, children));
		}

		private void GradualRemove(int index, int countToRemove)
		{
			base.StartCoroutine(this.GradualAddOrRemove(index, countToRemove, null));
		}

		private IEnumerator GradualAddOrRemove(int index, int count, FSEntryNodeModel[] childrenIfAdd)
		{
			this._BusyWithAnimation = true;
			int curIndexInChildren = 0;
			int remainingLen = count;
			int divider = Mathf.Min(7, count);
			int maxChunkSize = count / divider;
			float toWait = 0.01f;
			WaitForSeconds toWaitYieldInstr = new WaitForSeconds(toWait);
			if (childrenIfAdd == null)
			{
				index = index + count - 1;
				while (remainingLen > 0)
				{
					int curChunkSize = Math.Min(remainingLen, maxChunkSize);
					int curStartIndex = index - curChunkSize + 1;
					int i = index;
					while (i >= curStartIndex)
					{
						this._Params.flattenedVisibleHierarchy.RemoveAt(i);
						i--;
						index--;
					}
					this.RemoveItems(curStartIndex, curChunkSize, false, false);
					remainingLen -= curChunkSize;
					yield return toWaitYieldInstr;
				}
			}
			else
			{
				while (remainingLen > 0)
				{
					int curChunkSize = Math.Min(remainingLen, maxChunkSize);
					int curStartIndex2 = index;
					int j = 0;
					while (j < curChunkSize)
					{
						this._Params.flattenedVisibleHierarchy.Insert(index, childrenIfAdd[curIndexInChildren]);
						j++;
						index++;
						curIndexInChildren++;
					}
					this.InsertItems(curStartIndex2, curChunkSize, false, false);
					remainingLen -= curChunkSize;
					yield return toWaitYieldInstr;
				}
			}
			this._BusyWithAnimation = false;
			yield break;
		}

		private FSEntryNodeModel CreateRandomNodeModel(ref int itemIndex, int depth, bool forceDirirectory, int numChildren)
		{
			if (forceDirirectory || (depth + 1 < this._Params.maxHierarchyDepth && Random.Range(0, 2) == 0))
			{
				FSEntryNodeModel fsentryNodeModel = this.CreateNewModel(ref itemIndex, depth, true);
				fsentryNodeModel.children = new FSEntryNodeModel[numChildren];
				bool flag = depth == 1;
				for (int i = 0; i < numChildren; i++)
				{
					fsentryNodeModel.children[i] = this.CreateRandomNodeModel(ref itemIndex, depth + 1, flag, Random.Range(1, 7));
				}
				return fsentryNodeModel;
			}
			return this.CreateNewModel(ref itemIndex, depth, false);
		}

		private FSEntryNodeModel CreateNewModel(ref int itemIdex, int depth, bool isDirectory)
		{
			return new FSEntryNodeModel
			{
				title = ((!isDirectory) ? "File " : "Directory ") + itemIdex++,
				depth = depth
			};
		}

		private bool _BusyWithAnimation;
	}
}
