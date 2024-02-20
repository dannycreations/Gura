using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class EULAScreenInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> EULAAccepted;

	private void OnEnable()
	{
		string text = "\ue708";
		this.EulaMessage.text = string.Format(ScriptLocalization.Get("EULAMessage").Replace("<br>", "\n"), text);
	}

	public void AcceptEULA()
	{
		PhotonConnectionFactory.Instance.OnEulaSigned += this.InstanceOnOnEulaSigned;
		PhotonConnectionFactory.Instance.OnEulaSignFailed += this.Instance_OnEulaSignFailed;
		this._waitingPanel = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.WaitingPanel);
		this._waitingPanel.SetActive(true);
		PhotonConnectionFactory.Instance.SignEula(1);
	}

	public void ShowEULA()
	{
		Application.OpenURL(ScriptLocalization.Get("EULA_URL"));
	}

	private void Instance_OnEulaSignFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnEulaSigned -= this.InstanceOnOnEulaSigned;
		PhotonConnectionFactory.Instance.OnEulaSignFailed -= this.Instance_OnEulaSignFailed;
		if (this._waitingPanel != null)
		{
			Object.Destroy(this._waitingPanel);
		}
	}

	private void InstanceOnOnEulaSigned()
	{
		PhotonConnectionFactory.Instance.OnEulaSigned -= this.InstanceOnOnEulaSigned;
		PhotonConnectionFactory.Instance.OnEulaSignFailed -= this.Instance_OnEulaSignFailed;
		if (this._waitingPanel != null)
		{
			Object.Destroy(this._waitingPanel);
		}
		if (this.EULAAccepted != null)
		{
			this.EULAAccepted(this, null);
		}
	}

	private string EulaUrl
	{
		get
		{
			string text = CustomLanguagesShort.EN.ToString();
			KeyValuePair<CustomLanguages, Language> keyValuePair = ChangeLanguage.LanguagesList.FirstOrDefault((KeyValuePair<CustomLanguages, Language> x) => x.Value.Id == PlayerPrefs.GetInt("CurrentLanguage"));
			if (!keyValuePair.Equals(default(KeyValuePair<CustomLanguages, Language>)))
			{
				text = ChangeLanguage.GetCustomLanguagesShort(keyValuePair.Key).ToString();
			}
			return string.Format("http://www.fishingplanet.com/consoler/lang/{0}/", text.ToUpper());
		}
	}

	public Text EulaMessage;

	public const string MainHost = "http://www.fishingplanet.com";

	public const string EulaHost = "http://www.fishingplanet.com/consoler/lang/{0}/";

	private GameObject _waitingPanel;
}
