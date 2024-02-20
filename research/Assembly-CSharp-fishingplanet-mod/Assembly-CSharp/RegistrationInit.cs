using System;
using UnityEngine;

public class RegistrationInit : MonoBehaviour
{
	private void Start()
	{
		if (StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamOsX || StaticUserData.ClientType == ClientTypes.SteamLinux || StaticUserData.ClientType == ClientTypes.MailRuWindows || StaticUserData.ClientType == ClientTypes.TencentWindows)
		{
			GameObject steamRegistartionPanel = this.SteamRegistartionPanel;
			steamRegistartionPanel.SetActive(true);
			this.PCRegistartionPanel.SetActive(false);
			this.ConsoleRegistartionPanel.SetActive(false);
			base.GetComponent<ActivityState>().ActionObject = steamRegistartionPanel;
		}
		else
		{
			this.PCRegistartionPanel.SetActive(true);
			this.SteamRegistartionPanel.SetActive(false);
			this.SteamRetailRegistartionPanel.SetActive(false);
			this.ConsoleRegistartionPanel.SetActive(false);
			base.GetComponent<ActivityState>().ActionObject = this.PCRegistartionPanel;
		}
	}

	public GameObject PCRegistartionPanel;

	public GameObject ConsoleRegistartionPanel;

	public GameObject SteamRegistartionPanel;

	public GameObject SteamRetailRegistartionPanel;

	public LoadCustomizationScene LoadCustomizationScene;

	public EULAScreenInit EulaScreenInit;
}
