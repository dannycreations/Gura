using System;
using UnityEngine;

namespace ArtMode
{
	[Serializable]
	public class SceneInitPosition
	{
		public string sceneName;

		public Vector3 initialPos;

		public Quaternion initialRotation;
	}
}
