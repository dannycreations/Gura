using System;
using DeltaDNA;
using UnityEngine;

public class DeltaDNAIntergration : MonoBehaviour
{
	private void StartDNA()
	{
	}

	private static void DevelopmentConfiguration()
	{
		Singleton<DDNA>.Instance.SetLoggingLevel(global::DeltaDNA.Logger.Level.DEBUG);
		Singleton<DDNA>.Instance.ClientVersion = LaunchInit.CurrentVersion;
		Singleton<DDNA>.Instance.StartSDK("53990831298837973198512227514648", "http://collect8982fshng.deltadna.net/collect/api", "http://engage8982fshng.deltadna.net", PhotonConnectionFactory.Instance.Profile.UserId.ToString());
	}

	private static void ProductionConfiguration()
	{
		Singleton<DDNA>.Instance.SetLoggingLevel(global::DeltaDNA.Logger.Level.DEBUG);
		Singleton<DDNA>.Instance.ClientVersion = LaunchInit.CurrentVersion;
		Singleton<DDNA>.Instance.StartSDK("53990838934010886551401534214648", "http://collect8982fshng.deltadna.net/collect/api", "http://engage8982fshng.deltadna.net", PhotonConnectionFactory.Instance.Profile.UserId.ToString());
	}

	private bool _optionsInited;

	private bool _sdkInited;

	private static DeltaDNAIntergration _instance;
}
