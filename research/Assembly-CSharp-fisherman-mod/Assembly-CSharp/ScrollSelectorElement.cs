using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollSelectorElement : MonoBehaviour, ISelectHandler, IScrollHandler, IEventSystemHandler
{
	public void Init(ScrollSelectorMessageBox parent, RectTransform content)
	{
		this._parent = parent;
		this._inited = true;
		this._viewport = content;
		this._myTransform = base.transform as RectTransform;
		this.OnInputTypeChanged(SettingsManager.InputType);
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void OnInputTypeChanged(InputModuleManager.InputType obj)
	{
		ColorBlock colors = this.Button.colors;
		colors.highlightedColor = ((obj != InputModuleManager.InputType.Mouse) ? Color.clear : this.highlighted);
		this.Button.colors = colors;
	}

	public void SetSlected(bool selected)
	{
		this.Name.fontStyle = ((!selected) ? 0 : 1);
		ShortcutExtensions.DOFade(this.Icon, (!selected) ? 0f : 1f, 0f);
	}

	public void Update()
	{
		if (!this._inited)
		{
			return;
		}
		this._myTransform.GetWorldCorners(this._corners);
		this._corners[0] = this._viewport.InverseTransformPoint(this._corners[0]);
		this._corners[2] = this._viewport.InverseTransformPoint(this._corners[2]);
		if (this._enabled)
		{
			if (!this._viewport.rect.Contains(this._corners[0]) && !this._viewport.rect.Contains(this._corners[2]))
			{
				this._enabled = false;
				this.Name.gameObject.SetActive(this._enabled);
				this.Icon.gameObject.SetActive(this._enabled);
			}
		}
		else if ((this._viewport.rect.Contains(this._corners[0]) || this._viewport.rect.Contains(this._corners[2])) && !Mathf.Approximately(this._myTransform.rect.height, 0f))
		{
			this._enabled = true;
			this.Name.gameObject.SetActive(this._enabled);
			this.Icon.gameObject.SetActive(this._enabled);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (this.SelectedAction != null)
		{
			this.SelectedAction();
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (this._inited)
		{
			this._parent.OnScroll(eventData);
		}
	}

	public TextMeshProUGUI Name;

	public TextMeshProUGUI Icon;

	public BorderedButton Button;

	public Action SelectedAction;

	private RectTransform _viewport;

	private RectTransform _myTransform;

	private bool _enabled = true;

	private bool _inited;

	private ScrollSelectorMessageBox _parent;

	private Color highlighted = new Color(1f, 1f, 1f, 0.1f);

	private Vector3[] _corners = new Vector3[4];
}
