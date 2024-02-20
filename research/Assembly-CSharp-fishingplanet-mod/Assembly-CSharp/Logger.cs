using System;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
	private void OnEnable()
	{
		Application.RegisterLogCallback(new Application.LogCallback(this.HandleLog));
	}

	private void OnDisable()
	{
		Application.RegisterLogCallback(null);
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, (float)(Screen.height - 140), (float)Screen.width, 140f));
		foreach (string text in Logger.queue)
		{
			GUILayout.Label(text, new GUILayoutOption[0]);
		}
		GUILayout.EndArea();
	}

	private void HandleLog(string message, string stackTrace, LogType type)
	{
		Logger.queue.Enqueue(Time.time + " - " + message);
		if (Logger.queue.Count > 5)
		{
			Logger.queue.Dequeue();
		}
	}

	private static Queue<string> queue = new Queue<string>(6);
}
