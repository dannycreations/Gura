using System;
using System.Diagnostics;
using UnityEngine.EventSystems;

public class GracefulDisconnectHandler
{
	public static void Disconnect()
	{
		if (GracefulDisconnectHandler.isDisconnectInProgress)
		{
			return;
		}
		if (PhotonConnectionFactory.Instance == null)
		{
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
		GracefulDisconnectHandler.OnDisconnect();
	}

	private static void OnDisconnect()
	{
		GracefulDisconnectHandler.isDisconnectInProgress = false;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		PhotonConnectionFactory.Instance.Disconnect();
		if (DisconnectServerAction.IsQuitDisconnect)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	private static bool isDisconnectInProgress;
}
