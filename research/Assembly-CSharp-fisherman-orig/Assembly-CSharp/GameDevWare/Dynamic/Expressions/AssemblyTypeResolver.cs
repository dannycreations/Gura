using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameDevWare.Dynamic.Expressions
{
	public sealed class AssemblyTypeResolver : KnownTypeResolver
	{
		public AssemblyTypeResolver(params Assembly[] assemblies)
			: this(assemblies, null)
		{
		}

		public AssemblyTypeResolver(IEnumerable<Assembly> assemblies)
			: this(assemblies, null)
		{
		}

		public AssemblyTypeResolver(IEnumerable<Assembly> assemblies, ITypeResolver otherTypeResolver)
			: base(from t in (assemblies ?? Enumerable.Empty<Assembly>()).SelectMany((Assembly a) => a.GetTypes())
				where t.IsPublic
				select t, otherTypeResolver)
		{
			if (assemblies == null)
			{
				throw new ArgumentNullException("assemblies");
			}
		}
	}
}
