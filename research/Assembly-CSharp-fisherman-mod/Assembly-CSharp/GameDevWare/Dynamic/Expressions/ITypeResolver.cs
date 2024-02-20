using System;

namespace GameDevWare.Dynamic.Expressions
{
	public interface ITypeResolver
	{
		bool TryGetType(TypeReference typeReference, out Type foundType);

		bool IsKnownType(Type type);
	}
}
