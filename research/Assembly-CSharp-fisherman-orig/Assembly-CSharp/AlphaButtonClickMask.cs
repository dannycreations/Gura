using System;
using UnityEngine;
using UnityEngine.UI;

public class AlphaButtonClickMask : MonoBehaviour, ICanvasRaycastFilter
{
	public void Start()
	{
		this._image = base.GetComponent<Image>();
		Texture2D texture = this._image.sprite.texture;
		bool flag = false;
		if (texture != null)
		{
			try
			{
				texture.GetPixels32();
			}
			catch (UnityException ex)
			{
				Debug.LogError(ex.Message);
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			Debug.LogError("This script need an Image with a readbale Texture2D to work.");
		}
	}

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		Vector2 vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(this._image.rectTransform, sp, eventCamera, ref vector);
		Vector2 pivot = this._image.rectTransform.pivot;
		Vector2 vector2;
		vector2..ctor(pivot.x + vector.x / this._image.rectTransform.rect.width, pivot.y + vector.y / this._image.rectTransform.rect.height);
		Vector2 vector3;
		vector3..ctor(this._image.sprite.rect.x + vector2.x * this._image.sprite.rect.width, this._image.sprite.rect.y + vector2.y * this._image.sprite.rect.height);
		vector3.x /= (float)this._image.sprite.texture.width;
		vector3.y /= (float)this._image.sprite.texture.height;
		return this._image.sprite.texture.GetPixelBilinear(vector3.x, vector3.y).a > 0.1f;
	}

	protected Image _image;
}
