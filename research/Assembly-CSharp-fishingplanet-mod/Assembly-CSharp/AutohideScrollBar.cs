using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AutohideScrollBar : MonoBehaviour
{
	protected bool? IsActive { get; set; }

	protected void Update()
	{
		bool flag = this.Scroll.size < 0.98f;
		if (this.IsActive == null || flag != this.IsActive)
		{
			this.IsActive = new bool?(flag);
			ShortcutExtensions.DOKill(this.ScrollCanvas, false);
			ShortcutExtensions.DOFade(this.ScrollCanvas, (!flag) ? 0f : 1f, 0.25f);
			this.ScrollCanvas.interactable = flag;
		}
	}

	[SerializeField]
	protected Scrollbar Scroll;

	[SerializeField]
	protected CanvasGroup ScrollCanvas;

	protected const float AnimSpeed = 0.25f;

	protected const float ScrollSizeActive = 0.98f;
}
