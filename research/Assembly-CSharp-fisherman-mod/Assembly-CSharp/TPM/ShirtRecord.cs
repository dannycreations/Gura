using System;
using UnityEngine;

namespace TPM
{
	[Serializable]
	public class ShirtRecord
	{
		public Shirts shirts;

		public GameObject mModel;

		public HandLength mHandLength;

		public GameObject fModel;

		public HandLength fHandLength;

		public string mModelPath;

		public string fModelPath;
	}
}
