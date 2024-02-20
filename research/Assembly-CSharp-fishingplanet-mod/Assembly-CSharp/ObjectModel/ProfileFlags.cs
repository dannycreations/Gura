using System;

namespace ObjectModel
{
	public static class ProfileFlags
	{
		public static bool HasFlag(int flags, int flag)
		{
			return (flags & flag) != 0;
		}

		public static int AddFlag(int flags, int flag)
		{
			return flags | flag;
		}

		public static bool HasFlag(ProfileFlag flag)
		{
			return (PhotonConnectionFactory.Instance.Profile.Flags & (1 << (int)((byte)flag))) != 0;
		}

		public const int DebugSystem = 1;

		public const int DebugPhysics = 2;

		public const int DebugGameLogic = 4;

		public const int DebugProductDelivery = 8;

		public const int DebugMissions = 16;

		public const int DebugSystemIndex = 1;

		public const int DebugPhysicsIndex = 2;

		public const int DebugGameLogicIndex = 3;

		public const int DebugProductDeliveryIndex = 4;

		public const int DebugMissionsIndex = 5;
	}
}
