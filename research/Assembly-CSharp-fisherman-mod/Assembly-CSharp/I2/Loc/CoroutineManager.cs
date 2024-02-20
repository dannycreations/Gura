using System;
using UnityEngine;

namespace I2.Loc
{
	public class CoroutineManager : MonoBehaviour
	{
		public static CoroutineManager pInstance
		{
			get
			{
				if (CoroutineManager.mInstance == null)
				{
					GameObject gameObject = new GameObject("GoogleTranslation");
					gameObject.hideFlags |= 61;
					CoroutineManager.mInstance = gameObject.AddComponent<CoroutineManager>();
				}
				return CoroutineManager.mInstance;
			}
		}

		private static CoroutineManager mInstance;
	}
}
