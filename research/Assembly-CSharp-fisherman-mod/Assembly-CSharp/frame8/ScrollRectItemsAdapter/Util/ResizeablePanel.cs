using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class ResizeablePanel : MonoBehaviour
	{
		private float PreferredSize
		{
			get
			{
				return (this._Direction != ResizeablePanel.DIRECTION.HORIZONTAL) ? this._LayoutElement.preferredHeight : this._LayoutElement.preferredWidth;
			}
			set
			{
				if (this._Direction == ResizeablePanel.DIRECTION.HORIZONTAL)
				{
					this._LayoutElement.preferredWidth = value;
				}
				else
				{
					this._LayoutElement.preferredHeight = value;
				}
			}
		}

		private void Start()
		{
			Canvas.ForceUpdateCanvases();
			this._LayoutElement = base.GetComponent<LayoutElement>();
			if (this._Expanded)
			{
				if (this._ExpandedSize == -1f)
				{
					this._ExpandedSize = this.PreferredSize;
				}
			}
			else if (this._NonExpandedSize == -1f)
			{
				this._NonExpandedSize = this.PreferredSize;
			}
			Transform transform = base.transform;
			while ((transform = transform.parent) && !this._NearestScrollRectInParents)
			{
				this._NearestScrollRectInParents = transform.GetComponent<ScrollRect>();
			}
		}

		public void ToggleExpandedState()
		{
			bool expandedToSet = !this._Expanded;
			float preferredSize = this.PreferredSize;
			float num;
			if (expandedToSet)
			{
				num = this._ExpandedSize;
			}
			else
			{
				num = this._NonExpandedSize;
			}
			base.StartCoroutine(this.StartAnimating(preferredSize, num, delegate
			{
				this._Expanded = expandedToSet;
				if (this.onExpandedStateChanged != null)
				{
					this.onExpandedStateChanged.Invoke(this._Expanded);
				}
			}));
		}

		private IEnumerator StartAnimating(float from, float to, Action onDone)
		{
			float startTime = Time.time;
			float t;
			do
			{
				yield return null;
				float elapsed = Time.time - startTime;
				t = elapsed / this._AnimTime;
				if (t > 1f)
				{
					t = 1f;
				}
				else
				{
					t = Mathf.Sqrt(t);
				}
				this.PreferredSize = from * (1f - t) + to * t;
				if (this._RebuildNearestScrollRectParentDuringAnimation && this._NearestScrollRectInParents)
				{
					this._NearestScrollRectInParents.OnScroll(new PointerEventData(EventSystem.current));
				}
			}
			while (t < 1f);
			if (onDone != null)
			{
				onDone();
			}
			yield break;
		}

		[SerializeField]
		private bool _Expanded;

		[Tooltip("Only needed to be set if starting with _Expanded=false")]
		[SerializeField]
		private float _ExpandedSize;

		[Tooltip("Only needed to be set if starting with _Expanded=true")]
		[SerializeField]
		private float _NonExpandedSize;

		[SerializeField]
		private float _AnimTime = 1f;

		[SerializeField]
		private ResizeablePanel.DIRECTION _Direction;

		[SerializeField]
		private bool _RebuildNearestScrollRectParentDuringAnimation;

		[SerializeField]
		private ResizeablePanel.UnityEventBool onExpandedStateChanged;

		private LayoutElement _LayoutElement;

		private ScrollRect _NearestScrollRectInParents;

		private bool _Animating;

		public enum DIRECTION
		{
			HORIZONTAL,
			VERTICAL
		}

		[Serializable]
		public class UnityEventBool : UnityEvent<bool>
		{
		}
	}
}
