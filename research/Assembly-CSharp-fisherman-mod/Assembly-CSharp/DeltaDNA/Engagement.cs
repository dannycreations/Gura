using System;
using System.Collections.Generic;
using DeltaDNA.MiniJSON;

namespace DeltaDNA
{
	public class Engagement<T> where T : Engagement<T>
	{
		public Engagement(string decisionPoint)
		{
			if (string.IsNullOrEmpty(decisionPoint))
			{
				throw new ArgumentException("decisionPoint cannot be null or empty");
			}
			this.DecisionPoint = decisionPoint;
			this.Flavour = "engagement";
			this.parameters = new Params();
		}

		public string DecisionPoint { get; private set; }

		public string Flavour { get; private set; }

		public T AddParam(string key, object value)
		{
			this.parameters.AddParam(key, value);
			return (T)((object)this);
		}

		public Dictionary<string, object> AsDictionary()
		{
			return new Dictionary<string, object>
			{
				{ "decisionPoint", this.DecisionPoint },
				{ "flavour", this.Flavour },
				{
					"parameters",
					new Dictionary<string, object>(this.parameters.AsDictionary())
				}
			};
		}

		public string Raw
		{
			get
			{
				return this.response;
			}
			set
			{
				Dictionary<string, object> dictionary = null;
				if (!string.IsNullOrEmpty(value))
				{
					try
					{
						dictionary = Json.Deserialize(value) as Dictionary<string, object>;
					}
					catch (Exception)
					{
					}
				}
				this.response = value;
				this.JSON = dictionary ?? new Dictionary<string, object>();
			}
		}

		public int StatusCode { get; set; }

		public string Error { get; set; }

		public Dictionary<string, object> JSON { get; private set; }

		public override string ToString()
		{
			return string.Format("[Engagement: DecisionPoint={0}, Flavour={1}, Raw={2}, StatusCode={3}, Error={4}, JSON={5}]", new object[] { this.DecisionPoint, this.Flavour, this.Raw, this.StatusCode, this.Error, this.JSON });
		}

		private readonly Params parameters;

		private string response;
	}
}
