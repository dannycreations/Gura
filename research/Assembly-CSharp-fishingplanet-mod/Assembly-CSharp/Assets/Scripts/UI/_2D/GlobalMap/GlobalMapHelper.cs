using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;

namespace Assets.Scripts.UI._2D.GlobalMap
{
	public class GlobalMapHelper
	{
		public static DateTime? GetActivationTime(int pondId)
		{
			return (!GlobalMapHelper.ActivationTime.ContainsKey((Ponds)pondId)) ? null : new DateTime?(GlobalMapHelper.ActivationTime[(Ponds)pondId]);
		}

		public static bool IsActive(Pond pond)
		{
			DateTime? activationTime = GlobalMapHelper.GetActivationTime(pond.PondId);
			if (activationTime != null && activationTime > TimeHelper.UtcTime())
			{
				return PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.LevelLockRemovals != null && PhotonConnectionFactory.Instance.Profile.LevelLockRemovals.Any((LevelLockRemoval p) => p.Type == LevelLockRemovalType.Product && p.Ponds.ToList<int>().Contains(pond.PondId) && p.EndDate != null && p.EndDate > TimeHelper.UtcTime());
			}
			return pond.IsActive;
		}

		private static readonly Dictionary<Ponds, DateTime> ActivationTime = new Dictionary<Ponds, DateTime> { 
		{
			Ponds.AhtubaVolga,
			new DateTime(2018, 1, 3, 6, 0, 0)
		} };
	}
}
