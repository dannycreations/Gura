using System;

namespace frame8.Logic.Misc.Visual.UI
{
	public interface IScrollRectProxy
	{
		event Action<float> ScrollPositionChanged;

		void SetNormalizedPosition(float normalizedPosition);

		float GetNormalizedPosition();

		float GetContentSize();
	}
}
