using System;
using System.Collections;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class DiscreteScrollbar : MonoBehaviour
	{
		private void Awake()
		{
			this._ScrollRect = base.GetComponentInParent<ScrollRect>();
			if (this._ScrollRect == null)
			{
				throw new UnityException(base.GetType().Name + ": No ScrollRect found in parent");
			}
			this._Scrollbar = base.GetComponent<Scrollbar>();
			this._ScrollbarPanelRT = this._Scrollbar.transform as RectTransform;
			this._OneIfVert_ZeroIfHor = ((!this._ScrollRect.horizontal) ? 1 : 0);
		}

		private void OnEnable()
		{
			this._UpdatePending = false;
		}

		public void OnScrollbarSizeChanged()
		{
			base.StartCoroutine(this.UpdateSize());
		}

		private IEnumerator UpdateSize()
		{
			while (this._UpdatePending)
			{
				yield return null;
			}
			this._UpdatePending = true;
			yield return null;
			if (this.getItemsCountFunc == null)
			{
				throw new UnityException(base.GetType().Name + "getItemsCountFunc==null. Please specify a count provider");
			}
			this._UpdatePending = true;
			int count = this.getItemsCountFunc();
			if (count > 100)
			{
				throw new UnityException(string.Concat(new object[]
				{
					base.GetType().Name,
					": count is ",
					count,
					". Bigger than MAX_COUNT=",
					100,
					". Are you sure you want to use a discrete scrollbar?"
				}));
			}
			this.Rebuild(count);
			this._UpdatePending = false;
			yield break;
		}

		public void Rebuild(int numSlots)
		{
			this.slotPrefab.gameObject.SetActive(true);
			if (this.slots != null)
			{
				foreach (RectTransform rectTransform in this.slots)
				{
					Object.Destroy(rectTransform.gameObject);
				}
			}
			this.slots = new RectTransform[numSlots];
			float num = 0f;
			float num2 = this._ScrollbarPanelRT.rect.size[this._OneIfVert_ZeroIfHor] / (float)numSlots;
			RectTransform.Edge edge = ((this._OneIfVert_ZeroIfHor != 1) ? 0 : 2);
			for (int j = 0; j < numSlots; j++)
			{
				RectTransform component = Object.Instantiate<GameObject>(this.slotPrefab.gameObject).GetComponent<RectTransform>();
				this.slots[j] = component;
				component.SetParent(this.slotsParent, false);
				component.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(edge, num, num2);
				num += num2;
				int copyOfI = j;
				component.GetComponentInChildren<Button>().onClick.AddListener(delegate
				{
					if (this.OnSlotSelected != null)
					{
						this.OnSlotSelected.Invoke(copyOfI);
					}
				});
			}
			this.slotPrefab.gameObject.SetActive(false);
		}

		public RectTransform slotPrefab;

		public RectTransform slotsParent;

		public DiscreteScrollbar.UnityIntEvent OnSlotSelected;

		public Func<int> getItemsCountFunc;

		private Scrollbar _Scrollbar;

		private RectTransform[] slots = new RectTransform[0];

		private RectTransform _ScrollbarPanelRT;

		private ScrollRect _ScrollRect;

		private int _OneIfVert_ZeroIfHor;

		private const int MAX_COUNT = 100;

		private bool _UpdatePending;

		[Serializable]
		public class UnityIntEvent : UnityEvent<int>
		{
		}
	}
}
