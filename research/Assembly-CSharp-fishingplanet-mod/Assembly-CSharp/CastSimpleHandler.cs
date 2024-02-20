using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CastSimpleHandler : MonoBehaviour
{
	internal void Update()
	{
		float num = this.CurrentValue / this.MaxValue;
		this.CastIndicator.fillAmount = num;
		if (this.clipCanvasGroup.alpha > 0f && GameFactory.Player.RodSlot != null && GameFactory.Player.RodSlot.LineClips != null && GameFactory.Player.RodSlot.LineClips.Count == 0)
		{
			this.clipCanvasGroup.alpha = 0f;
		}
	}

	internal void OnDisable()
	{
		this.CastIndicator.fillAmount = 0f;
	}

	private void Awake()
	{
		this.ClippedLabel.text = ScriptLocalization.Get("LineClippedCaption");
	}

	public void SetClipLength(float length)
	{
		if (GameFactory.Player.Rod == null)
		{
			return;
		}
		length += GameFactory.Player.Rod.LineOnRodLength;
		float num = length / this.MaxValue;
		if (num >= 1f)
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

	public const float HEIGHT = 370f;

	public const float DEADZONE = 16f;

	public Image CastIndicator;

	public float MaxValue = 1f;

	public float CurrentValue;

	[SerializeField]
	private RectTransform ClipperZone;

	[SerializeField]
	private CanvasGroup clipCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI ClippedLabel;

	[SerializeField]
	private TextMeshProUGUI ClippedLength;
}
