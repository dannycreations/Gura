using System;
using System.Collections.Generic;
using UnityEngine;

namespace SingletoneObjects
{
	public class CreateSingletoneObjects : MonoBehaviour
	{
		private void Awake()
		{
			if (CreateSingletoneObjects.Instance != null)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			Object.DontDestroyOnLoad(base.gameObject);
			CreateSingletoneObjects.Instance = this;
			this.Init();
		}

		public void Refresh()
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Object.Destroy(base.transform.GetChild(i).gameObject);
			}
			this.Init();
		}

		private void Init()
		{
			for (int i = 0; i < this.objects.Count; i++)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.objects[i]);
				gameObject.transform.parent = base.transform;
			}
		}

		public List<GameObject> objects;

		public static CreateSingletoneObjects Instance;
	}
}
