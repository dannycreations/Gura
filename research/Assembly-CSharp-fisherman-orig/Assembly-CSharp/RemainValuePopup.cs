using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class RemainValuePopup : DurabilityValuePopup
{
	protected override void SetValueToPanel(GameObject popupPanel)
	{
		popupPanel.transform.Find("Content").GetComponent<Text>().text = string.Format("{0}: {1}{2}", ScriptLocalization.Get("TimeLeftCaption"), this._damageManager.DamageHours, ScriptLocalization.Get("HoursCaptionShort"));
	}

	[SerializeField]
	private DamageIconManagerChum _damageManager;
}
