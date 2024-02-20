using System;

public class DebugLoader
{
	public static void GoToLobby()
	{
		LogHelper.Log("Back to lobby {0}", new object[] { ++DebugLoader._lobbyCounter });
		StaticUserData.CurrentLocation = null;
		StaticUserData.CurrentPond = null;
		GameFactory.WaterFlow = null;
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoLobby, null, null);
	}

	private static int _lobbyCounter;
}
