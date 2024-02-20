using System;
using System.Diagnostics;
using frame8.Logic.Misc.Visual.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.PullToRefresh
{
	public class PullToRefreshBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollRectProxy, IEventSystemHandler
	{
		private IScrollRectProxy ScrollRectProxy
		{
			get
			{
				return (this.externalScrollRectProxy != null) ? this.externalScrollRectProxy : this;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ScrollPositionChanged;

		private void Awake()
		{
			this._ResolvedAVGScreenSize = (float)(Screen.width + Screen.height) / 2f;
			this._ScrollRect = base.GetComponent<ScrollRect>();
			this._RefreshGizmo = base.GetComponentInChildren<PullToRefreshGizmo>();
			this.externalScrollRectProxy = this._ScrollRect.GetComponent(typeof(IScrollRectProxy)) as IScrollRectProxy;
		}

		private void Update()
		{
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this._IgnoreCurrentDrag = this._RefreshGizmo.IsShown;
			if (this._IgnoreCurrentDrag)
			{
				return;
			}
			this.ShowGizmo();
			this._PlayedPreSoundForCurrentDrag = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (this._IgnoreCurrentDrag)
			{
				return;
			}
			float dragAmountNormalized = this.GetDragAmountNormalized(eventData);
			if (this._RefreshGizmo)
			{
				this._RefreshGizmo.OnPull(dragAmountNormalized);
			}
			if (this.OnPullProgress != null)
			{
				this.OnPullProgress.Invoke(dragAmountNormalized);
			}
			if (dragAmountNormalized >= 1f && !this._PlayedPreSoundForCurrentDrag)
			{
				this._PlayedPreSoundForCurrentDrag = true;
				if (this._SoundOnPreRefresh)
				{
					AudioSource.PlayClipAtPoint(this._SoundOnPreRefresh, Camera.main.transform.position);
				}
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (this._IgnoreCurrentDrag)
			{
				return;
			}
			float dragAmountNormalized = this.GetDragAmountNormalized(eventData);
			if (dragAmountNormalized >= 1f)
			{
				if (this.OnRefresh != null)
				{
					this.OnRefresh.Invoke();
				}
				if (this._RefreshGizmo)
				{
					this._RefreshGizmo.OnRefreshed(this._AutoHideRefreshGizmo);
				}
				if (this._SoundOnRefresh)
				{
					AudioSource.PlayClipAtPoint(this._SoundOnRefresh, Camera.main.transform.position);
				}
			}
			else if (this._RefreshGizmo)
			{
				this._RefreshGizmo.OnRefreshCancelled();
			}
			if (this._AutoHideRefreshGizmo)
			{
				this.HideGizmo();
			}
		}

		public void ShowGizmo()
		{
			if (this._RefreshGizmo)
			{
				this._RefreshGizmo.IsShown = true;
			}
		}

		public void HideGizmo()
		{
			if (this._RefreshGizmo)
			{
				this._RefreshGizmo.IsShown = false;
			}
		}

		private float GetDragAmountNormalized(PointerEventData eventData)
		{
			float normalizedPosition = this.ScrollRectProxy.GetNormalizedPosition();
			float num;
			if (this._ScrollRect.vertical)
			{
				if (normalizedPosition < 1f)
				{
					return 0f;
				}
				num = Mathf.Abs(eventData.pressPosition.y - eventData.position.y);
			}
			else
			{
				if (normalizedPosition > 0f)
				{
					return 0f;
				}
				num = Mathf.Abs(eventData.pressPosition.x - eventData.position.x);
			}
			return Mathf.Abs(num) / (this._PullAmountNormalized * this._ResolvedAVGScreenSize);
		}

		public void SetNormalizedPosition(float normalizedPosition)
		{
		}

		public float GetNormalizedPosition()
		{
			if (this._ScrollRect.vertical)
			{
				return this._ScrollRect.verticalNormalizedPosition;
			}
			return this._ScrollRect.horizontalNormalizedPosition;
		}

		public float GetContentSize()
		{
			throw new NotImplementedException();
		}

		[SerializeField]
		[Range(0.1f, 1f)]
		[Tooltip("The normalized distance relative to screen size. Always between 0 and 1")]
		private float _PullAmountNormalized = 0.25f;

		[SerializeField]
		[Tooltip("If null, will try to GetComponentInChildren()")]
		private PullToRefreshGizmo _RefreshGizmo;

		[SerializeField]
		[Tooltip("If false, you'll need to call HideGizmo() manually after pull. Subscribe to PullToRefreshBehaviour.OnRefresh event to know when a refresh event occurred")]
		private bool _AutoHideRefreshGizmo = true;

		[SerializeField]
		private AudioClip _SoundOnPreRefresh;

		[SerializeField]
		private AudioClip _SoundOnRefresh;

		public UnityEvent OnRefresh;

		public PullToRefreshBehaviour.UnityEventFloat OnPullProgress;

		public IScrollRectProxy externalScrollRectProxy;

		private ScrollRect _ScrollRect;

		private float _ResolvedAVGScreenSize;

		private bool _PlayedPreSoundForCurrentDrag;

		private bool _IgnoreCurrentDrag;

		[Serializable]
		public class UnityEventFloat : UnityEvent<float>
		{
		}
	}
}
