using System;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class InputFieldMinMaxChecker : IDisposable
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> OnValueChanged = delegate(int v)
		{
		};

		public int Value { get; protected set; }

		public void Init(InputField input)
		{
			if (input != null)
			{
				this.If = input;
				this.If.onEndEdit.AddListener(new UnityAction<string>(this.Listener));
			}
		}

		public virtual void Listener(string v)
		{
			if (this.Range.Check(v))
			{
				this.UpdateValue(int.Parse(v));
				this.OnValueChanged(this.Value);
			}
			else
			{
				int num;
				if (int.TryParse(v, out num) && num > this.Range.Max)
				{
					this.UpdateValue(this.Range.Max);
				}
				else
				{
					this.UpdateValue(this.Value);
				}
				UIAudioSourceListener.Instance.Fail();
			}
		}

		public virtual void SetRange(Range range, int? value = null)
		{
			this.Range = range;
			this.UpdateValue((value == null || !range.Check(value.Value)) ? this.Range.Min : value.Value);
		}

		public virtual void Inc(int v)
		{
			int num = this.Value + v;
			if (this.Range.Check(num))
			{
				this.UpdateValue(num);
				this.OnValueChanged(this.Value);
			}
			else
			{
				UIAudioSourceListener.Instance.Fail();
			}
		}

		public virtual void Dispose()
		{
			if (this.If != null)
			{
				this.If.onValueChanged.RemoveListener(new UnityAction<string>(this.Listener));
				this.If = null;
			}
		}

		protected virtual void UpdateValue(int v)
		{
			this.Value = v;
			if (this.If != null)
			{
				this.If.text = this.Value.ToString();
			}
		}

		protected InputField If;

		protected Range Range = new Range(0, int.MaxValue);
	}
}
