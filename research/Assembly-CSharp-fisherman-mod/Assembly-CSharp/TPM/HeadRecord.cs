using System;
using UnityEngine;

namespace TPM
{
	[Serializable]
	public class HeadRecord
	{
		public Faces face;

		public Gender gender;

		public GameObject headModel;

		public string headModelPath;
	}
}
