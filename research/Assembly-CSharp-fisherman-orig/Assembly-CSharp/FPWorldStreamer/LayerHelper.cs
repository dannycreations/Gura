using System;
using UnityEngine;

namespace FPWorldStreamer
{
	public class LayerHelper
	{
		public static Vector3 LayerInitialPos
		{
			get
			{
				return new Vector3(0f, -100f, 0f);
			}
		}

		public static string GetItemID(Vector3 pos, ICell layer)
		{
			return LayerHelper.GetItemID(LayerHelper.GetCellByPosition(pos, layer, default(CellPos)), layer);
		}

		public static string GetItemID(CellPos cPos, ICell layer)
		{
			return string.Format("{0}_x{1}_z{2}", layer.LayerName, cPos.X, cPos.Z);
		}

		public static CellPos GetCellPositionByItemID(string itemID)
		{
			int num = itemID.IndexOf("_x");
			int num2 = itemID.IndexOf("_z");
			int num3 = itemID.IndexOf(".unity");
			return new CellPos(Convert.ToInt32(itemID.Substring(num + 2, num2 - num - 2)), Convert.ToInt32(itemID.Substring(num2 + 2, num3 - num2 - 2)));
		}

		public static CellPos GetCellByPosition(Vector3 pos, ICell layer, CellPos movement = default(CellPos))
		{
			return new CellPos(Mathf.FloorToInt(pos.x / layer.XSize) + movement.X, Mathf.FloorToInt(pos.z / layer.ZSize) + movement.Z);
		}

		public static Vector3 GetCellCornerPosition(Vector3 pos, ICell layer, CellPos movement = default(CellPos))
		{
			CellPos cellByPosition = LayerHelper.GetCellByPosition(pos, layer, movement);
			return new Vector3((float)cellByPosition.X * layer.XSize, 0f, (float)cellByPosition.Z * layer.ZSize);
		}

		public static Vector3 GetCellCornerPosition(CellPos pos, ICell layer)
		{
			return new Vector3((float)pos.X * layer.XSize, 0f, (float)pos.Z * layer.ZSize);
		}
	}
}
