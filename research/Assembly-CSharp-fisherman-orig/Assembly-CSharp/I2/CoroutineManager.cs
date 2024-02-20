using System;
using System.Collections;
using UnityEngine;

namespace I2
{
	public class CoroutineManager : MonoBehaviour
	{
		public static Coroutine Start(IEnumerator coroutine)
		{
			if (CoroutineManager.mInstance == null)
			{
				GameObject gameObject = new GameObject("_Coroutiner");
				gameObject.hideFlags |= 61;
				CoroutineManager.mInstance = gameObject.AddComponent<CoroutineManager>();
				if (Application.isPlaying)
				{
					Object.DontDestroyOnLoad(gameObject);
				}
			}
			return CoroutineManager.mInstance.StartCoroutine(coroutine);
		}

		private static CoroutineManager mInstance;
	}
}
