using System;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
	private void Awake()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Pointers");
		this.pointers = new OptionsPointer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			this.pointers[i] = array[i].GetComponent<OptionsPointer>();
		}
		for (int j = 0; j < this.pointers.Length; j++)
		{
			for (int k = 0; k < this.pointers.Length; k++)
			{
				if (string.Compare(this.pointers[k].gameObject.name, this.pointers[j].gameObject.name) == 1)
				{
					OptionsPointer optionsPointer = this.pointers[j];
					this.pointers[j] = this.pointers[k];
					this.pointers[k] = optionsPointer;
				}
			}
		}
	}

	private void Start()
	{
		if (this.pointers.Length == 0)
		{
			Debug.LogWarning("(GTG) The list of 'Pointers' in TutorialManager is empty.\nFIX: TutorialManager Prefab has a default pointer. Select Tutorial Manager and press 'Revert' on the inspector");
			TutorialManager.end = true;
		}
		else
		{
			Debug.Log("(GTG) The total size of 'Pointers' is " + this.pointers.Length + "\nTIP: To add more, expand TutorialManager & GroupPointers in [Hierarchy] & duplicate 'Pointers'. The pointers are sorted by name at runtime");
			this.shader2 = Shader.Find("Highlight/Outline");
			this.p = this.pointers[this.i];
			if (this.stringPointer = null)
			{
				this.stringPointer = GameObject.Find("TextPointers");
			}
			this.StartPointer();
		}
	}

	public Sprite AddSprite(Texture2D tex)
	{
		Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f), 128f);
		GameObject gameObject = new GameObject();
		gameObject.name = "temp";
		gameObject.AddComponent<SpriteRenderer>();
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		component.sprite = sprite;
		gameObject.SetActive(false);
		Object.Destroy(gameObject, 1f);
		return component.sprite;
	}

	private IEnumerator activatePointer()
	{
		yield return new WaitForSeconds(this.p.PointerSettings.waitTime);
		this.tweenPointer();
		yield break;
	}

	private IEnumerator destroyPointer()
	{
		yield return new WaitForSeconds(this.p.ByTime.duration + this.p.PointerSettings.waitTime);
		this.EndFunction();
		if (this.p != null)
		{
			if (this.p.ByTime.OnlyDeactivate)
			{
				this.p.gameObject.SetActive(false);
			}
			else
			{
				Object.Destroy(this.p.gameObject, 0f);
			}
		}
		if (!TutorialManager.end)
		{
			this.moveToNext();
		}
		yield break;
	}

	public void tweenPointer()
	{
		float num = this.p.PointerSettings.movementX;
		float num2 = this.p.PointerSettings.movementY;
		float num3 = this.p.PointerSettings.distanceTargetX;
		float num4 = this.p.PointerSettings.distanceTargetY;
		if (this.p.PointerSettings.selectType.ToString() == "GUI2D")
		{
			num *= 10f;
			num2 *= 10f;
			num3 *= 10f;
			num4 *= 10f;
			RectTransform component = this.p.GetComponent<RectTransform>();
			component.localScale *= 30f;
		}
		if (this.p.PointerSettings.deactivateGameObjectAtStart)
		{
			this.p.PointerSettings.deactivateGameObjectAtStart.SetActive(false);
		}
		Vector3 position = this.p.target.transform.position;
		this.p.transform.position = new Vector3(position.x - num3, position.y - num4);
		HOTween.To(this.p.transform, this.p.PointerSettings.bounceFreq, new TweenParms().Prop("position", new Vector3(num, num2, 0f), true).Loops(this.p.PointerSettings.numberLoops, 1).Ease(6)
			.OnComplete(new TweenDelegate.TweenCallback(this.EndFunction)));
		if (this.p.PointerSettings.selectType.ToString() == "World3D")
		{
			this.p.transform.LookAt(Camera.main.transform.position);
			this.p.transform.Rotate(this.p.transform.rotation.x, this.p.transform.rotation.y, this.p.PointerSettings.angle);
		}
	}

	public void moveToNext()
	{
		this.i++;
		if (this.i < this.pointers.Length)
		{
			this.p = this.pointers[this.i];
			this.StartPointer();
		}
		else
		{
			TutorialManager.end = true;
		}
	}

	public void StartPointer()
	{
		if (this.p.PointerSettings.selectType.ToString() == "GUI2D")
		{
			GameObject.Find("GroupPointers").GetComponent<Canvas>().renderMode = 0;
			TutorialManager.follow = false;
		}
		else
		{
			GameObject.Find("GroupPointers").GetComponent<Canvas>().renderMode = 2;
			TutorialManager.follow = true;
		}
		if (this.p.HighlightTarget.highlight && this.p.target.GetComponent<Renderer>())
		{
			this.shader1 = this.p.target.GetComponent<Renderer>().material.shader;
			this.p.target.GetComponent<Renderer>().material.shader = this.shader2;
			this.p.target.GetComponent<Renderer>().material.SetColor("_OutlineColor", this.p.HighlightTarget.colorShader);
			if (this.p.HighlightTarget.highlightEffect)
			{
				this.effect = Object.Instantiate<GameObject>(this.p.HighlightTarget.highlightEffect, this.p.target.transform.position, Quaternion.identity);
			}
		}
		if (this.p.PointerSettings.StartEffect != null)
		{
			Object.Instantiate<GameObject>(this.p.PointerSettings.StartEffect, this.p.target.transform.position, Quaternion.identity);
		}
		for (int i = 0; i < this.p.OptionSendMessage.Length; i++)
		{
			if (((this.p.OptionSendMessage[i].ObjectSendMessage != null) & !string.IsNullOrEmpty(this.p.OptionSendMessage[i].nameFunction)) && this.p.OptionSendMessage[i].Activate.ToString() == "OnStart")
			{
				this.p.OptionSendMessage[i].ObjectSendMessage.SendMessage(this.p.OptionSendMessage[i].nameFunction);
			}
		}
		if (this.p.target == null)
		{
			Debug.LogWarning(string.Concat(new string[]
			{
				"(GTG) '",
				this.p.gameObject.name,
				"' has no target.\nFIX: Expand TutorialManager, GroupPointers if necessary, select '",
				this.p.gameObject.name,
				"' in the [Hierarchy]. Drag and Drop target to '",
				this.p.gameObject.name,
				"' in the [Inspector]"
			}));
			return;
		}
		if (!this.p.ByTime.byTime && !this.p.ByTrigger.byTrigger)
		{
			Debug.LogWarning(string.Concat(new string[]
			{
				"(GTG) '",
				this.p.gameObject.name,
				"' has no transition by time or trigger.\nFIX: Expand TutorialManager,GroupPointers if necessary, select '",
				this.p.gameObject.name,
				"' in the [Hierarchy]. Select 'ByTime' (& duration) and/or 'ByTrigger' in the [Inspector]"
			}));
			return;
		}
		this.actor = GameObject.Find("ActorPointers");
		this.bubble = GameObject.Find("BubblePointers");
		this.stringPointer = GameObject.Find("TextPointers");
		if (this.p.ByTrigger.byTrigger)
		{
			if (this.p.PointerSettings.selectType.ToString() == "World3D")
			{
				this.p.target.AddComponent<TapGesture>();
				this.p.target.AddComponent<HandleTouch>();
			}
			else
			{
				this.p.target.GetComponent<Button>().onClick.AddListener(new UnityAction(this.DeactivatePointer));
			}
		}
		this.actor.GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
		this.bubble.GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
		this.stringPointer.GetComponent<Text>().text = string.Empty;
		this.stringPointer.GetComponent<Text>().CrossFadeAlpha(1f, 0.5f, true);
		float num;
		if (this.p.ActorsSpeech.fadeIn)
		{
			num = this.p.ActorsSpeech.fadeTime;
		}
		else
		{
			num = 0f;
		}
		if (this.p.ByTime.byTime || this.p.ByTrigger.byTrigger)
		{
			base.StartCoroutine(this.activatePointer());
		}
		if (this.p.ByTime.byTime && this.p.ByTime.duration > -1f)
		{
			base.StartCoroutine("destroyPointer");
		}
		if (this.p.ActorsSpeech.positionActor != new Vector2(-1f, -1f))
		{
			this.actor.GetComponent<RectTransform>().anchoredPosition = this.p.ActorsSpeech.positionActor;
		}
		if (this.p.ActorsSpeech.positionBubble != new Vector2(-1f, -1f))
		{
			this.bubble.GetComponent<RectTransform>().anchoredPosition = this.p.ActorsSpeech.positionBubble;
		}
		if (this.p.ActorsSpeech.positionText != new Vector2(-1f, -1f))
		{
			this.stringPointer.GetComponent<RectTransform>().anchoredPosition = this.p.ActorsSpeech.positionText;
		}
		if (this.p.ActorsSpeech.actor == null)
		{
			this.actor.GetComponent<Image>().CrossFadeAlpha(0f, num, true);
		}
		else
		{
			this.actor.GetComponent<Image>().CrossFadeAlpha(1f, num, true);
			this.actor.GetComponent<Image>().sprite = this.AddSprite(this.p.ActorsSpeech.actor.texture);
			if (this.p.ActorsSpeech.heightActor != -1f)
			{
				float num2 = this.p.ActorsSpeech.heightActor;
			}
			if (this.p.ActorsSpeech.widthActor != -1f)
			{
				float num3 = this.p.ActorsSpeech.widthActor;
			}
		}
		if (this.p.ActorsSpeech.bubble == null)
		{
			this.bubble.GetComponent<Image>().CrossFadeAlpha(0f, num, true);
		}
		else
		{
			this.bubble.GetComponent<Image>().CrossFadeAlpha(1f, num, true);
			this.bubble.GetComponent<Image>().sprite = this.AddSprite(this.p.ActorsSpeech.bubble.texture);
			float num2;
			if (this.p.ActorsSpeech.heightBubble != -1f)
			{
				num2 = this.p.ActorsSpeech.heightBubble;
			}
			else
			{
				num2 = 200f;
			}
			float num3;
			if (this.p.ActorsSpeech.widthBubble != -1f)
			{
				num3 = this.p.ActorsSpeech.widthBubble;
			}
			else
			{
				num3 = 200f;
			}
			this.bubble.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, num2);
		}
		this.stringPointer.GetComponent<Text>().text = string.Empty;
		if (!string.IsNullOrEmpty(this.p.ActorsSpeech.text))
		{
			this.endText = false;
			base.StartCoroutine("tweenString");
		}
		else
		{
			Debug.Log("(GTG) The text of the '" + this.p.gameObject.name + "' is empty\nTIP: To add text, select the 'Pointer' in the [Hierarchy] and enter the text in [Inspector] Actors Speech - Text");
		}
	}

	private IEnumerator tweenString()
	{
		yield return new WaitForSeconds(this.p.ActorsSpeech.timeTextToStart);
		int length = this.p.ActorsSpeech.text.Length;
		int i = 0;
		this.stringPointer.GetComponent<Text>().text = string.Empty;
		if (!this.p.ActorsSpeech.tweenText)
		{
			this.stringPointer.GetComponent<Text>().text = this.p.ActorsSpeech.text;
		}
		else
		{
			while (i < length && !this.endText)
			{
				yield return new WaitForSeconds(this.p.ActorsSpeech.timeBetweenCharacters);
				if (i < this.p.ActorsSpeech.text.Length && !this.endText)
				{
					this.stringPointer.GetComponent<Text>().text = this.stringPointer.GetComponent<Text>().text + this.p.ActorsSpeech.text[i];
				}
				i++;
			}
		}
		yield break;
	}

	public void DeactivatePointer()
	{
		this.EndFunction();
		if (this.p != null)
		{
			this.p.gameObject.GetComponent<Image>().gameObject.SetActive(false);
			Object.Destroy(this.p.gameObject, 1f);
		}
		if (!TutorialManager.end)
		{
			this.moveToNext();
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void LateUpdate()
	{
		if (!TutorialManager.end)
		{
			if (Camera.main.GetComponent<SmoothLookAt>())
			{
				Camera.main.GetComponent<SmoothLookAt>().target = this.p.target;
			}
			if (this.p.ByTrigger.byTrigger && TutorialManager.touched)
			{
				this.DeactivatePointer();
			}
		}
	}

	public void EndFunction()
	{
		if (this.p.PointerSettings.EndEffect != null)
		{
			Object.Instantiate<GameObject>(this.p.PointerSettings.EndEffect, this.p.target.transform.position, Quaternion.identity);
		}
		for (int i = 0; i < this.p.OptionSendMessage.Length; i++)
		{
			if (((this.p.OptionSendMessage[i].ObjectSendMessage != null) & !string.IsNullOrEmpty(this.p.OptionSendMessage[i].nameFunction)) && this.p.OptionSendMessage[i].Activate.ToString() == "OnEnd")
			{
				this.p.OptionSendMessage[i].ObjectSendMessage.SendMessage(this.p.OptionSendMessage[i].nameFunction);
			}
		}
		this.endText = true;
		TutorialManager.touched = false;
		if (this.p.HighlightTarget.highlight && this.p.target.GetComponent<Renderer>())
		{
			this.p.target.GetComponent<Renderer>().material.shader = this.shader1;
			if (this.effect)
			{
				Object.Destroy(this.effect);
			}
		}
		if (this.p.PointerSettings.selectType.ToString() == "World3D")
		{
			Object.Destroy(this.p.target.GetComponent<HandleTouch>());
			Object.Destroy(this.p.target.GetComponent<TapGesture>());
		}
		else
		{
			this.p.target.GetComponent<Button>().onClick.RemoveListener(new UnityAction(this.DeactivatePointer));
		}
		base.StopCoroutine("tweenString");
		base.StopCoroutine("destroyPointer");
		if (this.p.ActorsSpeech.actor != null)
		{
			GameObject.Find("ActorPointers").GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
		}
		if (this.p.ActorsSpeech.bubble != null)
		{
			GameObject.Find("BubblePointers").GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
		}
		this.stringPointer.GetComponent<Text>().text = string.Empty;
		this.stringPointer.GetComponent<Text>().CrossFadeAlpha(0f, 0f, true);
		if (!string.IsNullOrEmpty(this.p.PointerSettings.EndFuntionName))
		{
			base.Invoke(this.p.PointerSettings.EndFuntionName, 0f);
		}
		else
		{
			Debug.Log("(GTG) No custom function at the end.\nTIP: To add an end custom funtion select the 'Pointer' in the [Hierarchy] and enter the name in [Inspector] Pointer Settings - End Funtion Name");
		}
	}

	public void exampleeEndFunction()
	{
		Debug.Log("(GTG) Example End Function");
	}

	[HideInInspector]
	public OptionsPointer[] pointers;

	private OptionsPointer p;

	private int i;

	[HideInInspector]
	public static bool end;

	[HideInInspector]
	public static bool follow = true;

	private Vector3 targetPos;

	private GameObject actor;

	private GameObject bubble;

	private GameObject stringPointer;

	private bool endText;

	public static bool touched;

	private Shader shader1;

	private Shader shader2;

	private GameObject effect;
}
