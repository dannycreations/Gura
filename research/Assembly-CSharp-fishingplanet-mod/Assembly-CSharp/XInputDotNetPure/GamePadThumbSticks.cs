using System;
using UnityEngine;

namespace XInputDotNetPure
{
	public struct GamePadThumbSticks
	{
		internal GamePadThumbSticks(GamePadThumbSticks.StickValue left, GamePadThumbSticks.StickValue right)
		{
			this.left = left;
			this.right = right;
		}

		public GamePadThumbSticks.StickValue Left
		{
			get
			{
				return this.left;
			}
		}

		public GamePadThumbSticks.StickValue Right
		{
			get
			{
				return this.right;
			}
		}

		private GamePadThumbSticks.StickValue left;

		private GamePadThumbSticks.StickValue right;

		public struct StickValue
		{
			internal StickValue(float x, float y)
			{
				this.vector = new Vector2(x, y);
			}

			public float X
			{
				get
				{
					return this.vector.x;
				}
			}

			public float Y
			{
				get
				{
					return this.vector.y;
				}
			}

			public Vector2 Vector
			{
				get
				{
					return this.vector;
				}
			}

			private Vector2 vector;
		}
	}
}
