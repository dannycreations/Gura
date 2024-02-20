using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasAlphaTween : BaseAlphaTween
{
	private void Update()
	{
		if (this._isPlaying)
		{
			base.GetComponent<CanvasGroup>().alpha = Mathf.Clamp01(base.GetCalculateValue(this.From, this.To, this.Duration, (DateTime.Now.Ticks - this._starTime) / 10000L));
			if ((float)((DateTime.Now.Ticks - this._starTime) / 10000L) > this.Duration * 1000f)
			{
				base.Stop();
				if (this.To == 0f && this.DisableOnZeroAlpha)
				{
					base.gameObject.SetActive(false);
				}
			}
		}
	}

	public bool DisableOnZeroAlpha;
}
