using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

namespace MultiplayerBasicExample
{
	public class PlayerManager : MonoBehaviour
	{
		private void Start()
		{
			InputManager.OnDeviceDetached += this.OnDeviceDetached;
		}

		private void Update()
		{
			InputDevice activeDevice = InputManager.ActiveDevice;
			if (this.JoinButtonWasPressedOnDevice(activeDevice) && this.ThereIsNoPlayerUsingDevice(activeDevice))
			{
				this.CreatePlayer(activeDevice);
			}
		}

		private bool JoinButtonWasPressedOnDevice(InputDevice inputDevice)
		{
			return inputDevice.Action1.WasPressed || inputDevice.Action2.WasPressed || inputDevice.Action3.WasPressed || inputDevice.Action4.WasPressed;
		}

		private Player FindPlayerUsingDevice(InputDevice inputDevice)
		{
			int count = this.players.Count;
			for (int i = 0; i < count; i++)
			{
				Player player = this.players[i];
				if (player.Device == inputDevice)
				{
					return player;
				}
			}
			return null;
		}

		private bool ThereIsNoPlayerUsingDevice(InputDevice inputDevice)
		{
			return this.FindPlayerUsingDevice(inputDevice) == null;
		}

		private void OnDeviceDetached(InputDevice inputDevice)
		{
			Player player = this.FindPlayerUsingDevice(inputDevice);
			if (player != null)
			{
				this.RemovePlayer(player);
			}
		}

		private Player CreatePlayer(InputDevice inputDevice)
		{
			if (this.players.Count < 4)
			{
				Vector3 vector = this.playerPositions[0];
				this.playerPositions.RemoveAt(0);
				GameObject gameObject = Object.Instantiate<GameObject>(this.playerPrefab, vector, Quaternion.identity);
				Player component = gameObject.GetComponent<Player>();
				component.Device = inputDevice;
				this.players.Add(component);
				return component;
			}
			return null;
		}

		private void RemovePlayer(Player player)
		{
			this.playerPositions.Insert(0, player.transform.position);
			this.players.Remove(player);
			player.Device = null;
			Object.Destroy(player.gameObject);
		}

		private void OnGUI()
		{
			float num = 10f;
			GUI.Label(new Rect(10f, num, 300f, num + 22f), string.Concat(new object[]
			{
				"Active players: ",
				this.players.Count,
				"/",
				4
			}));
			num += 22f;
			if (this.players.Count < 4)
			{
				GUI.Label(new Rect(10f, num, 300f, num + 22f), "Press a button to join!");
				num += 22f;
			}
		}

		public GameObject playerPrefab;

		private const int maxPlayers = 4;

		private List<Vector3> playerPositions = new List<Vector3>
		{
			new Vector3(-1f, 1f, -10f),
			new Vector3(1f, 1f, -10f),
			new Vector3(-1f, -1f, -10f),
			new Vector3(1f, -1f, -10f)
		};

		private List<Player> players = new List<Player>(4);
	}
}
