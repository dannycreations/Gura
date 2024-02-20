using System;
using UnityEngine;

[RequireComponent(typeof(AlphaFade))]
public class InfoMessage : MonoBehaviour
{
	public float DelayTime
	{
		get
		{
			return this.delayTime;
		}
	}

	public InfoMessageTypes MessageType
	{
		get
		{
			return this._messageType;
		}
		set
		{
			this._messageType = value;
		}
	}

	[SerializeField]
	private float delayTime;

	public bool CloseByAnyClick = true;

	public bool CloseBySpace = true;

	public bool ShowOnGlobalMap;

	public bool ShowInTutorial;

	public float ShowDelayTime;

	public Action ExecuteAfterShow;

	protected InfoMessageTypes _messageType;
}
