using System;
using UnityEngine;
using UnityEngine.UI;

public class SetHeightForContent : MonoBehaviour
{
	private void Start()
	{
		this._rectTransform = base.GetComponent<RectTransform>();
		this._text = base.GetComponent<Text>();
	}

	private void Update()
	{
		if (Math.Abs(this._rectTransform.rect.height - this._text.preferredHeight) > 0.1f)
		{
			this._rectTransform.sizeDelta = new Vector2(this._rectTransform.sizeDelta.x, this._text.preferredHeight);
		}
	}

	private RectTransform _rectTransform;

	private Text _text;
}
