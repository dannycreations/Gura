using System;

namespace FPWorldStreamer
{
	public interface ICell
	{
		float XSize { get; }

		float ZSize { get; }

		string LayerName { get; }
	}
}
