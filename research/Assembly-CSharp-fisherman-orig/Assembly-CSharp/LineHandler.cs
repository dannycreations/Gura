using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class LineHandler : MonoBehaviour
{
	private string LlSufix { get; set; }

	private void Awake()
	{
		this.LlSufix = MeasuringSystemManager.LineLengthSufix();
	}

	private void FixedUpdate()
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		PlayerController player = GameFactory.Player;
		if (player == null)
		{
			return;
		}
		bool flag = player.Rod != null && player.RodSlot != null && player.RodSlot.LineClips != null && player.RodSlot.LineClips.Count > 0;
		bool flag2 = CatchedFishInfoHandler.IsDisplayed() || (player.Reel != null && player.Reel.IsFightingMode);
		this._clippedLineLength.text = ((!flag || flag2) ? string.Empty : string.Format(" \ue72c {0} {1} ", player.LineLength, this.LlSufix));
		this._lineLengthControl.text = ((flag && !flag2) ? string.Format("/ <color=#7C7C7CFF>{0} {1} </color>", player.LineLengthAvailable, this.LlSufix) : string.Format("/ {0} {1}", (!flag) ? this.LineLength.ToString(CultureInfo.InvariantCulture) : player.LineLength.ToString(CultureInfo.InvariantCulture), this.LlSufix));
		this._lineInWaterControl.text = this.LineInWater.ToString(CultureInfo.InvariantCulture);
	}

	[SerializeField]
	private TextMeshProUGUI _lineLengthControl;

	[SerializeField]
	private TextMeshProUGUI _clippedLineLength;

	[SerializeField]
	private TextMeshProUGUI _lineInWaterControl;

	public int LineLength;

	public int LineInWater;
}
