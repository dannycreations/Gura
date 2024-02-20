using System;

namespace InControl
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class SingletonPrefabAttribute : Attribute
	{
		public SingletonPrefabAttribute(string name)
		{
			this.Name = name;
		}

		public readonly string Name;
	}
}
