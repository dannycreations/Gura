using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CastTargetHandler : MonoBehaviour
{
	private void Awake()
	{
		this._indicatitorRect = this.TargetIndicator.GetComponent<RectTransform>();
		this.ClippedLabel.text = ScriptLocalization.Get("LineClippedCaption");
	}

	public void SetClipLength(float length)
	{
		length += GameFactory.Player.Rod.LineOnRodLength;
		float num = length / this.MaxValue;
		if (num > 1f)
		{
			this.clipCanvasGroup.alpha = 0f;
			return;
		}
		this.clipCanvasGroup.alpha = 1f;
		Vector2 sizeDelta = this.ClipperZone.sizeDelta;
		sizeDelta.y = (1f - num) * 354f;
		this.ClipperZone.sizeDelta = sizeDelta;
		this.ClippedLength.text = ((int)MeasuringSystemManager.LineLength(length)).ToString();
	}

	internal void Update()
	{
		if (GameFactory.Player.HudFishingHandler.State != HudState.EngineIgnition)
		{
			float num = this.TargetValue / this.MaxValue;
			this._indicatitorRect.anchoredPosition = new Vector2(this._indicatitorRect.localPosition.x, 370f * num);
			this._indicatitorRect.sizeDelta = new Vector2(this._indicatitorRect.sizeDelta.x, 80f - 40f * num);
		}
		this.CastIndicator.fillAmount = this.CastMultiplier;
		if (this.clipCanvasGroup.alpha > 0f && GameFactory.Player.RodSlot != null && GameFactory.Player.RodSlot.LineClips != null && GameFactory.Player.RodSlot.LineClips.Count == 0)
		{
			this.clipCanvasGroup.alpha = 0f;
		}
	}

	public CastTargetResult GetTargetResult(out float accuracy)
	{
		accuracy = 0f;
		float num = this.CurrentValue / this.MaxValue * 370f;
		float num2 = this.TargetValue / this.MaxValue * 370f;
		float num3 = 40f - 20f * (this.TargetValue / this.MaxValue);
		if (num < num2 - num3)
		{
			return CastTargetResult.Undercast;
		}
		if (num > num2 + num3)
		{
			return CastTargetResult.Overcast;
		}
		accuracy = Mathf.Abs(num - num2) / num3;
		return CastTargetResult.Hitcast;
	}

	public float GetDeviation()
	{
		float num = this.CurrentValue / this.MaxValue * 370f;
		float num2 = this.TargetValue / this.MaxValue * 370f;
		float num3 = 40f - 20f * (this.TargetValue / this.MaxValue);
		float num4 = num2 - num3;
		float num5 = num2 + num3;
		if (num < num4)
		{
			return (num4 - num) / 370f;
		}
		if (num >= num5)
		{
			return (num - num5) / 370f;
		}
		return 0f;
	}

	public bool IsEngineIgnitionSuccess
	{
		get
		{
			return this.CurrentValue >= 1f - this._successZonePrc;
		}
	}

	public void SetupForEngineIgnition(float successZonePrc)
	{
		base.gameObject.SetActive(true);
		this._successZonePrc = successZonePrc;
		GameFactory.Player.OnStartEngineIgnition();
		this.MaxValue = 1f;
		this.CurrentValue = 0f;
		this._indicatitorRect.anchoredPosition = new Vector2(this._indicatitorRect.localPosition.x, 370f);
		this._indicatitorRect.sizeDelta = new Vector2(this._indicatitorRect.sizeDelta.x, 370f * this._successZonePrc * 2f);
		this.SetClipLength(float.PositiveInfinity);
	}

	public void FinishEngineIgnition()
	{
		base.gameObject.SetActive(false);
		GameFactory.Player.OnFinishEngineIgnition();
	}

	public float CastMultiplier
	{
		get
		{
			return this.CurrentValue / this.MaxValue;
		}
	}

	public const float HEIGHT = 370f;

	public const float DEADZONE = 16f;

	public Image CastIndicator;

	public GameObject TargetIndicator;

	public float TargetValue;

	public float MaxValue;

	public float CurrentValue;

	private float _successZonePrc;

	private RectTransform _indicatitorRect;

	[SerializeField]
	private RectTransform ClipperZone;

	[SerializeField]
	private CanvasGroup clipCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI ClippedLabel;

	[SerializeField]
	private TextMeshProUGUI ClippedLength;
}
