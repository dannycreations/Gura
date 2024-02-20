using System;
using UnityEngine;

namespace Assets.Scripts.Common.Managers
{
	public interface ISceneAction
	{
		void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null);
	}
}
