using System;
using ObjectModel;

public static class UIStatsCollector
{
	public static void ChangeGameScreen(GameScreenType gameScreen, GameScreenTabType gameScreenTab = GameScreenTabType.Undefined, int? categoryId = null, int? itemId = null, int[] childCategoryIds = null, string categoryElementId = null, string[] categoryElementsPath = null)
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		ScreenManager.Instance.SetGameScreen(gameScreen);
		if (PhotonConnectionFactory.Instance != null)
		{
			PhotonConnectionFactory.Instance.ChangeGameScreen(gameScreen, gameScreenTab, categoryId, itemId, childCategoryIds, categoryElementId, categoryElementsPath);
		}
	}

	private static void Instance_ChangeGameScreenFailed(Failure failure)
	{
		LogHelper.Error(string.Format("{0}:ChangeGameScreen - Failed ErrorCode:{1} ErrorMessage:{2}", UIStatsCollector.LOG_PREFIX, failure.ErrorCode, failure.ErrorMessage), new object[0]);
		UIStatsCollector.Unsubscribe();
	}

	private static void Instance_GameScreenChanged(GameScreenType type, GameScreenTabType tab)
	{
		LogHelper.Log(string.Format("{0}:GameScreenChanged - OK. Screen: {1}, Tab: {2}", UIStatsCollector.LOG_PREFIX, type, tab));
		UIStatsCollector.Unsubscribe();
	}

	private static void Unsubscribe()
	{
		PhotonConnectionFactory.Instance.GameScreenChanged -= UIStatsCollector.Instance_GameScreenChanged;
		PhotonConnectionFactory.Instance.ChangeGameScreenFailed -= UIStatsCollector.Instance_ChangeGameScreenFailed;
	}

	private static string LOG_PREFIX = "_UIStatsCollector";
}
