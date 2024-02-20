using System;
using UnityEngine;

namespace TPM
{
	[Serializable]
	public class PantsRecord
	{
		public Pants pants;

		[Tooltip("You need to fill models for each boots type or default model will be used")]
		public PantsModelLink[] models;

		public GameObject model;

		public string modelPath;
	}
}
