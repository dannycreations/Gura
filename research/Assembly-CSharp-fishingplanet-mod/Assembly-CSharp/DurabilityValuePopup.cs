using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityValuePopup : PopupInfo
{
	protected override void Start()
	{
		base.Start();
		this.PopupParentName = ActivityState.GetParentActivityState(base.transform).name;
	}

	protected override void SetValueToPanel(GameObject popupPanel)
	{
		Text component = popupPanel.transform.Find("Content").GetComponent<Text>();
		DamageIconManager component2 = base.GetComponent<DamageIconManager>();
		if (component2 != null)
		{
			component.text = string.Format("{0} {1}%", ScriptLocalization.Get("DurabilityCaption"), component2.DurabilityInProcent);
		}
	}
}
