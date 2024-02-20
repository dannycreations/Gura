using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuWidthSet : MonoBehaviour
{
	public float PreferredWidth
	{
		get
		{
			return (!(this._textForResize != null)) ? 0f : this._textForResize.preferredWidth;
		}
	}

	private void Awake()
	{
		this._le = base.GetComponent<LayoutElement>();
	}

	private void Start()
	{
		if (this._textForResize != null)
		{
			this._le.preferredWidth = this._textForResize.preferredWidth + this._spacing;
			if (this._rtForResize != null)
			{
				this._rtForResize.sizeDelta = new Vector2((this._rtMultiply > 0f) ? (this._textForResize.preferredWidth * this._rtMultiply) : this._textForResize.preferredWidth, this._rtForResize.rect.height);
			}
			if (this._rtForResize2 != null)
			{
				this._rtForResize2.anchoredPosition = new Vector2(this._textForResize.preferredWidth / 2f + this._spacing2, this._rtForResize2.anchoredPosition.y);
			}
			if (this._leForResize != null)
			{
				this._leForResize.preferredWidth = this._rtForResize.rect.width + this._leForResizeSpacing;
			}
		}
		else
		{
			Transform transform = base.transform.Find("Label");
			if (transform == null)
			{
				return;
			}
			float preferredWidth = transform.GetComponent<Text>().preferredWidth;
			this._le.preferredWidth = preferredWidth + this._spacing;
		}
	}

	private void OnEnable()
	{
		this.Start();
	}

	public void Refresh()
	{
		if (this._le == null)
		{
			this.Awake();
		}
		this.Start();
	}

	[SerializeField]
	private float _rtMultiply;

	[SerializeField]
	private float _spacing = 20f;

	[SerializeField]
	private Text _textForResize;

	[SerializeField]
	private RectTransform _rtForResize;

	[SerializeField]
	private RectTransform _rtForResize2;

	[SerializeField]
	private float _spacing2 = 22f;

	[SerializeField]
	private LayoutElement _leForResize;

	[SerializeField]
	private float _leForResizeSpacing = 5f;

	private LayoutElement _le;
}
