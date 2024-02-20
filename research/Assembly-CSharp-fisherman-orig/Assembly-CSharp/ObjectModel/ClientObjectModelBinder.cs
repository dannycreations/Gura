using System;
using System.Runtime.Serialization;

namespace ObjectModel
{
	public class ClientObjectModelBinder : SerializationBinder
	{
		public static void Init()
		{
			SerializationHelper.JsonSerializerSettings.Binder = new ClientObjectModelBinder();
		}

		public override Type BindToType(string assemblyName, string typeName)
		{
			return Type.GetType(typeName);
		}
	}
}
