using System;
using ExitGames.Client.Photon;
using UnityEngine;

internal static class TeamExtensions
{
	public static PunTeams.Team GetTeam(this PhotonPlayer player)
	{
		object obj;
		if (player.customProperties.TryGetValue("team", out obj))
		{
			return (PunTeams.Team)obj;
		}
		return PunTeams.Team.none;
	}

	public static void SetTeam(this PhotonPlayer player, PunTeams.Team team)
	{
		if (!PhotonNetwork.connectedAndReady)
		{
			Debug.LogWarning("JoinTeam was called in state: " + PhotonNetwork.connectionStateDetailed + ". Not connectedAndReady.");
		}
		PunTeams.Team team2 = PhotonNetwork.player.GetTeam();
		if (team2 != team)
		{
			PhotonPlayer player2 = PhotonNetwork.player;
			Hashtable hashtable = new Hashtable();
			hashtable.Add("team", (byte)team);
			player2.SetCustomProperties(hashtable);
		}
	}
}
