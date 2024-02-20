using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScrollSelectorInit : MonoBehaviour
{
	private void Awake()
	{
		this.button = base.GetComponent<BorderedButton>();
	}

	private void RefreshValueText()
	{
		this.Value.text = this._data[this._index].Text;
	}

	public List<IScrollSelectorElement> GetElements()
	{
		return this._data;
	}

	public void SavePrevious()
	{
		if (this._previous != null)
		{
			return;
		}
		IScrollSelectorElement currentValue = this.GetCurrentValue();
		if (currentValue != null)
		{
			this._previous = currentValue;
		}
	}

	private void ResetPrevious()
	{
		this._previous = null;
	}

	public void SetElements(List<IScrollSelectorElement> data)
	{
		this._data = data;
		this._default = this._data.FirstOrDefault((IScrollSelectorElement x) => x.IsDefault);
		this._hasDefault = this._default != null;
		if (this._previous != null && this._data.Contains(this._previous))
		{
			this._index = this._data.IndexOf(this._previous);
		}
	}

	public void Init(string header, List<IScrollSelectorElement> data, int selectedIndex, string placeholderText, Action onPicked)
	{
		if (this.button == null)
		{
			this.Awake();
		}
		this.Header.text = header;
		this.SetElements(data);
		this.SetIndex(selectedIndex);
		if (string.IsNullOrEmpty(placeholderText))
		{
			placeholderText = ScriptLocalization.Get("ActivateSearchField");
		}
		this.button.onClick.RemoveAllListeners();
		this.button.onClick.AddListener(delegate
		{
			int num = this._index;
			if (this._previous != null)
			{
				num = ((!this._data.Contains(this._previous)) ? 0 : this._data.IndexOf(this._previous));
			}
			MessageBoxBase messageBoxBase = MenuHelpers.Instance.ShowSearchSelectorMessage(this._data, num, this.button.GetBorderTransform(), placeholderText, new Action<int>(this.SetCurrent));
			messageBoxBase.AfterFullyHidden.AddListener(new UnityAction(onPicked.Invoke));
		});
	}

	public void SetIndex(int index)
	{
		this.ResetPrevious();
		this._index = index;
		this.RefreshValueText();
	}

	private void SetCurrent(int index)
	{
		if (this._hasDefault && index == this._index)
		{
			index = this._data.IndexOf(this._default);
		}
		this.SetIndex(index);
	}

	public IScrollSelectorElement GetCurrentValue()
	{
		if (this._data == null || this._index >= this._data.Count)
		{
			return null;
		}
		return this._data[this._index];
	}

	public TextMeshProUGUI Header;

	public TextMeshProUGUI Value;

	private BorderedButton button;

	private List<IScrollSelectorElement> _data;

	private int _index;

	private bool _hasDefault;

	private IScrollSelectorElement _default;

	private IScrollSelectorElement _previous;
}
