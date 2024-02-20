using System;
using CodeStage.AdvancedFPSCounter;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AFPSLogger : MonoBehaviour
{
	private void Start()
	{
		AFPSLogger.LogMemoryUsage(string.Format("Scene {0} loaded", SceneManager.GetActiveScene().name));
	}

	private void OnDestroy()
	{
		AFPSLogger.LogMemoryUsage(string.Format("Scene {0} unloading", SceneManager.GetActiveScene().name));
	}

	public static void LogMemoryUsage(string title)
	{
		if (AFPSCounter.Instance != null)
		{
			LogHelper.Log("AFPSLogger({0}): Memory usage. Total: {1}Mb, GFX: {2}Mb", new object[]
			{
				title,
				(float)AFPSCounter.Instance.memoryCounter.LastTotalValue / 1048576f,
				(float)AFPSCounter.Instance.memoryCounter.LastGfxValue / 1048576f
			});
		}
	}

	public static void LogFps(string title)
	{
		if (AFPSCounter.Instance != null)
		{
			LogHelper.Log("AFPSLogger({0}): FPS. Average: {1}, Min: {2}, Max: {3}, Low values: {4}", new object[]
			{
				title,
				AFPSCounter.Instance.fpsCounter.LastAverageValue,
				AFPSCounter.Instance.fpsCounter.LastMinimumValue,
				AFPSCounter.Instance.fpsCounter.LastMaximumValue,
				AFPSCounter.Instance.fpsCounter.LowValues
			});
		}
	}
}
