using System;

namespace GameDevWare.Dynamic.Expressions
{
	[Flags]
	public enum TypeDiscoveryOptions
	{
		Default = 0,
		Interfaces = 2,
		GenericArguments = 4,
		KnownTypes = 8,
		DeclaringTypes = 16,
		All = 30
	}
}
