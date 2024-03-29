﻿using System;

namespace ExitGames.Client.Photon
{
	public class GameProperties
	{
		public const byte MaxPlayers = 255;

		public const byte IsVisible = 254;

		public const byte IsOpen = 253;

		public const byte PlayerCount = 252;

		public const byte Removed = 251;

		public const byte PropsListedInLobby = 250;

		public const byte CleanupCacheOnLeave = 249;
	}
}
