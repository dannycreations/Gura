using System;
using System.Collections.Generic;
using UnityEngine;

public class EventDecoratorChangeWaterColor : MonoBehaviour
{
	private void Start()
	{
		if (EventsController.CurrentEvent != null && EventsController.CurrentEvent.EventId == this._eventId)
		{
			foreach (EventDecoratorChangeWaterColor.ShaderParams shaderParams in this._paramsForChanging)
			{
				this._fishWaterBase.sharedMaterial.SetColor(shaderParams.Name, shaderParams.Value);
			}
		}
	}

	[SerializeField]
	private int _eventId;

	[SerializeField]
	private List<EventDecoratorChangeWaterColor.ShaderParams> _paramsForChanging = new List<EventDecoratorChangeWaterColor.ShaderParams>();

	[SerializeField]
	private FishWaterBase _fishWaterBase;

	[Serializable]
	public class ShaderParams
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public Color Value;
	}
}
