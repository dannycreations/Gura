using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class ExpandCollapseOnClick : MonoBehaviour
	{
		private void Awake()
		{
			this.rectTransform = base.transform as RectTransform;
			if (this.button == null)
			{
				this.button = base.GetComponent<Button>();
			}
			if (this.button)
			{
				this.button.onClick.AddListener(new UnityAction(this.OnClicked));
			}
		}

		public void OnClicked()
		{
			if (this.animating)
			{
				return;
			}
			if (this.nonExpandedSize < 0f)
			{
				return;
			}
			this.animating = true;
			this.animStart = Time.time;
			if (this.expanded)
			{
				this.startSize = this.nonExpandedSize * this.expandFactor;
				this.endSize = this.nonExpandedSize;
			}
			else
			{
				this.startSize = this.nonExpandedSize;
				this.endSize = this.nonExpandedSize * this.expandFactor;
			}
		}

		private void Update()
		{
			if (this.animating)
			{
				float num = Time.time - this.animStart;
				float num2 = num / this.animDuration;
				if (num2 >= 1f)
				{
					num2 = 1f;
					this.animating = false;
				}
				else
				{
					num2 = Mathf.Sqrt(num2);
				}
				float num3 = Mathf.Lerp(this.startSize, this.endSize, num2);
				if (this.sizeChangesHandler != null)
				{
					if (!this.sizeChangesHandler.HandleSizeChangeRequest(this.rectTransform, num3))
					{
						this.animating = false;
					}
					if (!this.animating)
					{
						this.expanded = !this.expanded;
						this.sizeChangesHandler.OnExpandedStateChanged(this.rectTransform, this.expanded);
					}
				}
				if (this.onExpandAmounChanged != null)
				{
					this.onExpandAmounChanged.Invoke(num2);
				}
			}
		}

		[Tooltip("will be taken from this object, if not specified")]
		public Button button;

		[NonSerialized]
		public float expandFactor = 2f;

		public float animDuration = 0.2f;

		[HideInInspector]
		public float nonExpandedSize = -1f;

		[HideInInspector]
		public bool expanded;

		public ExpandCollapseOnClick.UnityFloatEvent onExpandAmounChanged;

		private float startSize;

		private float endSize;

		private float animStart;

		private bool animating;

		private RectTransform rectTransform;

		public ExpandCollapseOnClick.ISizeChangesHandler sizeChangesHandler;

		public interface ISizeChangesHandler
		{
			bool HandleSizeChangeRequest(RectTransform rt, float newSize);

			void OnExpandedStateChanged(RectTransform rt, bool expanded);
		}

		[Serializable]
		public class UnityFloatEvent : UnityEvent<float>
		{
		}
	}
}
