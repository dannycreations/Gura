using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class SupportInfoForm : SupportBaseForm
{
	private void Awake()
	{
		this._lefInput.text = string.Format(ScriptLocalization.Get("SupportFormInfoText1").Replace("\\n", Environment.NewLine), PhotonConnectionFactory.Instance.Profile.Name);
		this._rightInput.text = ScriptLocalization.Get("SupportFormInfoText2PS4").Replace("\\n", Environment.NewLine);
	}

	[SerializeField]
	private InputField _lefInput;

	[SerializeField]
	private InputField _rightInput;
}
