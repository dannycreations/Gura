using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollSelectorMessageBox : MessageBoxBase, IScrollHandler, IEventSystemHandler
{
	protected override void Awake()
	{
		base.Awake();
		this.ControlsActionsToUse = new List<string> { "KeyboardReturn", "KeyboardEscape" };
		this._defaultHeight = (this.prefab.transform as RectTransform).rect.height;
		this._alphaFade.FastHidePanel();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
		this.InputField.onValueChanged.AddListener(new UnityAction<string>(this.Search));
		this.InputField.onSubmit.AddListener(new UnityAction<string>(this.Find));
		this.ScrollRect.scrollSensitivity = 0f;
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	public void ActivateInputField()
	{
		UINavigation.SetSelectedGameObject(this.InputField.gameObject);
	}

	public new void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this._mouse = type == InputModuleManager.InputType.Mouse;
	}

	public void Init(List<IScrollSelectorElement> data, int selectedIndex, RectTransform selectorTransform, string placeholderText, Action<int> onPick)
	{
		this._mouse = SettingsManager.InputType == InputModuleManager.InputType.Mouse;
		Vector3[] array = new Vector3[4];
		this.elementsList = data;
		this.SearchContent.gameObject.SetActive(this.elementsList.Count > 10);
		this._selectedIndex = selectedIndex;
		selectorTransform.GetWorldCorners(array);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = base.transform.InverseTransformPoint(array[i]);
		}
		float num = Mathf.Abs(array[3].x - array[0].x);
		float num2 = Mathf.Abs(array[1].y - array[0].y);
		this.SelectorView.anchorMin = Vector2.one * 0.5f;
		this.SelectorView.anchorMax = Vector2.one * 0.5f;
		this.SelectorView.pivot = Vector2.zero;
		this.SelectorView.anchoredPosition = new Vector2(array[0].x, array[0].y);
		this.SelectorView.SetSizeWithCurrentAnchors(0, num);
		this.SelectorView.SetSizeWithCurrentAnchors(1, num2);
		this.normalizedViewportOffset = 1f - this.ScrollViewport.InverseTransformPoint(selectorTransform.position).y / this.ScrollViewport.rect.height;
		float num3 = (this.ScrollViewport.InverseTransformPoint(selectorTransform.position).y - this._defaultHeight) / this.ScrollViewport.rect.height;
		this.topOffset = this.normalizedViewportOffset * this.ScrollViewport.rect.height;
		this.bottomOffset = num3 * this.ScrollViewport.rect.height;
		this.vlg.padding.top = (int)this.topOffset;
		this.vlg.padding.bottom = (int)this.bottomOffset;
		float num4 = 0f;
		for (int j = 0; j < data.Count; j++)
		{
			ScrollSelectorElement item = Object.Instantiate<ScrollSelectorElement>(this.prefab, this.Content, false);
			bool flag = j == selectedIndex;
			item.Init(this, this.ScrollViewport);
			if (flag && !this._mouse)
			{
				UINavigation.SetSelectedGameObject(item.gameObject);
				item.Button.Select();
			}
			item.SetSlected(flag);
			if (flag)
			{
				this._lastSelected = item;
			}
			item.Name.text = data[j].Text.ToUpper();
			float preferredWidth = item.Name.preferredWidth;
			if (num4 < preferredWidth)
			{
				num4 = preferredWidth;
			}
			this._items.Add(item);
			int index = j;
			item.SelectedAction = delegate
			{
				if (this._mouse)
				{
					if (!this._ignoreSelect)
					{
						item.Button.onClick.Invoke();
					}
				}
				else if (this._filteredItems.Count > 0)
				{
					this.ScrollTo(this._filteredItems.IndexOf(item));
				}
				else
				{
					this.ScrollTo(index);
				}
			};
			item.Button.onClick.AddListener(delegate
			{
				this._lastSelected.SetSlected(false);
				item.SetSlected(true);
				onPick(index);
				this._lastSelected = item;
				this.Close();
			});
		}
		if (num4 > this.WidthParent.rect.width)
		{
			this.WidthParent.SetSizeWithCurrentAnchors(0, num4);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.Content);
		foreach (ScrollSelectorElement scrollSelectorElement in this._items)
		{
			(scrollSelectorElement.transform as RectTransform).SetSizeWithCurrentAnchors(0, this.Content.rect.width);
		}
		this.Navigation.ForceUpdateImmediately();
		this.Navigation.UpdateFromEventSystemSelected();
		base.AlphaFade.OnShowCalled.AddListener(delegate
		{
			this.StartCoroutine(this.Init());
		});
		(this.InputField.placeholder as TextMeshProUGUI).text = placeholderText;
	}

	private IEnumerator Init()
	{
		yield return null;
		yield return null;
		Vector3[] corners = new Vector3[4];
		if (this._items.Count > 0)
		{
			(this._items[0].transform as RectTransform).GetWorldCorners(corners);
			for (int i = 0; i < corners.Length; i++)
			{
				corners[i] = base.transform.InverseTransformPoint(corners[i]);
			}
			float num = Mathf.Abs(corners[1].y - corners[0].y);
			ShortcutExtensions.DOAnchorPos(this.SelectorView, corners[0], 0.15f, false);
			ShortcutExtensions.DOSizeDelta(this.SelectorView, new Vector2(this.Content.rect.width, num), 0.15f, false);
			this.OnInputTypeChanged(SettingsManager.InputType);
			this.ScrollTo(this._selectedIndex);
			this._ignoreSelect = true;
			UINavigation.SetSelectedGameObject(this._items[this._selectedIndex].gameObject);
			this.Navigation.UpdateFromEventSystemSelected();
			this._ignoreSelect = false;
		}
		yield break;
	}

	public void Search(string request)
	{
		this._selectedIndex = 0;
		this._filteredItems.Clear();
		if (string.IsNullOrEmpty(request))
		{
			foreach (ScrollSelectorElement scrollSelectorElement in this._items)
			{
				if (this.UseSize)
				{
					scrollSelectorElement.transform.localScale = Vector3.one;
					(scrollSelectorElement.transform as RectTransform).SetSizeWithCurrentAnchors(1, this._defaultHeight);
				}
				else
				{
					scrollSelectorElement.gameObject.SetActive(true);
				}
			}
			return;
		}
		for (int i = 0; i < this.elementsList.Count; i++)
		{
			IScrollSelectorElement scrollSelectorElement2 = this.elementsList[i];
			if (scrollSelectorElement2.Text.ToLower().Contains(request.ToLower()))
			{
				this._filteredItems.Add(this._items[i]);
				if (this.UseSize)
				{
					this._items[i].transform.localScale = Vector3.one;
					(this._items[i].transform as RectTransform).SetSizeWithCurrentAnchors(1, this._defaultHeight);
				}
				else
				{
					this._items[i].gameObject.SetActive(true);
				}
			}
			else if (this.UseSize)
			{
				this._items[i].transform.localScale = Vector3.zero;
				(this._items[i].transform as RectTransform).SetSizeWithCurrentAnchors(1, 0f);
			}
			else
			{
				this._items[i].gameObject.SetActive(false);
			}
		}
		this.ScrollTo(0);
		this.Navigation.ForceUpdateImmediately();
	}

	public void Find(string request)
	{
		this.Search(request);
		if (this._filteredItems.Count > 0 && !this._mouse)
		{
			UINavigation.SetSelectedGameObject(this._filteredItems[0].gameObject);
			this.Navigation.UpdateFromEventSystemSelected();
		}
	}

	public void ScrollTo(int index)
	{
		ShortcutExtensions.DOAnchorPos(this.Content, Vector2.up * (float)index * this._defaultHeight, 0.15f, false);
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (this.InputField.isFocused)
		{
			UINavigation.SetSelectedGameObject(null);
		}
		this._selectedIndex = Mathf.Max(0, Mathf.Min((this._filteredItems.Count <= 0) ? (this._items.Count - 1) : (this._filteredItems.Count - 1), this._selectedIndex - (int)eventData.scrollDelta.y));
		this.ScrollTo(this._selectedIndex);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		this._selectedIndex = Mathf.RoundToInt(this.Content.anchoredPosition.y / this._defaultHeight);
		this.ScrollTo(this._selectedIndex);
	}

	protected virtual void Update()
	{
		if (this.InputField.isFocused != this._lastFocused)
		{
			if (this._lastFocused)
			{
				this._lastFocusedTime = Time.time;
			}
			this._lastFocused = this.InputField.isFocused;
		}
		if (ControlsController.ControlsActions.KeyboardEscape.WasPressed && !this.InputField.isFocused && Time.time - this._lastFocusedTime > 0.1f)
		{
			this.Close();
		}
		if (ControlsController.ControlsActions.KeyboardReturn.WasPressed)
		{
			if (this.InputField.isFocused)
			{
				this.Find(this.InputField.text);
			}
			else
			{
				UINavigation.SetSelectedGameObject((this._filteredItems.Count <= 0) ? this._items[this._selectedIndex].gameObject : this._filteredItems[this._selectedIndex].gameObject);
				this.Navigation.UpdateFromEventSystemSelected();
			}
		}
		if (!this._dragging)
		{
			if (Input.GetMouseButton(0))
			{
				this._dragging = true;
			}
		}
		else if (!Input.GetMouseButton(0))
		{
			this._selectedIndex = Mathf.RoundToInt(this.Content.anchoredPosition.y / this._defaultHeight);
			this.ScrollTo(this._selectedIndex);
			this._dragging = false;
		}
	}

	public RectTransform WidthParent;

	public RectTransform ScrollViewport;

	public RectTransform SelectorView;

	public TMP_InputField InputField;

	public ScrollRect ScrollRect;

	public RectTransform Content;

	public RectTransform SearchContent;

	public VerticalLayoutGroup vlg;

	public ScrollSelectorElement prefab;

	public UINavigation Navigation;

	private ScrollSelectorElement _lastSelected;

	private bool _mouse;

	private bool _ignoreSelect;

	private int _selectedIndex;

	private float topOffset;

	private float bottomOffset;

	private List<ScrollSelectorElement> _items = new List<ScrollSelectorElement>();

	private List<ScrollSelectorElement> _filteredItems = new List<ScrollSelectorElement>();

	private float normalizedViewportOffset;

	private List<IScrollSelectorElement> elementsList;

	private float _defaultHeight;

	public bool UseSize = true;

	private bool _dragging;

	private bool _lastFocused;

	private float _lastFocusedTime;
}
