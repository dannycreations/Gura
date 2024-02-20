using System;
using System.Collections;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class TournamentDecorations : MonoBehaviour
{
	private void Awake()
	{
		this._sqDistToActivateAudio = this._distToActivateAudio * this._distToActivateAudio;
		for (int i = 0; i < this._records.Count; i++)
		{
			this._records[i].Root.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		ProfileTournament tournament = PhotonConnectionFactory.Instance.Profile.Tournament;
		int newID = ((tournament == null || tournament.SerieInstanceId == null) ? (-1) : tournament.TemplateId);
		if (newID != -1)
		{
			if (this._lastRecordIndex != -1)
			{
				TournamentDecorations.Record record = this._records[this._lastRecordIndex];
				if (newID != record.TournamentID)
				{
					this.DisableCurrent();
				}
			}
			int num = this._records.FindIndex((TournamentDecorations.Record rr) => rr.TournamentID == newID);
			if (num != -1 && num != this._lastRecordIndex)
			{
				this.ActivateByIndex(num, false);
			}
		}
		else if (this._lastRecordIndex != -1)
		{
			this.DisableCurrent();
		}
		this.UpdateSounds();
	}

	private void UpdateSounds()
	{
		if (this._lastRecordIndex != -1 && GameFactory.Player != null && this._nextAudioUpdateAt < Time.time)
		{
			this._nextAudioUpdateAt = Time.time + this._audioUpdateDelay;
			for (int i = 0; i < this._audiObjects.Count; i++)
			{
				this._audiObjects[i].enabled = (this._audiObjects[i].transform.position - GameFactory.Player.Position).sqrMagnitude < this._sqDistToActivateAudio;
			}
		}
	}

	private void ActivateByIndex(int index, bool debugActivation = false)
	{
		this._activatedByDebug = debugActivation;
		this._lastRecordIndex = index;
		TournamentDecorations.Record record = this._records[index];
		if (record.TemplateMaterial != null)
		{
			base.StartCoroutine(this.LoadTexturesAsync());
		}
		else
		{
			record.Root.gameObject.SetActive(true);
		}
		for (int i = 0; i < record.Root.childCount; i++)
		{
			AudioSource component = record.Root.GetChild(i).GetComponent<AudioSource>();
			if (component != null)
			{
				this._audiObjects.Add(component);
			}
		}
		this._nextAudioUpdateAt = -1f;
		this.UpdateSounds();
	}

	private void DisableCurrent()
	{
		this._activatedByDebug = false;
		TournamentDecorations.Record record = this._records[this._lastRecordIndex];
		if (record.TemplateMaterial != null)
		{
			this.SetTextureToAllObjects(record.Root, record.TemplateMaterial, null);
		}
		record.Root.gameObject.SetActive(false);
		this._lastRecordIndex = -1;
		this._audiObjects.Clear();
	}

	private IEnumerator LoadTexturesAsync()
	{
		TournamentDecorations.Record r = this._records[this._lastRecordIndex];
		ResourceRequest request = Resources.LoadAsync<Texture2D>(r.TexturePath);
		yield return request;
		Texture2D texture = request.asset as Texture2D;
		this.SetTextureToAllObjects(r.Root, r.TemplateMaterial, texture);
		r.Root.gameObject.SetActive(true);
		yield break;
	}

	private void SetTextureToAllObjects(Transform decoratorObject, Material templateMaterial, Texture2D texture)
	{
		for (int i = 0; i < decoratorObject.childCount; i++)
		{
			Transform child = decoratorObject.GetChild(i);
			MeshRenderer component = child.GetComponent<MeshRenderer>();
			if (component != null)
			{
				Material material = TournamentDecorations.FindMaterial(component, templateMaterial);
				if (material != null)
				{
					material.SetTexture("_MainTex", texture);
				}
			}
		}
	}

	private static Material FindMaterial(MeshRenderer renderer, Material material)
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			if (renderer.materials[i].name.Replace(" (Instance)", string.Empty) == material.name)
			{
				return renderer.materials[i];
			}
		}
		return null;
	}

	[SerializeField]
	private List<TournamentDecorations.Record> _records;

	[SerializeField]
	private float _distToActivateAudio = 10f;

	[SerializeField]
	private float _audioUpdateDelay = 1f;

	private float _sqDistToActivateAudio;

	private int _lastRecordIndex = -1;

	private bool _activatedByDebug;

	private float _nextAudioUpdateAt = -1f;

	private List<AudioSource> _audiObjects = new List<AudioSource>();

	[Serializable]
	private class Record
	{
		public int TournamentID;

		public Transform Root;

		public string TexturePath = "Equipment/Events/TournametDecor/res/MCT-texture";

		public Material TemplateMaterial;
	}
}
