using System;
using System.Collections;
using UnityEngine;

public class OnHoverHide : MonoBehaviour
{
	private void Awake()
	{
		if (this.group == null)
		{
			this.group = base.GetComponent<CanvasGroup>();
		}
		if (this.collider == null)
		{
			this.collider = base.GetComponent<BoxCollider2D>();
		}
		this._rt = base.transform as RectTransform;
		this._parentCanvas = UIHelper.GetTopmostCanvas(this);
	}

	private void OnMouseOver()
	{
		this.isOver = true;
		this.time = 0f;
	}

	private void OnMouseExit()
	{
		this.isOver = false;
	}

	private IEnumerator Tween(float value1, float value2)
	{
		float t = 0f;
		while (t < 1f)
		{
			this.group.alpha = Mathf.Lerp(value1, value2, t);
			t += 5f * Time.deltaTime;
			yield return null;
		}
		this.group.alpha = value2;
		yield break;
	}

	private void FixedUpdate()
	{
		if (this.collider.size.x != this._rt.rect.width || this.collider.size.y != this._rt.rect.height)
		{
			this.collider.size = new Vector2(this._rt.rect.width, this._rt.rect.height);
		}
		if (this._parentCanvas != null && this._parentCanvas.renderMode == null && SettingsManager.InputType == InputModuleManager.InputType.Mouse)
		{
			this.isOver = RectTransformUtility.RectangleContainsScreenPoint(this._rt, Input.mousePosition);
			if (this.isOver)
			{
				this.time = 0f;
			}
		}
		if (this.isOver)
		{
			if (!this.fading)
			{
				base.StopAllCoroutines();
				base.StartCoroutine(this.Tween(1f, 0f));
				this.fading = true;
			}
			if (this.time > 0.2f)
			{
				this.isOver = false;
				this.fading = true;
			}
		}
		else if (this.fading && this.time > 1f)
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.Tween(this.group.alpha, 1f));
			this.fading = false;
		}
		this.time += Time.fixedDeltaTime;
	}

	public bool isOver;

	public bool fading;

	public CanvasGroup group;

	public BoxCollider2D collider;

	private float time;

	private RectTransform _rt;

	private Canvas _parentCanvas;
}
