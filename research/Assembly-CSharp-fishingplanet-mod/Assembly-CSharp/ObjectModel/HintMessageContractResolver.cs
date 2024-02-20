using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ObjectModel
{
	public class HintMessageContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> list = base.CreateProperties(type, 0);
			if (type == typeof(HintMessage))
			{
				return list.Where((JsonProperty p) => HintMessageContractResolver.hintMessageProperties.Contains(p.PropertyName)).ToList<JsonProperty>();
			}
			return list.Where(delegate(JsonProperty property)
			{
				PropertyInfo property2 = type.GetProperty(property.PropertyName, BindingFlags.Instance | BindingFlags.Public);
				return property2 != null && !property2.GetCustomAttributes(typeof(JsonServerAttribute), true).Any<object>();
			}).ToList<JsonProperty>();
		}

		private static readonly string[] hintMessageProperties = new string[] { "MessageId", "Data", "Strings" };
	}
}
