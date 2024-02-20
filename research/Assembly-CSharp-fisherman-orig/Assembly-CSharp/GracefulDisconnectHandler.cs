using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public class GracefulDisconnectHandler
{
	public static bool NoGracefulDisconnectNeeded { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action OnGracefulDisconnectComplete;

	public static void Disconnect()
	{
		if (GracefulDisconnectHandler.NoGracefulDisconnectNeeded)
		{
			GracefulDisconnectHandler.NoGracefulDisconnectNeeded = false;
			GracefulDisconnectHandler.OnDisconnect();
			return;
		}
		if (GracefulDisconnectHandler.isDisconnectInProgress)
		{
			return;
		}
		if (PhotonConnectionFactory.Instance == null)
		{
			GracefulDisconnectHandler.OnDisconnect();
			return;
		}
		GracefulDisconnectHandler.isDisconnectInProgress = true;
		if (PhotonConnectionFactory.Instance.IsConnectedToGameServer)
		{
			PhotonConnectionFactory.Instance.OnPreviewQuitComplete += GracefulDisconnectHandler.PhotonServer_OnGracefulDisconnectComplete;
			PhotonConnectionFactory.Instance.OnPreviewQuitFailed += GracefulDisconnectHandler.PhotonServer_OnGracefulDisconnectFailed;
			PhotonConnectionFactory.Instance.GracefulDisconnect();
		}
		else
		{
			GracefulDisconnectHandler.PhotonServer_OnGracefulDisconnectComplete();
		}
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
	}

	private static void PhotonServer_OnGracefulDisconnectComplete()
	{
		GracefulDisconnectHandler.OnDisconnect();
	}

	private static void PhotonServer_OnGracefulDisconnectFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
		GracefulDisconnectHandler.OnDisconnect();
	}

	private static void OnDisconnect()
	{
		GracefulDisconnectHandler.isDisconnectInProgress = false;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		if (PhotonConnectionFactory.Instance != null)
		{
			PhotonConnectionFactory.Instance.Disconnect();
		}
		if (DisconnectServerAction.IsQuitDisconnect)
		{
			Process.GetCurrentProcess().Kill();
		}
		if (GracefulDisconnectHandler.OnGracefulDisconnectComplete != null)
		{
			GracefulDisconnectHandler.OnGracefulDisconnectComplete();
		}
	}

	private static bool isDisconnectInProgress;
}
