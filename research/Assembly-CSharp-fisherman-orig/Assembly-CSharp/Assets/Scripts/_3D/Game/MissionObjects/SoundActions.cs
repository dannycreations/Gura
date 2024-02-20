using System;
using UnityEngine;

namespace Assets.Scripts._3D.Game.MissionObjects
{
	[Serializable]
	public class SoundActions
	{
		public AudioClip Clip;

		public Vector3 Position;

		public Transform Obj;

		[Tooltip("Volume value between 0 and 1")]
		public float Volume;

		public bool IsPlayOnPlayer;
	}
}
