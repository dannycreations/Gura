using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InputChecking : MonoBehaviour
{
	protected virtual void Start()
	{
		if (this.inputField == null)
		{
			this.inputField = this.Label.GetComponent<InputField>();
		}
	}

	protected virtual void OnDestroy()
	{
	}

	public virtual void OnChange()
	{
		this.inputField.text = this.inputField.text.ReplaceNonLatin();
		string text = this.inputField.text;
		if (this.Checks(text))
		{
			if (this.CheckIn != null)
			{
				GUITools.SetActive(this.CheckIn, true);
			}
			if (this.CheckOut != null)
			{
				GUITools.SetActive(this.CheckOut, false);
			}
			if (this.ImageObject != null)
			{
				this.ImageObject.color = this.CorrectColor;
			}
			this.isCorrect = true;
		}
		else
		{
			if (this.CheckIn != null)
			{
				GUITools.SetActive(this.CheckIn, false);
			}
			if (this.CheckOut != null)
			{
				GUITools.SetActive(this.CheckOut, true);
			}
			if (this.ImageObject != null)
			{
				this.ImageObject.color = this.IncorrectColor;
			}
			this.isCorrect = false;
		}
	}

	public virtual void ClearChecks()
	{
		if (this.CheckIn != null)
		{
			GUITools.SetActive(this.CheckIn, false);
		}
		if (this.CheckOut != null)
		{
			GUITools.SetActive(this.CheckOut, false);
		}
		if (this.ImageObject != null)
		{
			this.ImageObject.color = this.DefaultColor;
		}
	}

	protected abstract bool Checks(string text);

	[HideInInspector]
	public bool isCorrect;

	public GameObject CheckIn;

	public GameObject CheckOut;

	public Text Label;

	public InputField inputField;

	public Color IncorrectColor;

	public Color CorrectColor;

	public Color DefaultColor;

	public Image ImageObject;
}
