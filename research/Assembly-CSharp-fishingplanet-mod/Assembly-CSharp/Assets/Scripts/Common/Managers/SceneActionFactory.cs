using System;
using Assets.Scripts.Common.Managers.SceneActions;

namespace Assets.Scripts.Common.Managers
{
	public static class SceneActionFactory
	{
		public static ISceneAction GetInstance(ScenesList scene)
		{
			switch (scene)
			{
			case ScenesList.Empty:
				return new EmptySceneAction();
			case ScenesList.Starting:
				return new StartingSceneAction();
			case ScenesList.Logged:
				return new LoggedSceneAction();
			case ScenesList.NotLogged:
				return new NotLoggedSceneAction();
			case ScenesList.Registration:
				return new RegisterSceneAction();
			case ScenesList.Login:
				return new LoginSceneAction();
			case ScenesList.Loading:
				return new LoadingSceneAction();
			case ScenesList.GameMenu:
				return new GameMenuAction();
			case ScenesList.GlobalMap:
				return new GlobalMapSceneAction();
			case ScenesList.Pond:
				return new PondSceneAction();
			}
			return null;
		}
	}
}
