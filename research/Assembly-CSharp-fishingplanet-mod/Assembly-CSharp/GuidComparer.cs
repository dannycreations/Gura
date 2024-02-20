using System;
using System.Collections.Generic;

public class GuidComparer : IEqualityComparer<Guid>
{
	public int GetHashCode(Guid obj)
	{
		return obj.GetHashCode();
	}

	public bool Equals(Guid x, Guid y)
	{
		return x.Equals(y);
	}
}
