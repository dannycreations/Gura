using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomElementPondMap : MonoBehaviour
{
	public bool IsSelected { get; protected set; }

	public string Id { get; protected set; }

	public string Name
	{
		get
		{
			return this.TextValue.text;
		}
	}

	public bool IsVisible
	{
		get
		{
			return base.gameObject.activeSelf;
		}
	}

	private void Awake()
	{
		this.Cg.interactable = this.Interactable;
		this.Tgl.onValueChanged.AddListener(delegate(bool b)
		{
			this.OnActive(b);
		});
	}

	public void Init(string locName, ToggleGroup tGroup, string id, bool isEnabled)
	{
		this.Tgl.group = tGroup;
		this.SetId(id);
		this.SetName(locName);
		this.SetActiveAndInteractable(isEnabled);
	}

	public void SetId(string id)
	{
		this.Id = id;
	}

	public void SetActiveAndInteractable(bool v)
	{
		this.SetActive(v);
		this.Cg.interactable = v;
		this.Interactable = v;
	}

	public void SetActive(bool v)
	{
		base.gameObject.SetActive(v);
	}

	public void SetOn(bool v)
	{
		PlayButtonEffect.SetToogleOn(v, this.Tgl);
	}

	public void SetName(string name)
	{
		this.TextValue.text = name;
	}

	public void SetSelected(bool v)
	{
		this.IsSelected = v;
		this.TextValue.color = ((!this.IsSelected) ? this.NormalColor : Color.white);
		this.TextValue.fontStyle = ((!this.IsSelected) ? 0 : 1);
		this.Corner.gameObject.SetActive(this.IsSelected);
		this.ToggleOnOff.gameObject.SetActive(this.IsSelected);
	}

	public void SetLineBold(bool v)
	{
		this.Line.color = new Color(this.Line.color.r, this.Line.color.g, this.Line.color.b, (!v) ? 0.2f : 0.6f);
	}

	public void SetLineLast(bool v)
	{
		this.Line.gameObject.SetActive(!v);
		this.Last.SetActive(v);
	}

	[SerializeField]
	protected TextMeshProUGUI Corner;

	[SerializeField]
	protected TextMeshProUGUI TextValue;

	[SerializeField]
	protected TextMeshProUGUI ToggleOnOff;

	[SerializeField]
	protected Toggle Tgl;

	[SerializeField]
	protected CanvasGroup Cg;

	[SerializeField]
	protected Image Line;

	[SerializeField]
	protected GameObject Last;

	public Action<bool> OnActive = delegate(bool b)
	{
	};

	protected bool Interactable;

	protected readonly Color NormalColor = new Color(0.8901961f, 0.8901961f, 0.8901961f);
}
