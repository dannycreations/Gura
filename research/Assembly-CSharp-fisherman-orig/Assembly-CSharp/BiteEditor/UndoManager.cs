using System;
using System.Collections.Generic;
using UnityEngine;

namespace BiteEditor
{
	[Serializable]
	public class UndoManager : ISerializationCallbackReceiver
	{
		public UndoManager()
		{
			if (this._undoState == null)
			{
				this._undoState = new List<UndoManager.State>(20);
			}
		}

		public int Step
		{
			get
			{
				return this._step;
			}
			set
			{
				this._step = value;
			}
		}

		public bool Initialized { get; set; }

		public bool HasUndoRedoPerformed { get; set; }

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (!this.Initialized)
			{
				this.Step = -1;
				this.Initialized = true;
			}
		}

		public void UndoRedoPerformed()
		{
			this.HasUndoRedoPerformed = true;
			this.RestoreTexture(this.Step);
		}

		private void RestoreTexture(int index)
		{
			if (index > -1 && index < this._undoState.Count)
			{
				this._undoState[index].Restore();
			}
		}

		public void Record(List<Texture2D> textures)
		{
			UndoManager.State state = new UndoManager.State(textures);
			if (this._undoState.Count == 0 || this.Step > this._undoState.Count)
			{
				this.Step = -1;
			}
			if (++this.Step == 20)
			{
				this.Step = 0;
			}
			if (this._undoState.Count < 20)
			{
				this._undoState.Insert(this.Step, state);
			}
			else
			{
				this._undoState[this.Step] = state;
			}
		}

		private const int MaxUndo = 20;

		[SerializeField]
		private int _step = -1;

		private List<UndoManager.State> _undoState;

		private class State
		{
			public State(List<Texture2D> textures)
			{
				this._state = new List<KeyValuePair<Texture2D, Color[]>>();
				this.Store(textures);
			}

			public void Store(List<Texture2D> textures)
			{
				foreach (Texture2D texture2D in textures)
				{
					this._state.Add(new KeyValuePair<Texture2D, Color[]>(texture2D, texture2D.GetPixels(0)));
				}
			}

			public void Restore()
			{
				foreach (KeyValuePair<Texture2D, Color[]> keyValuePair in this._state)
				{
					keyValuePair.Key.SetPixels(keyValuePair.Value);
					keyValuePair.Key.Apply();
				}
			}

			private List<KeyValuePair<Texture2D, Color[]>> _state;
		}
	}
}
