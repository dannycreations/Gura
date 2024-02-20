using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class GameEvent<T> where T : GameEvent<T>
	{
		public GameEvent(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name cannot be null or empty");
			}
			this.Name = name;
			this.parameters = new Params();
		}

		public string Name { get; private set; }

		public T AddParam(string key, object value)
		{
			this.parameters.AddParam(key, value);
			return (T)((object)this);
		}

		public Dictionary<string, object> AsDictionary()
		{
			return new Dictionary<string, object>
			{
				{ "eventName", this.Name },
				{
					"eventParams",
					new Dictionary<string, object>(this.parameters.AsDictionary())
				}
			};
		}

		private readonly Params parameters;
	}
}
