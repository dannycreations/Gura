using System;
using UnityEngine;

public class ButtonOptionsHandler : MonoBehaviour
{
	public void OpenOptions()
	{
		BackFromSettingsHandler.LastMainWindows = StaticUserData.CurrentForm;
		base.GetComponent<ChangeFormByName>().ChangeWithHideDashboard();
	}
}
