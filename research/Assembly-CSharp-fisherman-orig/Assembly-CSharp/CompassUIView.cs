using System;
using System.Diagnostics;

public class CompassUIView : ICompassView
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<float> OnNorthDegree = delegate
	{
	};

	public void SetNorthDegree(float degree)
	{
		this.OnNorthDegree(degree);
	}
}
