using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class MissionObject : MonoBehaviour
{
	public string OriginalText { get; set; }

	private void Start()
	{
		this.UpdateState(this._state, this.OriginalText);
	}

	public void UpdateState(GameMarkerState state, string taskName)
	{
		this._state = state;
		this._imageActive.gameObject.SetActive(state == GameMarkerState.Active);
		this._imageInactive.gameObject.SetActive(state == GameMarkerState.Inactive);
		this.OriginalText = taskName;
		this.taskName.text = taskName;
	}

	public void SetActive(bool flag)
	{
		this.UpdateState((!flag) ? GameMarkerState.Undefined : this._state, this.OriginalText ?? string.Empty);
	}

	[SerializeField]
	private GameObject _imageActive;

	[SerializeField]
	private GameObject _imageInactive;

	private GameMarkerState _state;

	[SerializeField]
	private Text taskName;
}
