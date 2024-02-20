using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

namespace MultiplayerWithBindingsExample
{
	public class PlayerManager : MonoBehaviour
	{
		private void OnEnable()
		{
			InputManager.OnDeviceDetached += this.OnDeviceDetached;
			this.keyboardListener = PlayerActions.CreateWithKeyboardBindings();
			this.joystickListener = PlayerActions.CreateWithJoystickBindings();
		}

		private void OnDisable()
		{
			InputManager.OnDeviceDetached -= this.OnDeviceDetached;
			this.joystickListener.Destroy();
			this.keyboardListener.Destroy();
		}

		private void Update()
		{
			if (this.JoinButtonWasPressedOnListener(this.joystickListener))
			{
				InputDevice activeDevice = InputManager.ActiveDevice;
				if (this.ThereIsNoPlayerUsingJoystick(activeDevice))
				{
					this.CreatePlayer(activeDevice);
				}
			}
			if (this.JoinButtonWasPressedOnListener(this.keyboardListener) && this.ThereIsNoPlayerUsingKeyboard())
			{
				this.CreatePlayer(null);
			}
		}

		private bool JoinButtonWasPressedOnListener(PlayerActions actions)
		{
			return actions.Green.WasPressed || actions.Red.WasPressed || actions.Blue.WasPressed || actions.Yellow.WasPressed;
		}

		private Player FindPlayerUsingJoystick(InputDevice inputDevice)
		{
			int count = this.players.Count;
			for (int i = 0; i < count; i++)
			{
				Player player = this.players[i];
				if (player.Actions.Device == inputDevice)
				{
					return player;
				}
			}
			return null;
		}

		private bool ThereIsNoPlayerUsingJoystick(InputDevice inputDevice)
		{
			return this.FindPlayerUsingJoystick(inputDevice) == null;
		}

		private Player FindPlayerUsingKeyboard()
		{
			int count = this.players.Count;
			for (int i = 0; i < count; i++)
			{
				Player player = this.players[i];
				if (player.Actions == this.keyboardListener)
				{
					return player;
				}
			}
			return null;
		}

		private bool ThereIsNoPlayerUsingKeyboard()
		{
			return this.FindPlayerUsingKeyboard() == null;
		}

		private void OnDeviceDetached(InputDevice inputDevice)
		{
			Player player = this.FindPlayerUsingJoystick(inputDevice);
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
				if (inputDevice == null)
				{
					component.Actions = this.keyboardListener;
				}
				else
				{
					PlayerActions playerActions = PlayerActions.CreateWithJoystickBindings();
					playerActions.Device = inputDevice;
					component.Actions = playerActions;
				}
				this.players.Add(component);
				return component;
			}
			return null;
		}

		private void RemovePlayer(Player player)
		{
			this.playerPositions.Insert(0, player.transform.position);
			this.players.Remove(player);
			player.Actions = null;
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
				GUI.Label(new Rect(10f, num, 300f, num + 22f), "Press a button or a/s/d/f key to join!");
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

		private PlayerActions keyboardListener;

		private PlayerActions joystickListener;
	}
}
