using System;
using UnityEngine;

public class OptionsPointer : MonoBehaviour
{
	public GameObject target;

	public OptionsPointer.PointerClass PointerSettings;

	public OptionsPointer.highlightobject HighlightTarget;

	public OptionsPointer.SendMessageClass[] OptionSendMessage;

	public OptionsPointer.Actors ActorsSpeech;

	public OptionsPointer.TimeClass ByTime;

	public OptionsPointer.TriggerClass ByTrigger;

	[Serializable]
	public class PointerClass
	{
		public OptionsPointer.typePointer selectType;

		[Header("Wait Time --> Time before arrow appears over the Target")]
		[Range(0f, 5f)]
		public float waitTime = 1f;

		public float distanceTargetY;

		public float distanceTargetX;

		public float angle;

		[Range(-1f, 5f)]
		public int numberLoops = -1;

		[Range(0f, 2f)]
		public float bounceFreq = 0.4f;

		[Range(0f, 5f)]
		public float movementY = 3f;

		[Range(0f, 5f)]
		public float movementX;

		public GameObject StartEffect;

		public GameObject EndEffect;

		public GameObject deactivateGameObjectAtStart;

		public string EndFuntionName;
	}

	[Serializable]
	public class highlightobject
	{
		public bool highlight = true;

		public Color colorShader = Color.red;

		public GameObject highlightEffect;
	}

	[Serializable]
	public class SendMessageClass
	{
		public GameObject ObjectSendMessage;

		public string nameFunction;

		public OptionsPointer.timming Activate;
	}

	[Serializable]
	public class Actors
	{
		public Sprite actor;

		public Vector2 positionActor = new Vector2(-150f, 50f);

		public float widthActor = -1f;

		public float heightActor = -1f;

		public Sprite bubble;

		public Vector2 positionBubble;

		public float widthBubble = -1f;

		public float heightBubble = -1f;

		public string text;

		public Vector2 positionText;

		public bool textBestFit = true;

		[Range(0f, 5f)]
		public float timeTextToStart = 1f;

		public bool tweenText = true;

		public float timeBetweenCharacters = 0.05f;

		public bool fadeIn = true;

		[Range(0f, 5f)]
		public float fadeTime = 1f;
	}

	[Serializable]
	public class TimeClass
	{
		public bool byTime;

		[Range(-1f, 10f)]
		public float duration = -1f;

		public bool OnlyDeactivate;
	}

	[Serializable]
	public class TriggerClass
	{
		public bool byTrigger = true;
	}

	public enum typePointer
	{
		World3D,
		GUI2D
	}

	public enum timming
	{
		OnStart,
		OnEnd
	}

	public enum typesParam
	{
		None,
		PBoolean,
		PInteger,
		PString,
		PFloat
	}
}
