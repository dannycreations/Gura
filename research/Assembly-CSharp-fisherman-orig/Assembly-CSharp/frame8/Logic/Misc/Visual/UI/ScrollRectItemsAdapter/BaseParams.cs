using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	[Serializable]
	public class BaseParams
	{
		public BaseParams()
		{
		}

		public BaseParams(ScrollRect scrollRect)
			: this(scrollRect, scrollRect.transform as RectTransform, scrollRect.content)
		{
		}

		public BaseParams(ScrollRect scrollRect, RectTransform viewport, RectTransform content)
		{
			this.scrollRect = scrollRect;
			this.viewport = ((!(viewport == null)) ? viewport : (scrollRect.transform as RectTransform));
			this.content = ((!(content == null)) ? content : scrollRect.content);
		}

		public float DefaultItemSize
		{
			get
			{
				return this._DefaultItemSize;
			}
		}

		public RectTransform ScrollViewRT
		{
			get
			{
				if (!this._ScrollViewRT)
				{
					this._ScrollViewRT = this.scrollRect.transform as RectTransform;
				}
				return this._ScrollViewRT;
			}
		}

		public Snapper8 Snapper { get; private set; }

		public virtual void InitIfNeeded(ISRIA sria)
		{
			if (!this.scrollRect)
			{
				this.scrollRect = sria.AsMonoBehaviour.GetComponent<ScrollRect>();
			}
			if (!this.scrollRect)
			{
				throw new UnityException("Can't find ScrollRect component!");
			}
			if (!this.viewport)
			{
				this.viewport = this.scrollRect.transform as RectTransform;
			}
			if (!this.content)
			{
				this.content = this.scrollRect.content;
			}
			if (!this.Snapper)
			{
				this.Snapper = this.scrollRect.GetComponent<Snapper8>();
			}
		}

		[Obsolete("This method was moved to the ScrollRectItemsAdapter8 class", true)]
		public float GetAbstractNormalizedScrollPosition()
		{
			return 0f;
		}

		public void UpdateContentPivotFromGravityType()
		{
			if (this.contentGravity != BaseParams.ContentGravity.NONE)
			{
				int num = ((!this.scrollRect.horizontal) ? 1 : 0);
				Vector2 pivot = this.content.pivot;
				pivot[1 - num] = 0.5f;
				int num2 = (int)this.contentGravity;
				float num3;
				if (num2 < 3)
				{
					num3 = 1f / (float)num2;
				}
				else
				{
					num3 = 0f;
				}
				pivot[num] = num3;
				if (num == 0)
				{
					pivot[num] = 1f - pivot[num];
				}
				this.content.pivot = pivot;
			}
		}

		[Tooltip("How much objects besides the visible ones to keep in memory at max. \nBy default, no more than the heuristically found \"ideal\" number of items will be held in memory.\nSet to a positive integer to limit it - Not recommended, unless you're OK with more GC calls (i.e. occasional FPS hiccups) in favor of using less RAM")]
		[Header("Optimizing process")]
		public int recycleBinCapacity = -1;

		[Tooltip("See BaseParams.UpdateMode enum for full description. The default is ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE and if the framerate is acceptable, it should be leaved this way")]
		public BaseParams.UpdateMode updateMode;

		[Tooltip("If true: When the last item is reached, the first one appears after it, basically allowing you to scroll infinitely.\n Initially intended for things like spinners, but it can be used for anything alike.\n It may interfere with other functionalities in some very obscure/complex contexts/setups, so be sure to test the hell out of it.\n Also please note that sometimes during dragging the content, the actual looping changes the Unity's internal PointerEventData for the current click/touch pointer id, so if you're also externally tracking the current click/touch, in this case only 'PointerEventData.pointerCurrentRaycast' and 'PointerEventData.position'(current position) are preserved, the other ones are reset to defaults to assure a smooth loop transition. Sorry for the long decription. Here's an ASCII potato: (@)")]
		public bool loopItems;

		[Tooltip("If null, the scrollRect is considered to be the viewport")]
		public RectTransform viewport;

		[Tooltip("This is used instead of the old way of putting a disabled LayoutGroup component on the content")]
		public RectOffset contentPadding = new RectOffset();

		public BaseParams.ContentGravity contentGravity = BaseParams.ContentGravity.START;

		[Tooltip("This is used instead of the old way of putting a disabled LayoutGroup component on the content")]
		public float contentSpacing;

		[Range(0f, 1f)]
		public float galleryEffectAmount;

		[Range(0f, 1f)]
		public float galleryEffectViewportPivot = 0.5f;

		[Tooltip("The size of all items for which the size is not specified in CollectItemSizes()")]
		[SerializeField]
		protected float _DefaultItemSize = 60f;

		[Obsolete("Not used anymore", true)]
		[NonSerialized]
		public int minNumberOfObjectsToKeepInMemory = -1;

		[NonSerialized]
		public ScrollRect scrollRect;

		[NonSerialized]
		public RectTransform content;

		private RectTransform _ScrollViewRT;

		public enum UpdateMode
		{
			ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE,
			ON_SCROLL,
			MONOBEHAVIOUR_UPDATE
		}

		public enum ContentGravity
		{
			NONE,
			START,
			CENTER,
			END
		}
	}
}
