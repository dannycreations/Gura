using System;

namespace Assets.Scripts._3D.Game.MissionObjects
{
	[Serializable]
	public class ActionsSettings
	{
		public RotationActions[] Rotations;

		public MovementActions[] Movements;

		public ParticleActions[] Particles;

		public SoundActions[] Sounds;
	}
}
