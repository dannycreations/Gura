using System;

namespace FSMAnimator
{
	public abstract class BlendTree<T> where T : struct, IConvertible
	{
		public BlendTree(string name, float blendTime)
		{
			this.Name = name;
			this.BlendTime = blendTime;
		}

		public BlendTree<T>.UpdateResult[] LastResult
		{
			get
			{
				return this._lastResult;
			}
		}

		public abstract string[] Clips { get; }

		public T[] Properties { get; protected set; }

		public float BlendTime { get; protected set; }

		public string Name { get; protected set; }

		public abstract void Update(float[] properties);

		protected BlendTree<T>.UpdateResult[] _lastResult;

		public class UpdateResult
		{
			public UpdateResult(string clip, float speed)
			{
				this.Clip = clip;
				this.Speed = speed;
			}

			public readonly string Clip;

			public readonly float Speed;

			public float Weight;
		}
	}
}
