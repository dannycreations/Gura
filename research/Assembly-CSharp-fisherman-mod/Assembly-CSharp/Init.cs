using System;
using System.IO;
using UnityEngine;

public class Init : MonoBehaviour
{
	internal void Start()
	{
		GameState.State = GameStates.StartSceneState;
		Time.timeScale = 1f;
		string text = Init.ApplicationPath();
		ConfigUtil configUtil = new ConfigUtil();
		configUtil.SetServerConnection(text + "/QAConfig.cfg");
		PhotonConnectionFactory.Instance.MasterServerAddress = StaticUserData.ServerConnectionString;
		PhotonConnectionFactory.Instance.Protocol = StaticUserData.ServerConnectionProtocol;
		PhotonConnectionFactory.Instance.AppName = "Master";
		SceneController.CallAction(ScenesList.Empty, SceneStatuses.Empty, this, null);
		Caching.ClearCache();
		CustomPlayerPrefs.DeleteKey("SentSystemInfo2");
	}

	private static string ApplicationPath()
	{
		string text = Application.dataPath;
		if (Application.platform == 1)
		{
			text = Directory.GetParent(text).Parent.FullName;
		}
		else if (Application.platform == 2 || Application.platform == 13 || Application.platform == 27 || Application.platform == 25)
		{
			text += "/../";
		}
		return text;
	}
}
