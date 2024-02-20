using System;
using System.Collections.Generic;
using ObjectModel;

public class TournamentComparer : IEqualityComparer<Tournament>
{
	public bool Equals(Tournament x, Tournament y)
	{
		return x != null && y != null && x.TournamentId == y.TournamentId && x.IsCanceled == y.IsCanceled && x.IsRegistered == y.IsRegistered && x.IsApproved == y.IsApproved && x.IsDone == y.IsDone && x.IsActive == y.IsActive;
	}

	public int GetHashCode(Tournament obj)
	{
		return obj.TournamentId ^ obj.IsCanceled.GetHashCode() ^ obj.IsRegistered.GetHashCode() ^ obj.IsApproved.GetHashCode() ^ obj.IsDone.GetHashCode() ^ obj.IsActive.GetHashCode();
	}
}
