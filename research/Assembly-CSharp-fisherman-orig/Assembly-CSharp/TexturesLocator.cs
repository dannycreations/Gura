using System;
using UnityEngine;

public class TexturesLocator : MonoBehaviour
{
	public string Path
	{
		get
		{
			return this._path;
		}
	}

	public void Set(string path)
	{
		this._path = path;
	}

	[SerializeField]
	private string _path;
}
