using System;
using System.Collections.Generic;
using System.Globalization;

namespace DeltaDNA
{
	public class Params
	{
		public Params AddParam(string key, object value)
		{
			try
			{
				if (value is Params)
				{
					this._params[key] = ((Params)value).AsDictionary();
				}
				else if (value is DateTime)
				{
					this._params[key] = ((DateTime)value).ToString(Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
				}
				else
				{
					this._params[key] = value;
				}
			}
			catch (ArgumentNullException ex)
			{
				throw new ArgumentNullException("Key can not be null.", ex);
			}
			return this;
		}

		public object GetParam(string key)
		{
			object obj;
			try
			{
				obj = ((!this._params.ContainsKey(key)) ? null : this._params[key]);
			}
			catch (ArgumentNullException ex)
			{
				throw new Exception("Key can not be null.", ex);
			}
			catch (KeyNotFoundException ex2)
			{
				throw new Exception("Key " + key + " not found.", ex2);
			}
			return obj;
		}

		public Dictionary<string, object> AsDictionary()
		{
			return this._params;
		}

		private Dictionary<string, object> _params = new Dictionary<string, object>();
	}
}
