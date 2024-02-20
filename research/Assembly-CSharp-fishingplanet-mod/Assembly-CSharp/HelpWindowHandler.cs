using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HelpWindowHandler : HelpWindowHandlerBase
{
	protected override void Start()
	{
		this._contentLanguages.Add(1, this.ContentEnglish);
		this._contentLanguages.Add(2, this.ContentRussian);
		this._contentLanguages.Add(4, this.ContentDeutch);
		this._contentLanguages.Add(5, this.ContentFrance);
		this._contentLanguages.Add(6, this.ContentPoland);
		this._contentLanguages.Add(7, this.ContentUkrainian);
		GameObject gameObject = ((!this._contentLanguages.ContainsKey(ChangeLanguage.GetCurrentLanguage.Id)) ? this.ContentEnglish : this._contentLanguages[ChangeLanguage.GetCurrentLanguage.Id]);
		List<GameObject> list = this._contentLanguages.Values.ToList<GameObject>();
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject2 = list[i];
			gameObject2.SetActive(gameObject.Equals(gameObject2));
		}
		base.Start();
	}

	public GameObject ContentEnglish;

	public GameObject ContentRussian;

	public GameObject ContentDeutch;

	public GameObject ContentFrance;

	public GameObject ContentPoland;

	public GameObject ContentUkrainian;

	private readonly Dictionary<int, GameObject> _contentLanguages = new Dictionary<int, GameObject>();
}
