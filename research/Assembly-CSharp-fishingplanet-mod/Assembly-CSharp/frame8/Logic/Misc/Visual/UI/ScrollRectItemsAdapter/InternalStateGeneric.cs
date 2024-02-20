using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	internal abstract class InternalStateGeneric<TParams, TItemViewsHolder> where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
	{
		protected InternalStateGeneric(TParams sourceParams, ItemsDescriptor itemsDescriptor)
		{
			this._SourceParams = sourceParams;
			this._ItemsDesc = itemsDescriptor;
			LayoutGroup component = sourceParams.content.GetComponent<LayoutGroup>();
			if (component && component.enabled)
			{
				component.enabled = false;
				Debug.Log("LayoutGroup on GameObject " + component.name + " has beed disabled in order to use ScrollRectItemsAdapter8");
			}
			ContentSizeFitter component2 = sourceParams.content.GetComponent<ContentSizeFitter>();
			if (component2 && component2.enabled)
			{
				component2.enabled = false;
				Debug.Log("ContentSizeFitter on GameObject " + component2.name + " has beed disabled in order to use ScrollRectItemsAdapter8");
			}
			LayoutElement component3 = sourceParams.content.GetComponent<LayoutElement>();
			if (component3)
			{
				Object.Destroy(component3);
				Debug.Log("LayoutElement on GameObject " + component2.name + " has beed DESTROYED in order to use ScrollRectItemsAdapter8");
			}
			if (sourceParams.scrollRect.horizontal)
			{
				this.startEdge = 0;
				this.endEdge = 1;
				this.transvStartEdge = 2;
				this._GetRTCurrentSizeFn = (RectTransform root) => root.rect.width;
			}
			else
			{
				this.startEdge = 2;
				this.endEdge = 3;
				this.transvStartEdge = 0;
				this._GetRTCurrentSizeFn = (RectTransform root) => root.rect.height;
			}
			this._SourceParams.UpdateContentPivotFromGravityType();
			this.CacheScrollViewInfo();
		}

		internal double ProximityToLimitNeeded01ToResetPos
		{
			get
			{
				return (this._SourceParams.scrollRect.movementType != 2) ? 1.0 : 0.9999995;
			}
		}

		internal bool HasScrollViewSizeChanged
		{
			get
			{
				return this.scrollViewSize != this._SourceParams.ScrollViewRT.rect.size;
			}
		}

		internal float MaxContentPanelRealSize
		{
			get
			{
				return this.viewportSize * 10f;
			}
		}

		internal double ContentPanelVirtualInsetFromViewportStart
		{
			get
			{
				return this.contentPanelSkippedInsetDueToVirtualization + (double)this._SourceParams.content.GetInsetFromParentEdge(this._SourceParams.viewport, this.startEdge);
			}
		}

		internal double ContentPanelVirtualInsetFromViewportEnd
		{
			get
			{
				return -this.contentPanelVirtualSize + (double)this.viewportSize - this.ContentPanelVirtualInsetFromViewportStart;
			}
		}

		internal double VirtualScrollableArea
		{
			get
			{
				return this.contentPanelVirtualSize - (double)this.viewportSize;
			}
		}

		internal float RealScrollableArea
		{
			get
			{
				return this.contentPanelSize - this.viewportSize;
			}
		}

		internal void CacheScrollViewInfo()
		{
			this.scrollViewSize = this._SourceParams.ScrollViewRT.rect.size;
			RectTransform viewport = this._SourceParams.viewport;
			Rect rect = viewport.rect;
			if (this._SourceParams.scrollRect.horizontal)
			{
				this.viewportSize = rect.width;
				this.paddingContentStart = (float)this._SourceParams.contentPadding.left;
				this.paddingContentEnd = (float)this._SourceParams.contentPadding.right;
				this.transversalPaddingContentStart = (float)this._SourceParams.contentPadding.top;
				this._ItemsDesc.itemsConstantTransversalSize = this._SourceParams.content.rect.height - (this.transversalPaddingContentStart + (float)this._SourceParams.contentPadding.bottom);
			}
			else
			{
				this.viewportSize = rect.height;
				this.paddingContentStart = (float)this._SourceParams.contentPadding.top;
				this.paddingContentEnd = (float)this._SourceParams.contentPadding.bottom;
				this.transversalPaddingContentStart = (float)this._SourceParams.contentPadding.left;
				this._ItemsDesc.itemsConstantTransversalSize = this._SourceParams.content.rect.width - (this.transversalPaddingContentStart + (float)this._SourceParams.contentPadding.right);
			}
			this.spacing = this._SourceParams.contentSpacing;
			if (this._SourceParams.loopItems)
			{
				this.paddingContentStart = (this.paddingContentEnd = this.spacing);
			}
			this.paddingStartPlusEnd = this.paddingContentStart + this.paddingContentEnd;
		}

		internal void OnItemsCountChanged(int itemsPrevCount, bool contentPanelEndEdgeStationary)
		{
			this.OnCumulatedSizesOfAllItemsChanged(contentPanelEndEdgeStationary, true);
			this.computeVisibilityTwinPassScheduled = false;
			this.lastComputeVisibilityHadATwinPass = false;
		}

		internal float ChangeItemSizeAndUpdateContentSizeAccordingly(TItemViewsHolder viewsHolder, int itemIndexInView, float requestedSize, bool itemEndEdgeStationary, bool rebuild = true)
		{
			float num;
			if (viewsHolder == null)
			{
				num = requestedSize;
			}
			else
			{
				if (viewsHolder.root == null)
				{
					throw new UnityException("God bless: shouldn't happen if implemented according to documentation/examples");
				}
				RectTransform.Edge edge;
				float num2;
				if (itemEndEdgeStationary)
				{
					edge = this.endEdge;
					num2 = this.GetItemInferredRealInsetFromParentEnd(itemIndexInView);
				}
				else
				{
					edge = this.startEdge;
					num2 = this.GetItemInferredRealInsetFromParentStart(itemIndexInView);
				}
				viewsHolder.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this._SourceParams.content, edge, num2, requestedSize);
				num = this._GetRTCurrentSizeFn(viewsHolder.root);
			}
			this._ItemsDesc.BeginChangingItemsSizes(itemIndexInView);
			this._ItemsDesc[itemIndexInView] = num;
			this._ItemsDesc.EndChangingItemsSizes();
			this.OnCumulatedSizesOfAllItemsChanged(itemEndEdgeStationary, rebuild);
			return num;
		}

		internal void OnItemsSizesChangedExternally(List<TItemViewsHolder> vhs, float[] sizes, bool itemEndEdgeStationary)
		{
			if (this._ItemsDesc.itemsCount == 0)
			{
				throw new UnityException("Cannot change item sizes externally if the items count is 0!");
			}
			int count = vhs.Count;
			this._ItemsDesc.BeginChangingItemsSizes(vhs[0].itemIndexInView);
			for (int i = 0; i < count; i++)
			{
				TItemViewsHolder titemViewsHolder = vhs[i];
				int itemIndexInView = titemViewsHolder.itemIndexInView;
				this._ItemsDesc[itemIndexInView] = sizes[i];
			}
			this._ItemsDesc.EndChangingItemsSizes();
			this.OnCumulatedSizesOfAllItemsChanged(itemEndEdgeStationary, true);
			if (count > 0)
			{
				this.CorrectPositions(vhs, true);
			}
		}

		internal void CorrectPositions(List<TItemViewsHolder> vhs, bool alsoCorrectTransversalPositioning)
		{
			int count = vhs.Count;
			double num = this.GetItemVirtualInsetFromParentStartUsingItemIndexInView(vhs[0].itemIndexInView);
			for (int i = 0; i < count; i++)
			{
				TItemViewsHolder titemViewsHolder = vhs[i];
				float num2 = this._ItemsDesc[titemViewsHolder.itemIndexInView];
				titemViewsHolder.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this._SourceParams.content, this.startEdge, this.ConvertItemInsetFromParentStart_FromVirtualToReal(num), num2);
				num += (double)(num2 + this.spacing);
				if (alsoCorrectTransversalPositioning)
				{
					titemViewsHolder.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this.transvStartEdge, this.transversalPaddingContentStart, this._ItemsDesc.itemsConstantTransversalSize);
				}
			}
		}

		internal void UpdateLastProcessedCTVirtualInsetFromParentStart()
		{
			this.lastProcessedCTVirtualInsetFromParentStart = this.ContentPanelVirtualInsetFromViewportStart;
		}

		internal double GetVirtualAbstractNormalizedScrollPosition()
		{
			float num = this.viewportSize;
			double num2 = this.contentPanelVirtualSize;
			if ((double)num > num2)
			{
				return (double)(this._SourceParams.content.GetInsetFromParentEdge(this._SourceParams.viewport, this.startEdge) / num);
			}
			double num3 = -num2 + (double)num;
			return 1.0 - this.ContentPanelVirtualInsetFromViewportStart / num3;
		}

		internal bool SetVirtualAbstractNormalizedScrollPosition(double pos)
		{
			if ((double)this.viewportSize > this.contentPanelVirtualSize)
			{
				return false;
			}
			double num = this.contentPanelVirtualSize - (double)this.viewportSize;
			double num2 = (1.0 - pos) * num;
			float num3 = this.contentPanelSize - this.viewportSize;
			float num4;
			if (pos < 1E-06)
			{
				num4 = -num3;
			}
			else if (pos > 0.999999)
			{
				num4 = 0f;
			}
			else
			{
				num4 = -(float)Math.Min(3.4028234663852886E+38, Math.Max(-3.4028234663852886E+38, num2 % (double)num3));
			}
			this._SourceParams.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this.startEdge, num4, this.contentPanelSize);
			this.contentPanelSkippedInsetDueToVirtualization = -num2 - (double)num4;
			return true;
		}

		internal void SetContentVirtualInsetFromViewportStart(double virtualInset)
		{
			double virtualScrollableArea = this.VirtualScrollableArea;
			if (!this._SourceParams.loopItems)
			{
				if (virtualInset > 0.0)
				{
					virtualInset = 0.0;
					Debug.Log("virtualInset>0: " + virtualInset + ". Clamping...");
				}
				else if (-virtualInset > virtualScrollableArea)
				{
					Debug.Log(string.Concat(new object[]
					{
						"-virtualInset(",
						-virtualInset,
						") > virtualScrollableArea(",
						virtualScrollableArea,
						"). Clamping..."
					}));
					virtualInset = -virtualScrollableArea;
				}
			}
			float realScrollableArea = this.RealScrollableArea;
			double num = Math.Abs(virtualInset);
			double num2 = (double)Math.Sign(virtualInset) * (num % (double)realScrollableArea);
			if (num < (double)realScrollableArea)
			{
				this.contentPanelSkippedInsetDueToVirtualization = 0.0;
			}
			else
			{
				this.contentPanelSkippedInsetDueToVirtualization = virtualInset - num2;
			}
			this._SourceParams.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this.startEdge, (float)num2, this.contentPanelSize);
			Canvas.ForceUpdateCanvases();
		}

		internal double GetItemVirtualInsetFromParentStartUsingItemIndexInView(int itemIndexInView)
		{
			double num = 0.0;
			if (itemIndexInView > 0)
			{
				num = this._ItemsDesc.GetItemSizeCumulative(itemIndexInView - 1, true) + (double)itemIndexInView * (double)this.spacing;
			}
			return (double)this.paddingContentStart + num;
		}

		internal double GetItemVirtualInsetFromParentEndUsingItemIndexInView(int itemIndexInView)
		{
			return this.contentPanelVirtualSize - this.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemIndexInView) - (double)this._ItemsDesc[itemIndexInView];
		}

		internal float GetItemInferredRealInsetFromParentStart(int itemIndexInView)
		{
			return this.ConvertItemInsetFromParentStart_FromVirtualToReal(this.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemIndexInView));
		}

		internal float GetItemInferredRealInsetFromParentEnd(int itemIndexInView)
		{
			return this.contentPanelSize - this.GetItemInferredRealInsetFromParentStart(itemIndexInView) - this._ItemsDesc[itemIndexInView];
		}

		internal float ConvertItemInsetFromParentStart_FromVirtualToReal(double virtualInsetFromParrentStart)
		{
			return (float)(virtualInsetFromParrentStart + this.contentPanelSkippedInsetDueToVirtualization);
		}

		private void OnCumulatedSizesOfAllItemsChanged(bool contentPanelEndEdgeStationary, bool rebuild = true)
		{
			this._ItemsDesc.cumulatedSizesOfAllItemsPlusSpacing = this._ItemsDesc.CumulatedSizeOfAllItems + Math.Max(0.0, (double)(this._ItemsDesc.itemsCount - 1)) * (double)this.spacing;
			this.OnCumulatedSizesOfAllItemsPlusSpacingChanged(contentPanelEndEdgeStationary, rebuild);
		}

		private void OnCumulatedSizesOfAllItemsPlusSpacingChanged(bool contentPanelEndEdgeStationary, bool rebuild = true)
		{
			double contentPanelVirtualInsetFromViewportEnd = this.ContentPanelVirtualInsetFromViewportEnd;
			double contentPanelVirtualInsetFromViewportStart = this.ContentPanelVirtualInsetFromViewportStart;
			double num = this.contentPanelVirtualSize;
			this.contentPanelVirtualSize = this._ItemsDesc.cumulatedSizesOfAllItemsPlusSpacing + (double)this.paddingStartPlusEnd;
			bool flag = this.contentPanelVirtualSize < (double)this.MaxContentPanelRealSize;
			float num2;
			if (flag)
			{
				num2 = (float)this.contentPanelVirtualSize;
				this.contentPanelSkippedInsetDueToVirtualization = 0.0;
			}
			else
			{
				num2 = this.MaxContentPanelRealSize;
			}
			float num3 = this.contentPanelSize;
			this.contentPanelSize = num2;
			float num4 = this.contentPanelSize - num3;
			RectTransform.Edge edge = ((!contentPanelEndEdgeStationary) ? this.startEdge : this.endEdge);
			float insetFromParentEdge = this._SourceParams.content.GetInsetFromParentEdge(this._SourceParams.viewport, edge);
			this._SourceParams.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this._SourceParams.viewport, edge, insetFromParentEdge, this.contentPanelSize);
			if (rebuild)
			{
				this._SourceParams.scrollRect.Rebuild(2);
				Canvas.ForceUpdateCanvases();
			}
			if (flag)
			{
				return;
			}
			double num5 = this.contentPanelVirtualSize - num;
			if (num5 != 0.0 || num4 != 0f)
			{
				double num6 = -num5;
				if (contentPanelEndEdgeStationary)
				{
					this.contentPanelSkippedInsetDueToVirtualization -= num5;
					if (num5 < 0.0)
					{
						double num7 = Math.Abs(contentPanelVirtualInsetFromViewportStart);
						if (num7 < num6)
						{
							double num8 = num6 - num7;
							this.contentPanelSkippedInsetDueToVirtualization -= num8;
						}
					}
					else if (this.contentPanelSize > num3)
					{
						this.contentPanelSkippedInsetDueToVirtualization += (double)num4;
					}
				}
				else if (num5 < 0.0)
				{
					double num9 = Math.Abs(contentPanelVirtualInsetFromViewportEnd);
					double num10 = num6;
					if (num4 < 0f)
					{
						num10 -= (double)num4;
					}
					if (num9 < num10)
					{
						double num11 = num10 - num9;
						this.contentPanelSkippedInsetDueToVirtualization += num11;
					}
				}
				else if ((double)num4 < 0.0)
				{
					float num12 = Math.Abs(this.viewportSize - insetFromParentEdge - num3);
					float num13 = -num4;
					if (num12 < num13)
					{
						float num14 = num13 - num12;
						this.contentPanelSkippedInsetDueToVirtualization += (double)num14;
					}
				}
			}
		}

		internal readonly Vector2 constantAnchorPosForAllItems = new Vector2(0f, 1f);

		internal float viewportSize;

		internal float paddingContentStart;

		internal float transversalPaddingContentStart;

		internal float paddingContentEnd;

		internal float paddingStartPlusEnd;

		internal float spacing;

		internal RectTransform.Edge startEdge;

		internal RectTransform.Edge endEdge;

		internal RectTransform.Edge transvStartEdge;

		internal double lastProcessedCTVirtualInsetFromParentStart;

		internal double contentPanelSkippedInsetDueToVirtualization;

		internal Vector2 scrollViewSize;

		internal float contentPanelSize;

		internal double contentPanelVirtualSize;

		internal bool updateRequestPending;

		internal bool computeVisibilityTwinPassScheduled;

		internal bool preferKeepingContentEndEdgeStationaryInNextComputeVisibilityTwinPass;

		internal bool lastComputeVisibilityHadATwinPass;

		private ItemsDescriptor _ItemsDesc;

		private TParams _SourceParams;

		private Func<RectTransform, float> _GetRTCurrentSizeFn;
	}
}
