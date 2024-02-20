using System;
using DG.Tweening;
using UnityEngine;

public class UnderlineHint : ManagedHintObject
{
	private void Awake()
	{
		this.rt = base.transform.parent as RectTransform;
		this.width = this.rt.rect.width;
		this.height = this.rt.rect.height;
	}

	protected override void Show()
	{
		this.SetSizes();
		ShortcutExtensions.DOFade(this.group, 1f, 0.1f);
		this.BubbleParticles.Play();
	}

	protected override void Hide()
	{
		ShortcutExtensions.DOFade(this.group, 0f, 0.1f);
		this.BubbleParticles.Stop();
	}

	private void SetSizes()
	{
		if (this.rt == null)
		{
			this.rt = base.transform.parent as RectTransform;
		}
		this.width = this.rt.rect.width;
		this.height = this.rt.rect.height;
		this.BloomBottom.sizeDelta = new Vector2(this.width * 1.5f, this.width * 1.5f);
		this.BloomTop.sizeDelta = this.BloomBottom.sizeDelta;
		this.BloomTop.anchoredPosition = new Vector2(0f, this.width * 0.7f);
		this.BloomBottom.anchoredPosition = new Vector2(0f, -this.width * 0.7f);
		this.BubbleParticles.transform.localPosition = new Vector3(0f, -this.height * 0.5f, 0f);
		this.BubbleParticles.shape.radius = this.width * 0.5f;
	}

	protected override void Update()
	{
		if (Mathf.Abs(this.rt.rect.width - this.width) > Mathf.Epsilon)
		{
			this.SetSizes();
		}
		base.Update();
	}

	public ParticleSystem BubbleParticles;

	public RectTransform BloomTop;

	public RectTransform BloomBottom;

	private RectTransform rt;

	private float width;

	private float height;
}
