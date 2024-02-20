using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Kender.uGUI
{
	[RequireComponent(typeof(RectTransform))]
	public class ComboBox : MonoBehaviour
	{
		public bool Interactable
		{
			get
			{
				return this._interactable;
			}
			set
			{
				this._interactable = value;
				Button component = this.comboButtonRectTransform.GetComponent<Button>();
				component.interactable = this._interactable;
				Image component2 = this.comboImageRectTransform.GetComponent<Image>();
				component2.color = ((!(component2.sprite == null)) ? ((!this._interactable) ? component.colors.disabledColor : component.colors.normalColor) : new Color(1f, 1f, 1f, 0f));
				if (!Application.isPlaying)
				{
					return;
				}
				if (!this._interactable && this.overlayGO.activeSelf)
				{
					this.ToggleComboBox(false);
				}
			}
		}

		public int ItemsToDisplay
		{
			get
			{
				return this._itemsToDisplay;
			}
			set
			{
				if (this._itemsToDisplay == value)
				{
					return;
				}
				this._itemsToDisplay = value;
				this.Refresh();
			}
		}

		public bool HideFirstItem
		{
			get
			{
				return this._hideFirstItem;
			}
			set
			{
				if (value)
				{
					this.scrollOffset--;
				}
				else
				{
					this.scrollOffset++;
				}
				this._hideFirstItem = value;
				this.Refresh();
			}
		}

		public int SelectedIndex
		{
			get
			{
				return this._selectedIndex;
			}
			set
			{
				if (this._selectedIndex == value)
				{
					return;
				}
				if (value > -1 && value < this.Items.Length)
				{
					this._selectedIndex = value;
					this.RefreshSelected();
				}
			}
		}

		public ComboBoxItem[] Items
		{
			get
			{
				if (this._items == null)
				{
					this._items = new ComboBoxItem[0];
				}
				return this._items;
			}
			set
			{
				this._items = value;
				this.Refresh();
			}
		}

		private Transform canvasTransform
		{
			get
			{
				if (this._canvasTransform == null)
				{
					this._canvasTransform = base.transform;
					while (this._canvasTransform.GetComponent<Canvas>() == null)
					{
						this._canvasTransform = this._canvasTransform.parent;
					}
				}
				return this._canvasTransform;
			}
		}

		private RectTransform rectTransform
		{
			get
			{
				if (this._rectTransform == null)
				{
					this._rectTransform = base.GetComponent<RectTransform>();
				}
				return this._rectTransform;
			}
			set
			{
				this._rectTransform = value;
			}
		}

		private RectTransform buttonRectTransform
		{
			get
			{
				if (this._buttonRectTransform == null)
				{
					this._buttonRectTransform = this.rectTransform.Find("Button").GetComponent<RectTransform>();
				}
				return this._buttonRectTransform;
			}
			set
			{
				this._buttonRectTransform = value;
			}
		}

		private RectTransform comboButtonRectTransform
		{
			get
			{
				if (this._comboButtonRectTransform == null)
				{
					this._comboButtonRectTransform = this.buttonRectTransform.Find("ComboButton").GetComponent<RectTransform>();
				}
				return this._comboButtonRectTransform;
			}
			set
			{
				this._comboButtonRectTransform = value;
			}
		}

		private RectTransform comboImageRectTransform
		{
			get
			{
				if (this._comboImageRectTransform == null)
				{
					this._comboImageRectTransform = this.comboButtonRectTransform.Find("Image").GetComponent<RectTransform>();
				}
				return this._comboImageRectTransform;
			}
			set
			{
				this._comboImageRectTransform = value;
			}
		}

		private RectTransform comboTextRectTransform
		{
			get
			{
				if (this._comboTextRectTransform == null)
				{
					this._comboTextRectTransform = this.comboButtonRectTransform.Find("Text").GetComponent<RectTransform>();
				}
				return this._comboTextRectTransform;
			}
			set
			{
				this._comboTextRectTransform = value;
			}
		}

		private RectTransform comboArrowRectTransform
		{
			get
			{
				if (this._comboArrowRectTransform == null)
				{
					this._comboArrowRectTransform = this.buttonRectTransform.Find("Arrow").GetComponent<RectTransform>();
				}
				return this._comboArrowRectTransform;
			}
			set
			{
				this._comboArrowRectTransform = value;
			}
		}

		private RectTransform scrollPanelRectTransfrom
		{
			get
			{
				Transform transform = this.overlayGO.transform.Find("ScrollPanel");
				if (this._scrollPanelRectTransfrom == null && transform != null)
				{
					this._scrollPanelRectTransfrom = this.overlayGO.transform.Find("ScrollPanel").GetComponent<RectTransform>();
				}
				return this._scrollPanelRectTransfrom;
			}
			set
			{
				this._scrollPanelRectTransfrom = value;
			}
		}

		private RectTransform itemsRectTransfrom
		{
			get
			{
				if (this._itemsRectTransfrom == null)
				{
					this._itemsRectTransfrom = this.scrollPanelRectTransfrom.Find("Items").GetComponent<RectTransform>();
				}
				return this._itemsRectTransfrom;
			}
			set
			{
				this._itemsRectTransfrom = value;
			}
		}

		private RectTransform scrollbarRectTransfrom
		{
			get
			{
				if (this._scrollbarRectTransfrom == null)
				{
					this._scrollbarRectTransfrom = this.scrollPanelRectTransfrom.Find("Scrollbar").GetComponent<RectTransform>();
				}
				return this._scrollbarRectTransfrom;
			}
			set
			{
				this._scrollbarRectTransfrom = value;
			}
		}

		private RectTransform slidingAreaRectTransform
		{
			get
			{
				if (this._slidingAreaRectTransform == null)
				{
					this._slidingAreaRectTransform = this.scrollbarRectTransfrom.Find("SlidingArea").GetComponent<RectTransform>();
				}
				return this._slidingAreaRectTransform;
			}
			set
			{
				this._slidingAreaRectTransform = value;
			}
		}

		private RectTransform handleRectTransfrom
		{
			get
			{
				if (this._handleRectTransfrom == null)
				{
					this._handleRectTransfrom = this.slidingAreaRectTransform.Find("Handle").GetComponent<RectTransform>();
				}
				return this._handleRectTransfrom;
			}
			set
			{
				this._handleRectTransfrom = value;
			}
		}

		private void Awake()
		{
			this.InitControl();
		}

		public void OnItemClicked(int index)
		{
			bool flag = index != this.SelectedIndex;
			this.SelectItem(index);
			this.ToggleComboBox(true);
			if (flag && this.OnSelectionChanged != null)
			{
				this.OnSelectionChanged(index);
			}
		}

		public void SelectItem(int index)
		{
			this.SelectedIndex = index;
			if (this.OnItemSelected != null)
			{
				this.OnItemSelected(index);
			}
		}

		public void AddItems(params object[] list)
		{
			List<ComboBoxItem> list2 = new List<ComboBoxItem>();
			foreach (object obj in list)
			{
				if (obj is ComboBoxItem)
				{
					ComboBoxItem comboBoxItem = (ComboBoxItem)obj;
					list2.Add(comboBoxItem);
				}
				else if (obj is string)
				{
					ComboBoxItem comboBoxItem2 = new ComboBoxItem((string)obj, null, false, null);
					list2.Add(comboBoxItem2);
				}
				else
				{
					if (!(obj is Sprite))
					{
						throw new Exception("Only ComboBoxItem, string and Sprite types are allowed");
					}
					ComboBoxItem comboBoxItem3 = new ComboBoxItem(null, (Sprite)obj, false, null);
					list2.Add(comboBoxItem3);
				}
			}
			ComboBoxItem[] array = new ComboBoxItem[this.Items.Length + list2.Count];
			this.Items.CopyTo(array, 0);
			list2.ToArray().CopyTo(array, this.Items.Length);
			this.Refresh();
			this.Items = array;
		}

		public void ClearItems()
		{
			this.Items = new ComboBoxItem[0];
		}

		public void CreateControl()
		{
			this.rectTransform = base.GetComponent<RectTransform>();
			GameObject gameObject = new GameObject("Button");
			gameObject.transform.SetParent(base.transform, false);
			this.buttonRectTransform = gameObject.AddComponent<RectTransform>();
			this.buttonRectTransform.SetSizeWithCurrentAnchors(0, this.rectTransform.sizeDelta.x);
			this.buttonRectTransform.SetSizeWithCurrentAnchors(1, this.rectTransform.sizeDelta.y);
			this.buttonRectTransform.anchoredPosition = Vector2.zero;
			GameObject gameObject2 = new GameObject("ComboButton");
			Image image = gameObject2.AddComponent<Image>();
			gameObject2.transform.SetParent(this.buttonRectTransform, false);
			this.comboButtonRectTransform = gameObject2.AddComponent<RectTransform>();
			this.comboButtonRectTransform.SetSizeWithCurrentAnchors(0, this.buttonRectTransform.sizeDelta.x);
			this.comboButtonRectTransform.SetSizeWithCurrentAnchors(1, this.buttonRectTransform.sizeDelta.y);
			this.comboButtonRectTransform.anchoredPosition = Vector2.zero;
			image.sprite = this.Sprite_UISprite;
			image.type = 1;
			Button button = gameObject2.AddComponent<Button>();
			button.targetGraphic = image;
			ColorBlock colorBlock = default(ColorBlock);
			colorBlock.normalColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			colorBlock.highlightedColor = new Color32(245, 245, 245, byte.MaxValue);
			colorBlock.pressedColor = new Color32(200, 200, 200, byte.MaxValue);
			colorBlock.disabledColor = new Color32(200, 200, 200, 128);
			colorBlock.colorMultiplier = 1f;
			colorBlock.fadeDuration = 0.1f;
			button.colors = colorBlock;
			GameObject gameObject3 = new GameObject("Arrow");
			gameObject3.transform.SetParent(this.buttonRectTransform, false);
			Text text = gameObject3.AddComponent<Text>();
			text.color = new Color32(0, 0, 0, byte.MaxValue);
			text.alignment = 4;
			text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			text.text = "▼";
			this.comboArrowRectTransform.localScale = new Vector3(1f, 0.5f, 1f);
			this.comboArrowRectTransform.pivot = new Vector2(1f, 0.5f);
			this.comboArrowRectTransform.anchorMin = Vector2.right;
			this.comboArrowRectTransform.anchorMax = Vector2.one;
			this.comboArrowRectTransform.anchoredPosition = Vector2.zero;
			this.comboArrowRectTransform.SetSizeWithCurrentAnchors(0, this.comboButtonRectTransform.sizeDelta.y);
			this.comboArrowRectTransform.SetSizeWithCurrentAnchors(1, this.comboButtonRectTransform.sizeDelta.y);
			CanvasGroup canvasGroup = gameObject3.AddComponent<CanvasGroup>();
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			GameObject gameObject4 = new GameObject("Image");
			Image image2 = gameObject4.AddComponent<Image>();
			gameObject4.transform.SetParent(this.comboButtonRectTransform, false);
			image2.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
			this.comboImageRectTransform.pivot = Vector2.up;
			this.comboImageRectTransform.anchorMin = Vector2.zero;
			this.comboImageRectTransform.anchorMax = Vector2.up;
			this.comboImageRectTransform.anchoredPosition = new Vector2(4f, -4f);
			this.comboImageRectTransform.SetSizeWithCurrentAnchors(0, this.comboButtonRectTransform.sizeDelta.y - 8f);
			this.comboImageRectTransform.SetSizeWithCurrentAnchors(1, this.comboButtonRectTransform.sizeDelta.y - 8f);
			GameObject gameObject5 = new GameObject("Text");
			gameObject5.transform.SetParent(this.comboButtonRectTransform, false);
			Text text2 = gameObject5.AddComponent<Text>();
			text2.color = new Color32(0, 0, 0, byte.MaxValue);
			text2.alignment = 3;
			text2.lineSpacing = 1.2f;
			text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			this.comboTextRectTransform.pivot = Vector2.up;
			this.comboTextRectTransform.anchorMin = Vector2.zero;
			this.comboTextRectTransform.anchorMax = Vector2.one;
			this.comboTextRectTransform.anchoredPosition = new Vector2(10f, 0f);
			this.comboTextRectTransform.offsetMax = new Vector2(4f, 0f);
			this.comboTextRectTransform.SetSizeWithCurrentAnchors(1, this.comboButtonRectTransform.sizeDelta.y);
		}

		private void InitControl()
		{
			Transform transform = base.transform.Find("Button/ComboButton/Image");
			Transform transform2 = base.transform.Find("Button/ComboButton/Text");
			Transform transform3 = base.transform.Find("Button/Arrow");
			if (transform == null || transform2 == null || transform3 == null)
			{
				IEnumerator enumerator = base.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform4 = (Transform)obj;
						Object.Destroy(transform4);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = enumerator as IDisposable) != null)
					{
						disposable.Dispose();
					}
				}
				this.CreateControl();
			}
			this.comboButtonRectTransform.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.ToggleComboBox(false);
			});
			float num = this.comboButtonRectTransform.sizeDelta.y * (float)Mathf.Min(this.ItemsToDisplay, this.Items.Length - ((!this.HideFirstItem) ? 0 : 1));
			this.overlayGO = new GameObject("CBOverlay");
			this.overlayGO.SetActive(false);
			Image image = this.overlayGO.AddComponent<Image>();
			image.color = new Color32(0, 0, 0, 0);
			this.overlayGO.transform.SetParent(this.canvasTransform, false);
			RectTransform component = this.overlayGO.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			Button button = this.overlayGO.AddComponent<Button>();
			button.targetGraphic = image;
			button.onClick.AddListener(delegate
			{
				this.ToggleComboBox(false);
			});
			GameObject gameObject = new GameObject("ScrollPanel");
			Image image2 = gameObject.AddComponent<Image>();
			image2.sprite = this.Sprite_Background;
			image2.type = 1;
			gameObject.transform.SetParent(this.overlayGO.transform, false);
			this.scrollPanelRectTransfrom.pivot = new Vector2(0.5f, 1f);
			this.scrollPanelRectTransfrom.anchorMin = new Vector2(0.5f, 0.5f);
			this.scrollPanelRectTransfrom.anchorMax = new Vector2(0.5f, 0.5f);
			gameObject.transform.SetParent(base.transform, false);
			this.scrollPanelRectTransfrom.anchoredPosition = new Vector2(0f, -this.rectTransform.sizeDelta.y * (float)this._itemsToDisplay);
			gameObject.transform.SetParent(this.overlayGO.transform, true);
			this.scrollPanelRectTransfrom.SetSizeWithCurrentAnchors(0, this.comboButtonRectTransform.sizeDelta.x);
			this.scrollPanelRectTransfrom.SetSizeWithCurrentAnchors(1, num);
			ScrollRect scrollRect = gameObject.AddComponent<ScrollRect>();
			scrollRect.horizontal = false;
			scrollRect.elasticity = 0f;
			scrollRect.movementType = 2;
			scrollRect.inertia = false;
			scrollRect.scrollSensitivity = this.comboButtonRectTransform.sizeDelta.y;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.AddComponent<Mask>();
			float num2 = ((this.Items.Length - ((!this.HideFirstItem) ? 0 : 1) <= this._itemsToDisplay) ? 0f : this._scrollbarWidth);
			GameObject gameObject2 = new GameObject("Items");
			gameObject2.transform.SetParent(gameObject.transform, false);
			this.itemsRectTransfrom = gameObject2.AddComponent<RectTransform>();
			this.itemsRectTransfrom.pivot = Vector2.up;
			this.itemsRectTransfrom.anchorMin = Vector2.up;
			this.itemsRectTransfrom.anchorMax = Vector2.one;
			this.itemsRectTransfrom.anchoredPosition = Vector2.right;
			this.itemsRectTransfrom.SetSizeWithCurrentAnchors(0, this.scrollPanelRectTransfrom.sizeDelta.x - num2);
			ContentSizeFitter contentSizeFitter = gameObject2.AddComponent<ContentSizeFitter>();
			contentSizeFitter.horizontalFit = 2;
			contentSizeFitter.verticalFit = 2;
			GridLayoutGroup gridLayoutGroup = gameObject2.AddComponent<GridLayoutGroup>();
			gridLayoutGroup.cellSize = new Vector2(this.comboButtonRectTransform.sizeDelta.x - num2, this.comboButtonRectTransform.sizeDelta.y);
			gridLayoutGroup.constraint = 1;
			gridLayoutGroup.constraintCount = 1;
			scrollRect.content = this.itemsRectTransfrom;
			GameObject gameObject3 = new GameObject("Scrollbar");
			Image image3 = gameObject3.AddComponent<Image>();
			gameObject3.transform.SetParent(gameObject.transform, false);
			image3.color = new Color32(232, 232, 232, byte.MaxValue);
			image3.sprite = this.Sprite_UISprite;
			image3.type = 1;
			Scrollbar scrollbar = gameObject3.AddComponent<Scrollbar>();
			ColorBlock colorBlock = default(ColorBlock);
			colorBlock.normalColor = new Color32(104, 110, 127, byte.MaxValue);
			colorBlock.highlightedColor = new Color32(191, 191, 191, byte.MaxValue);
			colorBlock.pressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			colorBlock.disabledColor = new Color32(128, 128, 128, byte.MaxValue);
			colorBlock.colorMultiplier = 1f;
			colorBlock.fadeDuration = 0.1f;
			scrollbar.colors = colorBlock;
			scrollRect.verticalScrollbar = scrollbar;
			scrollbar.direction = 2;
			this.scrollbarRectTransfrom.pivot = Vector2.one;
			this.scrollbarRectTransfrom.anchorMin = Vector2.one;
			this.scrollbarRectTransfrom.anchorMax = Vector2.one;
			this.scrollbarRectTransfrom.anchoredPosition = Vector2.zero;
			this.scrollbarRectTransfrom.SetSizeWithCurrentAnchors(0, num2);
			this.scrollbarRectTransfrom.SetSizeWithCurrentAnchors(1, num);
			GameObject gameObject4 = new GameObject("SlidingArea");
			gameObject4.transform.SetParent(gameObject3.transform, false);
			this.slidingAreaRectTransform = gameObject4.AddComponent<RectTransform>();
			this.slidingAreaRectTransform.anchoredPosition = Vector2.zero;
			this.slidingAreaRectTransform.SetSizeWithCurrentAnchors(0, 10f);
			this.slidingAreaRectTransform.SetSizeWithCurrentAnchors(1, num);
			GameObject gameObject5 = new GameObject("Handle");
			Image image4 = gameObject5.AddComponent<Image>();
			gameObject5.transform.SetParent(gameObject4.transform, false);
			image4.sprite = this.Sprite_UISprite;
			image4.type = 1;
			image4.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 150);
			scrollbar.targetGraphic = image4;
			scrollbar.handleRect = this.handleRectTransfrom;
			this.handleRectTransfrom.pivot = new Vector2(0.5f, 0.5f);
			this.handleRectTransfrom.anchorMin = new Vector2(0.5f, 0.5f);
			this.handleRectTransfrom.anchorMax = new Vector2(0.5f, 0.5f);
			this.handleRectTransfrom.anchoredPosition = Vector2.zero;
			this.handleRectTransfrom.localPosition = new Vector3(0f, 0f, 0f);
			this.handleRectTransfrom.offsetMin = new Vector2(0f, 0f);
			this.handleRectTransfrom.offsetMax = new Vector2(0f, 0f);
			this.handleRectTransfrom.SetSizeWithCurrentAnchors(1, num2);
			this.Interactable = this.Interactable;
			AutoScroll autoScroll = gameObject.AddComponent<AutoScroll>();
			autoScroll._verticalScrollBar = scrollRect.verticalScrollbar;
			autoScroll.useParentHeight = false;
			DropDownList component2 = base.GetComponent<DropDownList>();
			if (component2 != null)
			{
				component2._transitionRegionRoot = gameObject2.transform;
				DropDownList dropDownList = component2;
				dropDownList.OnClick = (Action)Delegate.Combine(dropDownList.OnClick, new Action(delegate
				{
					this.ToggleComboBox(true);
				}));
			}
			if (this.Items.Length < 1)
			{
				return;
			}
			this.Refresh();
		}

		private void Refresh()
		{
			GridLayoutGroup component = this.itemsRectTransfrom.GetComponent<GridLayoutGroup>();
			int num = this.Items.Length - ((!this.HideFirstItem) ? 0 : 1);
			float num2 = this.comboButtonRectTransform.sizeDelta.y * (float)Mathf.Min(this._itemsToDisplay, num);
			float num3 = ((num <= this.ItemsToDisplay) ? 0f : this._scrollbarWidth);
			this.scrollPanelRectTransfrom.SetSizeWithCurrentAnchors(1, num2);
			this.scrollbarRectTransfrom.SetSizeWithCurrentAnchors(0, num3);
			this.scrollbarRectTransfrom.SetSizeWithCurrentAnchors(1, num2);
			this.slidingAreaRectTransform.SetSizeWithCurrentAnchors(1, num2 - this.scrollbarRectTransfrom.sizeDelta.x);
			this.itemsRectTransfrom.SetSizeWithCurrentAnchors(0, this.scrollPanelRectTransfrom.sizeDelta.x - num3);
			component.cellSize = new Vector2(this.comboButtonRectTransform.sizeDelta.x - num3, this.comboButtonRectTransform.sizeDelta.y);
			for (int i = this.itemsRectTransfrom.childCount - 1; i > -1; i--)
			{
				Object.DestroyImmediate(this.itemsRectTransfrom.GetChild(0).gameObject);
			}
			for (int j = 0; j < this.Items.Length; j++)
			{
				if (!this.HideFirstItem || j != 0)
				{
					ComboBoxItem item = this.Items[j];
					item.OnUpdate = new Action(this.Refresh);
					Transform transform = Object.Instantiate<RectTransform>(this.comboButtonRectTransform);
					transform.SetParent(this.itemsRectTransfrom, false);
					transform.GetComponent<Image>().sprite = null;
					this.Items[j].AdditionalAction(transform.gameObject);
					Text component2 = transform.Find("Text").GetComponent<Text>();
					component2.text = item.Caption;
					if (item.IsDisabled)
					{
						component2.color = new Color32(174, 174, 174, byte.MaxValue);
					}
					Image component3 = transform.Find("Image").GetComponent<Image>();
					component3.sprite = item.Image;
					component3.color = ((!(item.Image == null)) ? ((!item.IsDisabled) ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 147)) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0));
					Button component4 = transform.GetComponent<Button>();
					component4.interactable = !item.IsDisabled;
					int index = j;
					component4.onClick.AddListener(delegate
					{
						this.OnItemClicked(index);
						if (item.OnSelect != null)
						{
							item.OnSelect();
						}
					});
				}
			}
			this.scrollbarRectTransfrom.gameObject.SetActive(this.Items.Count<ComboBoxItem>() > this.ItemsToDisplay);
			this.RefreshSelected();
			this.UpdateComboBoxImages();
			this.FixScrollOffset();
			this.UpdateHandle();
		}

		public void RefreshSelected()
		{
			Image image = this.comboImageRectTransform.GetComponent<Image>();
			ComboBoxItem comboBoxItem = ((this.SelectedIndex <= -1 || this.SelectedIndex >= this.Items.Length) ? null : this.Items[this.SelectedIndex]);
			bool flag = comboBoxItem != null && comboBoxItem.Image != null;
			image.sprite = ((!flag) ? null : comboBoxItem.Image);
			Button component = this.comboButtonRectTransform.GetComponent<Button>();
			image.color = ((!flag) ? new Color(1f, 1f, 1f, 0f) : ((!this.Interactable) ? component.colors.disabledColor : component.colors.normalColor));
			this.UpdateComboBoxImage(this.comboButtonRectTransform, flag);
			this.comboTextRectTransform.GetComponent<Text>().text = ((comboBoxItem == null) ? string.Empty : comboBoxItem.Caption);
			if (!Application.isPlaying)
			{
				return;
			}
			int num = 0;
			IEnumerator enumerator = this.itemsRectTransfrom.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					image = transform.GetComponent<Image>();
					image.color = ((this.SelectedIndex != num + ((!this.HideFirstItem) ? 0 : 1)) ? component.colors.normalColor : component.colors.highlightedColor);
					num++;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private void UpdateComboBoxImages()
		{
			bool flag = false;
			foreach (ComboBoxItem comboBoxItem in this.Items)
			{
				if (comboBoxItem.Image != null)
				{
					flag = true;
					break;
				}
			}
			IEnumerator enumerator = this.itemsRectTransfrom.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					this.UpdateComboBoxImage(transform, flag);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private void UpdateComboBoxImage(Transform comboButton, bool includeImage)
		{
			comboButton.Find("Text").GetComponent<RectTransform>().offsetMin = Vector2.right * ((!includeImage) ? 10f : (this.comboImageRectTransform.rect.width + 8f));
		}

		private void FixScrollOffset()
		{
			int num = this.SelectedIndex + ((!this.HideFirstItem) ? 0 : 1);
			if (num < this.scrollOffset)
			{
				this.scrollOffset = num;
			}
			else if (num > this.scrollOffset + this.ItemsToDisplay - 1)
			{
				this.scrollOffset = num - this.ItemsToDisplay + 1;
			}
			int num2 = this.Items.Length - ((!this.HideFirstItem) ? 0 : 1);
			if (this.scrollOffset > num2 - this.ItemsToDisplay)
			{
				this.scrollOffset = num2 - this.ItemsToDisplay;
			}
			if (this.scrollOffset < 0)
			{
				this.scrollOffset = 0;
			}
			this.itemsRectTransfrom.anchoredPosition = new Vector2(0f, (float)this.scrollOffset * this.rectTransform.sizeDelta.y);
		}

		private void ToggleComboBox(bool directClick)
		{
			this.overlayGO.SetActive(!this.overlayGO.activeSelf);
			if (this.overlayGO.activeSelf)
			{
				float y = this.buttonRectTransform.sizeDelta.y;
				float x = this.buttonRectTransform.sizeDelta.x;
				float y2 = this.buttonRectTransform.position.y;
				float x2 = this.buttonRectTransform.position.x;
				int num = Mathf.Min(this._itemsToDisplay, this.Items.Length - ((!this.HideFirstItem) ? 0 : 1));
				float num2 = y2 - y * 0.5f;
				float num3 = x2;
				this.scrollPanelRectTransfrom.position = new Vector3(num3, num2, this.scrollPanelRectTransfrom.position.z);
				this.FixScrollOffset();
			}
			else if (directClick)
			{
				this.scrollOffset = (int)Mathf.Round(this.itemsRectTransfrom.anchoredPosition.y / this.rectTransform.sizeDelta.y);
			}
		}

		public void UpdateGraphics()
		{
			this.UpdateHandle();
			if (this.rectTransform.sizeDelta != this.buttonRectTransform.sizeDelta && this.buttonRectTransform.sizeDelta == this.comboButtonRectTransform.sizeDelta)
			{
				this.buttonRectTransform.SetSizeWithCurrentAnchors(0, this.rectTransform.sizeDelta.x);
				this.buttonRectTransform.SetSizeWithCurrentAnchors(1, this.rectTransform.sizeDelta.y);
				this.comboButtonRectTransform.SetSizeWithCurrentAnchors(0, this.rectTransform.sizeDelta.x);
				this.comboButtonRectTransform.SetSizeWithCurrentAnchors(1, this.rectTransform.sizeDelta.y);
				this.comboArrowRectTransform.SetSizeWithCurrentAnchors(0, this.rectTransform.sizeDelta.y);
				this.comboImageRectTransform.SetSizeWithCurrentAnchors(0, this.comboImageRectTransform.rect.height);
				this.comboTextRectTransform.offsetMax = new Vector2(4f, 0f);
				if (this.overlayGO == null)
				{
					return;
				}
				this.scrollPanelRectTransfrom.SetParent(base.transform, true);
				this.scrollPanelRectTransfrom.anchoredPosition = new Vector2(0f, -this.rectTransform.sizeDelta.y * (float)this._itemsToDisplay);
				RectTransform component = this.overlayGO.GetComponent<RectTransform>();
				component.SetParent(this.canvasTransform, false);
				component.offsetMin = Vector2.zero;
				component.offsetMax = Vector2.zero;
				this.scrollPanelRectTransfrom.SetParent(component, true);
				this.scrollPanelRectTransfrom.GetComponent<ScrollRect>().scrollSensitivity = this.comboButtonRectTransform.sizeDelta.y;
				this.UpdateComboBoxImage(this.comboButtonRectTransform, this.Items[this.SelectedIndex].Image != null);
				this.Refresh();
			}
		}

		private void UpdateHandle()
		{
			if (this.overlayGO == null)
			{
				return;
			}
			float num = ((this.Items.Length - ((!this.HideFirstItem) ? 0 : 1) <= this.ItemsToDisplay) ? 0f : this._scrollbarWidth);
		}

		public Sprite Sprite_UISprite;

		public Sprite Sprite_Background;

		public Action<int> OnSelectionChanged;

		public Action<int> OnItemSelected;

		[SerializeField]
		private bool _interactable = true;

		[SerializeField]
		private int _itemsToDisplay = 4;

		[SerializeField]
		private bool _hideFirstItem;

		[SerializeField]
		private int _selectedIndex;

		[SerializeField]
		private ComboBoxItem[] _items;

		private GameObject overlayGO;

		private int scrollOffset;

		private float _scrollbarWidth = 10f;

		private Transform _canvasTransform;

		private RectTransform _rectTransform;

		private RectTransform _buttonRectTransform;

		private RectTransform _comboButtonRectTransform;

		private RectTransform _comboImageRectTransform;

		private RectTransform _comboTextRectTransform;

		private RectTransform _comboArrowRectTransform;

		private RectTransform _scrollPanelRectTransfrom;

		private RectTransform _itemsRectTransfrom;

		private RectTransform _scrollbarRectTransfrom;

		private RectTransform _slidingAreaRectTransform;

		private RectTransform _handleRectTransfrom;
	}
}
