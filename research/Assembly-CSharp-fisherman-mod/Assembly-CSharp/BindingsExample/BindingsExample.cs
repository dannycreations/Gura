using System;
using InControl;
using UnityEngine;

namespace BindingsExample
{
	public class BindingsExample : MonoBehaviour
	{
		private void OnEnable()
		{
			this.playerActions = PlayerActions.CreateWithDefaultBindings();
			this.LoadBindings();
		}

		private void OnDisable()
		{
			this.playerActions.Destroy();
		}

		private void Start()
		{
			this.cachedRenderer = base.GetComponent<Renderer>();
		}

		private void Update()
		{
			base.transform.Rotate(Vector3.down, 500f * Time.deltaTime * this.playerActions.Move.X, 0);
			base.transform.Rotate(Vector3.right, 500f * Time.deltaTime * this.playerActions.Move.Y, 0);
			Color color = ((!this.playerActions.Fire.IsPressed) ? Color.white : Color.red);
			Color color2 = ((!this.playerActions.Jump.IsPressed) ? Color.white : Color.green);
			this.cachedRenderer.material.color = Color.Lerp(color, color2, 0.5f);
		}

		private void SaveBindings()
		{
			this.saveData = this.playerActions.Save();
			PlayerPrefs.SetString("Bindings", this.saveData);
		}

		private void LoadBindings()
		{
			if (PlayerPrefs.HasKey("Bindings"))
			{
				this.saveData = PlayerPrefs.GetString("Bindings");
				this.playerActions.Load(this.saveData);
			}
		}

		private void OnApplicationQuit()
		{
			PlayerPrefs.Save();
		}

		private void OnGUI()
		{
			float num = 10f;
			GUI.Label(new Rect(10f, num, 300f, num + 22f), "Last Input Type: " + this.playerActions.LastInputType);
			num += 22f;
			int count = this.playerActions.Actions.Count;
			for (int i = 0; i < count; i++)
			{
				PlayerAction playerAction = this.playerActions.Actions[i];
				string text = playerAction.Name;
				if (playerAction.IsListeningForBinding)
				{
					text += " (Listening)";
				}
				text = text + " = " + playerAction.Value;
				GUI.Label(new Rect(10f, num, 500f, num + 22f), text);
				num += 22f;
				int count2 = playerAction.Bindings.Count;
				for (int j = 0; j < count2; j++)
				{
					BindingSource bindingSource = playerAction.Bindings[j];
					GUI.Label(new Rect(75f, num, 300f, num + 22f), bindingSource.DeviceName + ": " + bindingSource.Name);
					if (GUI.Button(new Rect(20f, num + 3f, 20f, 17f), "-"))
					{
						playerAction.RemoveBinding(bindingSource);
					}
					if (GUI.Button(new Rect(45f, num + 3f, 20f, 17f), "+"))
					{
						playerAction.ListenForBindingReplacing(bindingSource);
					}
					num += 22f;
				}
				if (GUI.Button(new Rect(20f, num + 3f, 20f, 17f), "+"))
				{
					playerAction.ListenForBinding();
				}
				if (GUI.Button(new Rect(50f, num + 3f, 50f, 17f), "Reset"))
				{
					playerAction.ResetBindings();
				}
				num += 25f;
			}
			if (GUI.Button(new Rect(20f, num + 3f, 50f, 22f), "Load"))
			{
				this.LoadBindings();
			}
			if (GUI.Button(new Rect(80f, num + 3f, 50f, 22f), "Save"))
			{
				this.SaveBindings();
			}
			if (GUI.Button(new Rect(140f, num + 3f, 50f, 22f), "Reset"))
			{
				this.playerActions.Reset();
			}
		}

		private Renderer cachedRenderer;

		private PlayerActions playerActions;

		private string saveData;
	}
}
