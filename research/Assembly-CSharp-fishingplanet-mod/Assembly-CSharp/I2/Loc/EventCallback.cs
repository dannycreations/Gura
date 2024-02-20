using System;
using UnityEngine;

namespace I2.Loc
{
	[Serializable]
	public class EventCallback
	{
		public void Execute(Object Sender = null)
		{
			if (this.Target && Application.isPlaying)
			{
				this.Target.gameObject.SendMessage(this.MethodName, Sender, 1);
			}
		}

		public bool HasCallback()
		{
			return this.Target != null && !string.IsNullOrEmpty(this.MethodName);
		}

		public MonoBehaviour Target;

		public string MethodName = string.Empty;
	}
}
