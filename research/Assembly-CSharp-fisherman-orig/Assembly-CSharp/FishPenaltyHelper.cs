using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishPenaltyHelper : MonoBehaviour
{
	public void CheckPenalty(bool isPriority = false)
	{
		this._isPriority = isPriority;
		this._penaltyInfoWarning = null;
		this._penaltyInfo = null;
		PhotonConnectionFactory.Instance.OnLicensePenalty += this.Instance_OnLicensePenalty;
		PhotonConnectionFactory.Instance.OnLicensePenaltyWarning += this.Instance_OnLicensePenaltyWarning;
		this._checkPenaltyPanel = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.CheckPenaltyPanel).GetComponent<MessageBox>();
		this._checkPenaltyPanel.OnPriority = isPriority;
		this._checkPenaltyPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this._timeChecking = 0f;
		this._checkPenaltyPanel.GetComponent<AlphaFade>().ShowFinished += delegate(object s, EventArgs a)
		{
			base.StartCoroutine(this.WaitCheckLicense());
		};
	}

	private IEnumerator WaitCheckLicense()
	{
		while (this._timeChecking < 2f || (this._penaltyInfo == null && this._penaltyInfoWarning == null))
		{
			this._timeChecking += 1f;
			yield return new WaitForSeconds(1f);
		}
		this._checkPenaltyPanel.Close();
		if (this._penaltyInfo != null)
		{
			MessageBox component = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.PenaltyPanel).GetComponent<MessageBox>();
			component.transform.Find("Icon").GetComponent<Text>().text = string.Format("\ue62b{0}", this._penaltyInfo.Value);
			component.OnPriority = this._isPriority;
			component.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			component.GetComponent<AlphaFade>().HideFinished += this.OnHideFinished;
		}
		else
		{
			MessageBox component2 = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.NonPenaltyPanel).GetComponent<MessageBox>();
			component2.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			component2.OnPriority = this._isPriority;
			component2.GetComponent<AlphaFade>().HideFinished += this.OnHideFinished;
		}
		yield break;
	}

	private void OnHideFinished(object sender, EventArgsAlphaFade eventArgsAlphaFade)
	{
		Object.Destroy(this);
	}

	private void Instance_OnLicensePenaltyWarning(LicenseBreakingInfo info)
	{
		PhotonConnectionFactory.Instance.OnLicensePenaltyWarning -= this.Instance_OnLicensePenaltyWarning;
		PhotonConnectionFactory.Instance.OnLicensePenalty -= this.Instance_OnLicensePenalty;
		this._penaltyInfoWarning = info;
	}

	private void Instance_OnLicensePenalty(LicenseBreakingInfo info)
	{
		PhotonConnectionFactory.Instance.OnLicensePenaltyWarning -= this.Instance_OnLicensePenaltyWarning;
		PhotonConnectionFactory.Instance.OnLicensePenalty -= this.Instance_OnLicensePenalty;
		this._penaltyInfo = info;
	}

	private float _timeChecking;

	private LicenseBreakingInfo _penaltyInfo;

	private LicenseBreakingInfo _penaltyInfoWarning;

	private MessageBox _checkPenaltyPanel;

	private bool _isPriority;
}
