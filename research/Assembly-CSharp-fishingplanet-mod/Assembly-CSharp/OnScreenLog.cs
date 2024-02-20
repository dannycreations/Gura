using System;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenLog : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		this.frameCount++;
	}

	private void OnGUI()
	{
	}

	public static void Add(string msg)
	{
		string text = msg.Replace("\r", " ");
		text = text.Replace("\n", " ");
		Console.WriteLine("[APP] " + text);
		OnScreenLog.log.Add(text);
		OnScreenLog.msgCount++;
		if (OnScreenLog.msgCount > OnScreenLog.maxLines)
		{
			OnScreenLog.log.RemoveAt(0);
		}
	}

	private static int msgCount = 0;

	private static List<string> log = new List<string>();

	private static int maxLines = 16;

	private static int fontSize = 24;

	private int frameCount;
}
