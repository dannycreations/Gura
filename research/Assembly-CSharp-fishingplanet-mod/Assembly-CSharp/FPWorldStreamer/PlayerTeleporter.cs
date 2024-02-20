using System;
using UnityEngine;

namespace FPWorldStreamer
{
	public class PlayerTeleporter : MonoBehaviour
	{
		private void Start()
		{
			if (this._streamer == null)
			{
				this._streamer = base.GetComponent<Streamer>();
			}
			if (this._player == null)
			{
				GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
				if (gameObject != null)
				{
					this._player = gameObject.transform;
				}
			}
		}

		private void Update()
		{
			if (Input.GetKeyUp(116))
			{
				this._streamer.TeleportPlayer(new Vector3(Random.Range(0f, this._maxX), this._player.position.y, Random.Range(0f, this._maxZ)), false);
			}
		}

		[SerializeField]
		private Streamer _streamer;

		[SerializeField]
		private Transform _player;

		[SerializeField]
		private float _maxX = 2000f;

		[SerializeField]
		private float _maxZ = 2000f;
	}
}
