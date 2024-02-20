using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ManagedHintObject3DSpriteRenderer : ManagedHintObject
{
	protected void Awake()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			SpriteRenderer component = base.transform.GetChild(i).GetComponent<SpriteRenderer>();
			if (component != null)
			{
				this.SpriteRenderers.Add(component);
			}
		}
		this.SpriteRenderers.ForEach(delegate(SpriteRenderer p)
		{
			ShortcutExtensions.DOFade(p, 0f, 0f);
		});
	}

	protected override void Show()
	{
		this.SpriteRenderers.ForEach(delegate(SpriteRenderer p)
		{
			ShortcutExtensions.DOFade(p, 1f, 0.2f);
		});
	}

	protected override void Hide()
	{
		this.SpriteRenderers.ForEach(delegate(SpriteRenderer p)
		{
			ShortcutExtensions.DOFade(p, 0f, 0.2f);
		});
	}

	protected List<SpriteRenderer> SpriteRenderers = new List<SpriteRenderer>();
}
