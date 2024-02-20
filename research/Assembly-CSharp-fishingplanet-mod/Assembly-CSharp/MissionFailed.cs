using System;
using Assets.Scripts.UI._2D.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class MissionFailed : MonoBehaviour
{
	public void Init(string name, int? imageBID, string description)
	{
		this.IconLdbl.Load(imageBID, this._image, "Textures/Inventory/{0}");
		this._name.text = name;
		this._description.text = description;
	}

	[SerializeField]
	private Text _name;

	[SerializeField]
	private Text _description;

	[SerializeField]
	private Image _image;

	private ResourcesHelpers.AsyncLoadableImage IconLdbl = new ResourcesHelpers.AsyncLoadableImage();
}
