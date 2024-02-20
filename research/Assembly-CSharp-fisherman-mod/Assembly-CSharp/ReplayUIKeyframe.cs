using System;
using UnityEngine;
using UnityEngine.UI;

public class ReplayUIKeyframe : MonoBehaviour
{
	private void Awake()
	{
		this._image = base.GetComponent<Image>();
		this._selectionColor = this._image.color;
	}

	public void SetPosition(float x)
	{
		RectTransform component = base.GetComponent<RectTransform>();
		component.anchoredPosition = new Vector2(x, this._y);
	}

	public void Select(bool flag)
	{
		this._image.color = ((!flag) ? Color.black : this._selectionColor);
	}

	[SerializeField]
	private float _y = 10f;

	private Image _image;

	private Color _selectionColor;

	private RectTransform _transform;
}
