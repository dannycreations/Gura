using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ProgressDisplayer : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFinish = delegate
	{
	};

	public void InitProgressDisplay(float prev, float curr)
	{
		if (curr < prev)
		{
			return;
		}
		base.StopAllCoroutines();
		Vector2 size = this.Background.rectTransform.rect.size;
		this._width = size.x;
		MonoBehaviour.print(this._width);
		this.Background.color = this.BackgroundColor;
		this.CurrentProgress.color = this.PendingProgressColor;
		this.PrevProgress.color = this.ProgressColor;
		this.DeltaToFill.color = this.DeltaColor;
		this.DeltaGlow.color = new Color(this.DeltaColor.r, this.DeltaColor.g, this.DeltaColor.b, this.DeltaColor.a / 4f);
		size.x = this._width * curr;
		this.CurrentProgress.rectTransform.sizeDelta = size;
		size.x = this._width * prev;
		this.PrevProgress.rectTransform.sizeDelta = size;
		this._deltaFillAmount = curr - prev;
		size.x = 0f;
		this.DeltaToFill.rectTransform.sizeDelta = size;
		this._deltaGlowFade = this.DeltaGlow.GetComponent<AlphaFade>();
		this._deltaGlowFade.FastHidePanel();
		this._deltaGlowFade.ShowFadeTime = this.GlowShowHideDelay;
		this._deltaGlowFade.HideFadeTime = this.GlowShowHideDelay;
		this._currProgressFade = this.CurrentProgress.GetComponent<AlphaFade>();
		this._currProgressFade.FastHidePanel();
		this._currProgressFade.ShowFadeTime = this.ProgressPreviewDelay;
		base.StartCoroutine(this.DisplayProgress());
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	private IEnumerator DisplayProgress()
	{
		this._currProgressFade.ShowPanel();
		yield return new WaitForSeconds(this.ProgressPreviewDelay);
		this._deltaGlowFade.ShowPanel();
		float glowStartTime = Time.time;
		float endWidth = this._deltaFillAmount * this._width;
		while (this.DeltaToFill.rectTransform.sizeDelta.x < endWidth)
		{
			this.DeltaToFill.rectTransform.sizeDelta += new Vector2(this._width * this.FillSpeed * Time.deltaTime, 0f);
			yield return null;
		}
		Vector2 size = this.DeltaToFill.rectTransform.sizeDelta;
		size.x = endWidth;
		this.DeltaToFill.rectTransform.sizeDelta = size;
		float colorLerpTime = 0f;
		while (colorLerpTime < this.ColorCooldownDelay)
		{
			Color newColor = Color.Lerp(this.DeltaColor, this.ProgressColor, colorLerpTime / this.ColorCooldownDelay);
			this.DeltaToFill.color = newColor;
			this.DeltaGlow.color = new Color(newColor.r, newColor.g, newColor.b, newColor.a / 4f);
			colorLerpTime += Time.deltaTime;
			yield return null;
		}
		float timeSinceShowedGlow = Time.time - glowStartTime;
		if (timeSinceShowedGlow < this.GlowShowHideDelay)
		{
			yield return new WaitForSeconds(this.GlowShowHideDelay - timeSinceShowedGlow);
		}
		this._deltaGlowFade.HideFinished += this._deltaGlowFade_HideFinished;
		this._deltaGlowFade.HidePanel();
		yield break;
	}

	private void _deltaGlowFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._deltaGlowFade.HideFinished -= this._deltaGlowFade_HideFinished;
		this.OnFinish();
	}

	[SerializeField]
	private Image Background;

	[SerializeField]
	private Image CurrentProgress;

	[SerializeField]
	private Image PrevProgress;

	[SerializeField]
	private Image DeltaToFill;

	[SerializeField]
	private Image DeltaGlow;

	[SerializeField]
	private Color BackgroundColor;

	[SerializeField]
	private Color ProgressColor;

	[SerializeField]
	private Color PendingProgressColor;

	[SerializeField]
	private Color DeltaColor;

	private AlphaFade _deltaGlowFade;

	private AlphaFade _currProgressFade;

	private float _deltaFillAmount;

	private float _width;

	public float FillSpeed = 0.1f;

	public float ProgressPreviewDelay = 0.5f;

	public float GlowShowHideDelay = 0.5f;

	public float ColorCooldownDelay = 0.5f;
}
