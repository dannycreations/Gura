using System;
using UnityEngine;
using UnityEngine.UI;

public class CheckUniqueUsername : MonoBehaviour
{
	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique += this.OnCheckUsernameIsUnique;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique -= this.OnCheckUsernameIsUnique;
	}

	public void OnChange(string name)
	{
		if (name != PhotonConnectionFactory.Instance.Profile.Name)
		{
			if (!InputCheckingName.rgx.IsMatch(name) || AbusiveWords.HasAbusiveWords(name))
			{
				this.IsCorrect = false;
				this.CorrectObject.SetActive(false);
				this.InCorrectObject.SetActive(true);
				return;
			}
			PhotonConnectionFactory.Instance.CheckUsernameIsUnique(name);
		}
		else
		{
			this.IsCorrect = true;
			this.CorrectObject.SetActive(false);
			this.InCorrectObject.SetActive(false);
		}
	}

	internal void Update()
	{
		if (this._currentName != base.GetComponent<Text>().text)
		{
			this._currentName = base.GetComponent<Text>().text;
			this.OnChange(this._currentName);
		}
	}

	private void OnCheckUsernameIsUnique(bool unique)
	{
		if (this._currentName.Length < 3)
		{
			this.IsCorrect = false;
			this.CorrectObject.SetActive(false);
			this.InCorrectObject.SetActive(true);
			return;
		}
		this.IsCorrect = unique;
		this.CorrectObject.SetActive(unique);
		this.InCorrectObject.SetActive(!unique);
	}

	public GameObject CorrectObject;

	public GameObject InCorrectObject;

	[HideInInspector]
	public bool IsCorrect;

	private string _currentName;
}
