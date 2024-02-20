using System;
using UnityEngine;

namespace FireWork
{
	public class SoundController : MonoBehaviour, IEnvironmentSound
	{
		public void OnApplicationQuit()
		{
			SoundController.instance = null;
		}

		public void Start()
		{
			if (SoundController.instance != null && SoundController.instance != this)
			{
				Object.Destroy(SoundController.instance);
			}
			SoundController.instance = this;
			this.AddChannels();
		}

		public void StopMusic(bool fade)
		{
			this.PlayMusic(null, 0f, 1f, fade);
		}

		public void FadeUpMusic()
		{
			if (this._musicChannels[this._musicChannel].volume < this._fadeTo)
			{
				this._musicChannels[this._musicChannel].volume += 0.0025f;
			}
			else
			{
				base.CancelInvoke("FadeUpMusic");
			}
		}

		public void FadeDownMusic()
		{
			int num = 0;
			if (this._musicChannel == 0)
			{
				num = 1;
			}
			if (this._musicChannels[num].volume > 0f)
			{
				this._musicChannels[num].volume -= 0.0025f;
			}
			else
			{
				this._musicChannels[num].Stop();
				base.CancelInvoke("FadeDownMusic");
			}
		}

		public void UpdateMusicVolume()
		{
			for (int i = 0; i < 2; i++)
			{
				this._musicChannels[i].volume = this._currentMusicVol * this._masterVol * this._musicVol;
			}
		}

		public void AddChannels()
		{
			this.channels = new AudioSource[this._audioChannels];
			this._channelVolumes = new float[this._audioChannels];
			this._musicChannels = new AudioSource[2];
			if (this.channels.Length <= this._audioChannels)
			{
				for (int i = 0; i < this._audioChannels; i++)
				{
					GameObject gameObject = new GameObject();
					gameObject.AddComponent<AudioSource>();
					gameObject.name = "AudioChannel " + i;
					gameObject.transform.parent = base.transform;
					this.channels[i] = gameObject.GetComponent<AudioSource>();
					if (this._linearRollOff)
					{
						this.channels[i].rolloffMode = 1;
					}
				}
			}
			for (int j = 0; j < 2; j++)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.AddComponent<AudioSource>();
				gameObject2.name = "MusicChannel " + j;
				gameObject2.transform.parent = base.transform;
				this._musicChannels[j] = gameObject2.GetComponent<AudioSource>();
				this._musicChannels[j].loop = true;
				this._musicChannels[j].volume = 0f;
				if (this._linearRollOff)
				{
					this._musicChannels[j].rolloffMode = 1;
				}
			}
			GameFactory.AudioController.RegisterSound(this);
		}

		public void PlayMusic(AudioClip clip, float volume, float pitch, bool fade)
		{
			if (!fade)
			{
				this._musicChannels[this._musicChannel].volume = 0f;
			}
			if (this._musicChannel == 0)
			{
				this._musicChannel = 1;
			}
			else
			{
				this._musicChannel = 0;
			}
			this._currentMusicVol = volume;
			this._musicChannels[this._musicChannel].clip = clip;
			if (fade)
			{
				this._fadeTo = volume * this._masterVol * this._musicVol;
				base.InvokeRepeating("FadeUpMusic", 0.01f, 0.01f);
				base.InvokeRepeating("FadeDownMusic", 0.01f, 0.01f);
			}
			else
			{
				this._musicChannels[this._musicChannel].volume = volume * this._masterVol * this._musicVol;
			}
			this._musicChannels[this._musicChannel].GetComponent<AudioSource>().pitch = pitch;
			this._musicChannels[this._musicChannel].GetComponent<AudioSource>().Play();
		}

		public void Play(AudioClip clip, float volume, float pitch, Vector3 position)
		{
			if (this.channel < this.channels.Length - 1)
			{
				this.channel++;
			}
			else
			{
				this.channel = 0;
			}
			this.channels[this.channel].clip = clip;
			this._channelVolumes[this.channel] = volume;
			this.channels[this.channel].volume = volume * this._masterVol * this._soundVol * GlobalConsts.BgVolume;
			this.channels[this.channel].pitch = pitch;
			this.channels[this.channel].transform.position = position;
			this.channels[this.channel].Play();
		}

		public void Mute(bool flag)
		{
			for (int i = 0; i < this.channels.Length; i++)
			{
				this.channels[i].mute = flag;
			}
		}

		public void SetVolume(float globalVolume)
		{
			for (int i = 0; i < this.channels.Length; i++)
			{
				this.channels[i].volume = this._channelVolumes[this.channel] * this._masterVol * this._soundVol * globalVolume;
			}
		}

		private void OnDestroy()
		{
			if (GameFactory.AudioController != null)
			{
				GameFactory.AudioController.UnRegisterSound(this);
			}
		}

		public void StopAll()
		{
			for (int i = 0; i < this.channels.Length; i++)
			{
				this.channels[i].Stop();
			}
		}

		public AudioClip[] _audioClips;

		public int _audioChannels = 10;

		public float _masterVol = 0.5f;

		public float _soundVol = 1f;

		public float _musicVol = 1f;

		public bool _linearRollOff;

		public AudioSource[] channels;

		[SerializeField]
		private float[] _channelVolumes;

		public int channel;

		public AudioSource[] _musicChannels;

		public int _musicChannel;

		private float _currentMusicVol;

		private float _fadeTo;

		public static SoundController instance;
	}
}
