using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExceptionHandler : MonoBehaviour
{
	private void Awake()
	{
		Application.RegisterLogCallback(new Application.LogCallback(this.HandleException));
	}

	private void HandleException(string condition, string stackTrace, LogType type)
	{
		if (type == 4)
		{
			string combinedMessage = condition + Environment.NewLine + stackTrace;
			if (ExceptionHandler.skipExceptionRules.Any((string[] r) => r.All((string s) => combinedMessage.Contains(s))))
			{
				return;
			}
			if (PhotonConnectionFactory.Instance == null || !PhotonConnectionFactory.Instance.IsAuthenticated)
			{
				return;
			}
			if (this.loggedExceptions.Count >= 20)
			{
				this.loggedExceptions.Clear();
			}
			DateTime dateTime;
			if (!this.loggedExceptions.TryGetValue(combinedMessage, out dateTime) || DateTime.UtcNow.Subtract(dateTime).TotalSeconds > 60.0)
			{
				PhotonConnectionFactory.Instance.PinError(condition, stackTrace);
				this.loggedExceptions[combinedMessage] = DateTime.UtcNow;
			}
		}
	}

	private readonly Dictionary<string, DateTime> loggedExceptions = new Dictionary<string, DateTime>();

	private static readonly List<string[]> skipExceptionRules = new List<string[]> { new string[] { "ArgumentOutOfRangeException", "InventorySRIA.InventorySRIA.CreateViewsHolder" } };
}
