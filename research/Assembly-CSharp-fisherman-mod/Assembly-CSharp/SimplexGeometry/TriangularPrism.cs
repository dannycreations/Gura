using System;
using Mono.Simd;

namespace SimplexGeometry
{
	public class TriangularPrism : SimplexComposite
	{
		public TriangularPrism(Vector4f tA1, Vector4f tA2, Vector4f tA3, Vector4f tB1, Vector4f tB2, Vector4f tB3)
			: base(new Vector4f[] { tA1, tA2, tA3, tB1, tB2, tB3 }, new int[]
			{
				0, 1, 2, 3, 3, 4, 5, 1, 3, 5,
				2, 1
			})
		{
			base.cachedSimilarClipBody = null;
		}
	}
}
