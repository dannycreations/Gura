using System;

namespace FSMAnimator
{
	public interface IAnimationSequence<T> where T : struct, IConvertible
	{
		T[] Properties { get; }

		void Update(float[] properties);

		void Stop();
	}
}
