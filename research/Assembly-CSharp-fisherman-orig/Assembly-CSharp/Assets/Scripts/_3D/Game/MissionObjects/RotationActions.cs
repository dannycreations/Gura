using System;
using UnityEngine;

namespace Assets.Scripts._3D.Game.MissionObjects
{
	[Serializable]
	public class RotationActions
	{
		public Transform Obj;

		public Vector3 Rotation;

		public float Speed;

		public int Loops;
	}
}
