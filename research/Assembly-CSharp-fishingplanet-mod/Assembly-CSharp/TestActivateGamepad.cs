using System;
using UnityEngine;

public class TestActivateGamepad : MonoBehaviour
{
	private void Start()
	{
		SettingsManager.InputType = InputModuleManager.InputType.GamePad;
	}
}
