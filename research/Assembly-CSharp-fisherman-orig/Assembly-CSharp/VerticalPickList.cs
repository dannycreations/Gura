using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VerticalPickList : MonoBehaviour
{
	public List<string> Items
	{
		get
		{
			return this._items;
		}
		set
		{
			this._items = value;
		}
	}

	public int Index
	{
		get
		{
			return this._index;
		}
		set
		{
			if (value < this.Items.Count && value >= 0)
			{
				this._index = value;
				this._currentLabel.text = this.Items[this._index];
				if (this._index == 0)
				{
					this._prevButton.interactable = false;
					if (this._prevIcon != null)
					{
						this._prevIcon.color = this.inactiveColor;
					}
					else
					{
						Transform transform = this._prevButton.transform.Find("label");
						if (transform != null)
						{
							transform.GetComponent<Text>().color = this.inactiveColor;
						}
					}
				}
				else
				{
					this._prevButton.interactable = true;
					if (this._prevIcon != null)
					{
						this._prevIcon.color = this.activeColor;
					}
					else
					{
						Transform transform2 = this._prevButton.transform.Find("label");
						if (transform2 != null)
						{
							transform2.GetComponent<Text>().color = this.activeColor;
						}
					}
				}
				if (this._index == this.Items.Count - 1)
				{
					this._nextButton.interactable = false;
					if (this._nextIcon != null)
					{
						this._nextIcon.color = this.inactiveColor;
					}
					else
					{
						Transform transform3 = this._nextButton.transform.Find("label");
						if (transform3 != null)
						{
							transform3.GetComponent<Text>().color = this.inactiveColor;
						}
					}
				}
				else
				{
					this._nextButton.interactable = true;
					if (this._nextIcon != null)
					{
						this._nextIcon.color = this.activeColor;
					}
					else
					{
						Transform transform4 = this._nextButton.transform.Find("label");
						if (transform4 != null)
						{
							transform4.GetComponent<Text>().color = this.activeColor;
						}
					}
				}
			}
		}
	}

	private void Start()
	{
		this._prevButton.onClick.AddListener(new UnityAction(this.PrevClicked));
		this._nextButton.onClick.AddListener(new UnityAction(this.NextClicked));
	}

	private void Destroy()
	{
		this._prevButton.onClick.RemoveListener(new UnityAction(this.PrevClicked));
		this._nextButton.onClick.RemoveListener(new UnityAction(this.NextClicked));
	}

	private void OnEnable()
	{
		this._currentLabelTransform = this._currentLabel.GetComponent<RectTransform>();
		this._hiddenLabelTransform = this._hiddenLabel.GetComponent<RectTransform>();
		this._firstCurrentLabelPosition = this._currentLabelTransform.anchoredPosition;
		this._firstHiddenLabelPosition = this._hiddenLabelTransform.anchoredPosition;
	}

	private void NextClicked()
	{
		if (this.Index < this.Items.Count - 1)
		{
			this.ShowLabel(this.Index + 1);
		}
	}

	private void PrevClicked()
	{
		if (this.Index > 0)
		{
			this.ShowLabel(this.Index - 1);
		}
	}

	public void ShowLabel(int index)
	{
		if (!this._inProgress)
		{
			this._inProgress = true;
			this._openNextIndex = -1;
			float num;
			float num2;
			if (this._index > index)
			{
				num = this._currentLabelTransform.anchoredPosition.x + this._currentLabelTransform.rect.width;
				num2 = this._currentLabelTransform.anchoredPosition.x - this._currentLabelTransform.rect.width;
			}
			else
			{
				num = this._currentLabelTransform.anchoredPosition.x - this._currentLabelTransform.rect.width;
				num2 = this._currentLabelTransform.anchoredPosition.x + this._currentLabelTransform.rect.width;
			}
			this._hiddenLabel.text = this.Items[index];
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._currentLabelTransform, new Vector2(num, 0f), this._duration, true), 8), delegate
			{
				this.Index = index;
				this._inProgress = false;
				if (this._openNextIndex != -1)
				{
					this.ShowLabel(this._openNextIndex);
				}
				this._currentLabel.text = this.Items[this.Index];
				this._currentLabelTransform.anchoredPosition = this._firstCurrentLabelPosition;
			});
			this._hiddenLabelTransform.anchoredPosition = new Vector2(num2, this._firstHiddenLabelPosition.y);
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._hiddenLabelTransform, new Vector2(this._firstCurrentLabelPosition.x, this._firstHiddenLabelPosition.y), this._duration, true), 8), delegate
			{
				this._hiddenLabelTransform.anchoredPosition = this._firstHiddenLabelPosition;
			});
		}
		else
		{
			this._openNextIndex = index;
		}
	}

	[SerializeField]
	private Button _prevButton;

	[SerializeField]
	private Button _nextButton;

	[SerializeField]
	private Graphic _prevIcon;

	[SerializeField]
	private Graphic _nextIcon;

	[SerializeField]
	private Text _currentLabel;

	[SerializeField]
	private Text _hiddenLabel;

	private RectTransform _currentLabelTransform;

	private RectTransform _hiddenLabelTransform;

	private float _duration = 0.1f;

	private int _index;

	private int _openNextIndex = -1;

	private bool _inProgress;

	private Vector3 _firstCurrentLabelPosition;

	private Vector3 _firstHiddenLabelPosition;

	private List<string> _items = new List<string>();

	private Color32 activeColor = new Color32(247, 247, 247, byte.MaxValue);

	private Color32 inactiveColor = new Color32(42, 42, 42, byte.MaxValue);
}
