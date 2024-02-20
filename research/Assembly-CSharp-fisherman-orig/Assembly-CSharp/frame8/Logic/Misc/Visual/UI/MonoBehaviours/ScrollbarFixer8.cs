using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.MonoBehaviours
{
	[RequireComponent(typeof(Scrollbar))]
	public class ScrollbarFixer8 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IScrollRectProxy, IEventSystemHandler
	{
		public bool IsDragging
		{
			get
			{
				return this._Dragging;
			}
		}

		private IScrollRectProxy ScrollRectProxy
		{
			get
			{
				return (this.externalScrollRectProxy != null) ? this.externalScrollRectProxy : this;
			}
		}

		private void Awake()
		{
			if (this.autoHideTime == 0f)
			{
				this.autoHideTime = 1f;
			}
			this._Scrollbar = base.GetComponent<Scrollbar>();
			this._InitialScale = this._Scrollbar.transform.localScale;
			this._LastValue = this._Scrollbar.value;
			this._TimeOnLastValueChange = Time.time;
			this._HorizontalScrollBar = this._Scrollbar.direction == null || this._Scrollbar.direction == 1;
			if (!this.scrollRect)
			{
				this.scrollRect = base.GetComponentInParent<ScrollRect>();
			}
			if (this.scrollRect)
			{
				this._ScrollRectRT = this.scrollRect.transform as RectTransform;
				if (!this.viewport)
				{
					this.viewport = this._ScrollRectRT;
				}
				if (this._HorizontalScrollBar)
				{
					if (!this.scrollRect.horizontal)
					{
						throw new UnityException("Can't use horizontal scrollbar with non-horizontal scrollRect");
					}
					if (this.scrollRect.horizontalScrollbar)
					{
						Debug.Log("ScrollbarFixer8: setting scrollRect.horizontalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
						this.scrollRect.horizontalScrollbar = null;
					}
					if (this.scrollRect.verticalScrollbar == this._Scrollbar)
					{
						Debug.Log("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
						this.scrollRect.verticalScrollbar = null;
					}
				}
				else
				{
					if (!this.scrollRect.vertical)
					{
						throw new UnityException("Can't use vertical scrollbar with non-vertical scrollRect");
					}
					if (this.scrollRect.verticalScrollbar)
					{
						Debug.Log("ScrollbarFixer8: setting scrollRect.verticalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
						this.scrollRect.verticalScrollbar = null;
					}
					if (this.scrollRect.horizontalScrollbar == this._Scrollbar)
					{
						Debug.Log("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
						this.scrollRect.horizontalScrollbar = null;
					}
				}
			}
			else
			{
				Debug.LogError("No ScrollRect assigned!");
			}
			if (this.autoHide)
			{
				this.UpdateStartingValuesForAutoHideEffect();
			}
			this.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.ScrollRect_OnValueChangedCalled));
			this.externalScrollRectProxy = this.scrollRect.GetComponent(typeof(IScrollRectProxy)) as IScrollRectProxy;
		}

		private void OnEnable()
		{
			this._Dragging = false;
			this._SlowUpdateCoroutine = base.StartCoroutine(this.SlowUpdate());
		}

		private void Update()
		{
			if (!this._FullyInitialized)
			{
				this.InitializeInFirstUpdate();
			}
			if (this.scrollRect)
			{
				if (this._Dragging)
				{
					return;
				}
				float normalizedPosition = this.ScrollRectProxy.GetNormalizedPosition();
				this._Scrollbar.value = normalizedPosition;
				if (this.autoHide)
				{
					if (normalizedPosition == this._LastValue)
					{
						if (!this._Hidden)
						{
							float num = Mathf.Clamp01((Time.time - this._TimeOnLastValueChange) / this.autoHideTime);
							if (num >= 0.4f)
							{
								float num2 = (num - 0.4f) / 0.6f;
								num2 = num2 * num2 * num2;
								if (this.CheckForAudoHideFadeEffectAndInitIfNeeded())
								{
									this._CanvasGroupForFadeEffect.alpha = Mathf.Lerp(this._AlphaOnLastDrag, this.autoHideFadeEffectMinAlpha, num2);
								}
								if (this.autoHideCollapseEffect)
								{
									Vector3 localScale = base.transform.localScale;
									localScale[(!this.scrollRect.vertical) ? 1 : 0] = Mathf.Lerp(this._TransversalScaleOnLastDrag, this.autoHideCollapseEffectMinScale, num2);
									base.transform.localScale = localScale;
								}
							}
							if (num == 1f)
							{
								this._AutoHidden = true;
								this.Hide();
							}
						}
					}
					else
					{
						this._TimeOnLastValueChange = Time.time;
						this._LastValue = normalizedPosition;
						if (this._Hidden && !this._HiddenNotNeeded)
						{
							this.Show();
						}
					}
				}
				else if (!this.hideWhenNotNeeded && this._Hidden)
				{
					this.Show();
				}
			}
		}

		private void OnDisable()
		{
			base.StopCoroutine(this._SlowUpdateCoroutine);
		}

		private void OnDestroy()
		{
			if (this.scrollRect)
			{
				this.scrollRect.onValueChanged.RemoveListener(new UnityAction<Vector2>(this.ScrollRect_OnValueChangedCalled));
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ScrollPositionChanged;

		public void SetNormalizedPosition(float normalizedPosition)
		{
			if (this._HorizontalScrollBar)
			{
				this.scrollRect.horizontalNormalizedPosition = normalizedPosition;
			}
			else
			{
				this.scrollRect.verticalNormalizedPosition = normalizedPosition;
			}
		}

		public float GetNormalizedPosition()
		{
			return (!this._HorizontalScrollBar) ? this.scrollRect.verticalNormalizedPosition : this.scrollRect.horizontalNormalizedPosition;
		}

		public float GetContentSize()
		{
			return (!this._HorizontalScrollBar) ? this.scrollRect.content.rect.height : this.scrollRect.content.rect.width;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this._Dragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			this._Dragging = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			this.OnScrollRectValueChanged(false);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			this.scrollRect.StopMovement();
		}

		private void InitializeInFirstUpdate()
		{
			if (this.externalScrollRectProxy != null)
			{
				this.externalScrollRectProxy.ScrollPositionChanged += this.ExternalScrollRectProxy_OnScrollPositionChanged;
			}
			this._FullyInitialized = true;
		}

		private IEnumerator SlowUpdate()
		{
			WaitForSeconds waitAmount = new WaitForSeconds(this.sizeUpdateInterval);
			for (;;)
			{
				yield return waitAmount;
				if (!base.enabled)
				{
					break;
				}
				if (this._ScrollRectRT && this.scrollRect.content)
				{
					float contentSize = this.ScrollRectProxy.GetContentSize();
					float num;
					if (this._HorizontalScrollBar)
					{
						num = this.viewport.rect.width;
					}
					else
					{
						num = this.viewport.rect.height;
					}
					float num2;
					if (contentSize <= 0f || contentSize == float.NaN || contentSize == 1E-45f || contentSize == float.NegativeInfinity || contentSize == float.PositiveInfinity)
					{
						num2 = 1f;
					}
					else
					{
						num2 = Mathf.Clamp(num / contentSize, this.minSize, 1f);
					}
					float size = this._Scrollbar.size;
					this._Scrollbar.size = num2;
					if (this.hideWhenNotNeeded)
					{
						if (num2 > 0.99f)
						{
							if (!this._Hidden)
							{
								this._HiddenNotNeeded = true;
								this.Hide();
							}
						}
						else if (this._Hidden && !this._AutoHidden)
						{
							this.Show();
						}
					}
					else if (!this.autoHide && this._Hidden)
					{
						this.Show();
					}
					if (!this._TriedToCallOnScrollbarSizeChangedAtLeastOnce || size != num2)
					{
						this._TriedToCallOnScrollbarSizeChangedAtLeastOnce = true;
						if (this.OnScrollbarSizeChanged != null)
						{
							this.OnScrollbarSizeChanged.Invoke();
						}
					}
				}
			}
			yield break;
		}

		private void Hide()
		{
			this._Hidden = true;
			if (!this.autoHide || this._HiddenNotNeeded)
			{
				base.gameObject.transform.localScale = Vector3.zero;
			}
		}

		private void Show()
		{
			base.gameObject.transform.localScale = this._InitialScale;
			this._HiddenNotNeeded = (this._AutoHidden = (this._Hidden = false));
			if (this.CheckForAudoHideFadeEffectAndInitIfNeeded())
			{
				this._CanvasGroupForFadeEffect.alpha = 1f;
			}
			this.UpdateStartingValuesForAutoHideEffect();
		}

		private void UpdateStartingValuesForAutoHideEffect()
		{
			if (this.CheckForAudoHideFadeEffectAndInitIfNeeded())
			{
				this._AlphaOnLastDrag = this._CanvasGroupForFadeEffect.alpha;
			}
			if (this.autoHideCollapseEffect)
			{
				this._TransversalScaleOnLastDrag = base.transform.localScale[(!this.scrollRect.vertical) ? 1 : 0];
			}
		}

		private bool CheckForAudoHideFadeEffectAndInitIfNeeded()
		{
			if (this.autoHideFadeEffect && !this._CanvasGroupForFadeEffect)
			{
				this._CanvasGroupForFadeEffect = base.GetComponent<CanvasGroup>();
				if (!this._CanvasGroupForFadeEffect)
				{
					this._CanvasGroupForFadeEffect = base.gameObject.AddComponent<CanvasGroup>();
				}
			}
			return this.autoHideFadeEffect;
		}

		private void ScrollRect_OnValueChangedCalled(Vector2 _)
		{
			if (this.externalScrollRectProxy == null)
			{
				this.OnScrollRectValueChanged(true);
			}
		}

		private void ExternalScrollRectProxy_OnScrollPositionChanged(float _)
		{
			this.OnScrollRectValueChanged(true);
		}

		private void OnScrollRectValueChanged(bool fromScrollRect)
		{
			if (!fromScrollRect)
			{
				this.scrollRect.StopMovement();
				if (this._FrameCountOnLastPositionUpdate + this.skippedFramesBetweenPositionChanges < Time.frameCount)
				{
					this.ScrollRectProxy.SetNormalizedPosition(this._Scrollbar.value);
					this._FrameCountOnLastPositionUpdate = Time.frameCount;
				}
			}
			this._TimeOnLastValueChange = Time.time;
			if (this.autoHide)
			{
				this.UpdateStartingValuesForAutoHideEffect();
			}
			if (!this._HiddenNotNeeded && this._Scrollbar.size < 1f)
			{
				this.Show();
			}
		}

		public bool hideWhenNotNeeded = true;

		public bool autoHide = true;

		[Tooltip("A CanvasGroup will be added to the Scrollbar, if not already present, and the fade effect will be achieved by changing its alpha property")]
		public bool autoHideFadeEffect = true;

		[Tooltip("The collapsing effect will change the localScale of the Scrollbar, so the pivot's position decides in what direction it'll grow/shrink.\n Note that sometimes a really nice effect is achieved by placing the pivot slightly outside the rect (the minimized scrollbar will move outside while collapsing)")]
		public bool autoHideCollapseEffect = true;

		[Tooltip("Used if autoHide is on. Duration in seconds")]
		public float autoHideTime = 1f;

		public float autoHideFadeEffectMinAlpha = 0.8f;

		public float autoHideCollapseEffectMinScale = 0.2f;

		[Range(0.01f, 1f)]
		public float minSize = 0.1f;

		[Range(0.015f, 2f)]
		public float sizeUpdateInterval = 0.05f;

		[Tooltip("Used to prevent updates to be processed too often, in case this is a concern")]
		public int skippedFramesBetweenPositionChanges;

		[Tooltip("If not assigned, will try yo find one in the parent")]
		public ScrollRect scrollRect;

		[Tooltip("If not assigned, will use the resolved scrollRect")]
		public RectTransform viewport;

		public UnityEvent OnScrollbarSizeChanged;

		public IScrollRectProxy externalScrollRectProxy;

		private const float HIDE_EFFECT_START_DELAY_01 = 0.4f;

		private RectTransform _ScrollRectRT;

		private RectTransform _ViewPortRT;

		private Scrollbar _Scrollbar;

		private CanvasGroup _CanvasGroupForFadeEffect;

		private bool _HorizontalScrollBar;

		private Vector3 _InitialScale = Vector3.one;

		private bool _Hidden;

		private bool _AutoHidden;

		private bool _HiddenNotNeeded;

		private float _LastValue;

		private float _TimeOnLastValueChange;

		private bool _Dragging;

		private Coroutine _SlowUpdateCoroutine;

		private float _TransversalScaleOnLastDrag;

		private float _AlphaOnLastDrag;

		private bool _FullyInitialized;

		private int _FrameCountOnLastPositionUpdate;

		private bool _TriedToCallOnScrollbarSizeChangedAtLeastOnce;
	}
}
