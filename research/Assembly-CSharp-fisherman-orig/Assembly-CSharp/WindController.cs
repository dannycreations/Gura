using System;
using System.Linq;
using UnityEngine;

public class WindController : MonoBehaviour
{
	public float CurWindObjectVolumeMultiplier
	{
		get
		{
			return (this._curRecord != -1) ? this._records[this._curRecord].ObjectsVolumeMultiplier : 0f;
		}
	}

	private void Awake()
	{
		this._records = this._records.OrderBy((WindController.Record r) => r.MinSpeed).ToArray<WindController.Record>();
		for (int i = 0; i < this._records.Length; i++)
		{
			AudioSource sound = this._records[i].Sound;
			sound.gameObject.SetActive(true);
			sound.loop = true;
			this._records[i].InitialVolume = sound.volume;
			sound.volume *= GlobalConsts.BgVolume;
			sound.Play();
			sound.mute = true;
			sound.gameObject.SetActive(true);
		}
	}

	public void SetWind(float speed)
	{
		int i = 0;
		while (i < this._records.Length)
		{
			if (speed < this._records[i].MinSpeed)
			{
				int num = i - 1;
				if (num == -1)
				{
					this.DisableCurRecord();
					return;
				}
				this.SetRecord(num);
				return;
			}
			else
			{
				i++;
			}
		}
		if (this._records.Length > 0)
		{
			this.SetRecord(this._records.Length - 1);
		}
	}

	private void SetRecord(int nearestIndex)
	{
		if (nearestIndex != this._curRecord)
		{
			this.DisableCurRecord();
			this._curRecord = nearestIndex;
			WindController.Record record = this._records[nearestIndex];
			if (record.Sound != null)
			{
				record.Sound.mute = false;
			}
			for (int i = 0; i < record.Randomizers.Length; i++)
			{
				record.Randomizers[i].SetActive(true);
			}
		}
	}

	private void DisableCurRecord()
	{
		if (this._curRecord != -1)
		{
			WindController.Record record = this._records[this._curRecord];
			if (record.Sound != null)
			{
				record.Sound.mute = true;
			}
			for (int i = 0; i < record.Randomizers.Length; i++)
			{
				record.Randomizers[i].SetActive(false);
			}
			this._curRecord = -1;
		}
	}

	public void Mute(bool flag)
	{
		if (this._curRecord != -1)
		{
			WindController.Record record = this._records[this._curRecord];
			record.Sound.mute = flag;
			for (int i = 0; i < record.Randomizers.Length; i++)
			{
				record.Randomizers[i].Mute(flag);
			}
		}
	}

	public void UpdateVolume()
	{
		for (int i = 0; i < this._records.Length; i++)
		{
			WindController.Record record = this._records[i];
			record.Sound.volume = record.InitialVolume * GlobalConsts.BgVolume;
		}
	}

	[SerializeField]
	private WindController.Record[] _records;

	private int _curRecord = -1;

	[Serializable]
	private class Record
	{
		public float MinSpeed
		{
			get
			{
				return this._minSpeed;
			}
		}

		public AudioSource Sound
		{
			get
			{
				return this._sound;
			}
		}

		public SoundRandomizer[] Randomizers
		{
			get
			{
				return this._randomizers;
			}
		}

		public float ObjectsVolumeMultiplier
		{
			get
			{
				return this._objectsVolumeMultiplier;
			}
		}

		public float InitialVolume { get; set; }

		[SerializeField]
		private float _minSpeed;

		[SerializeField]
		private AudioSource _sound;

		[SerializeField]
		private SoundRandomizer[] _randomizers;

		[SerializeField]
		private float _objectsVolumeMultiplier;
	}
}
