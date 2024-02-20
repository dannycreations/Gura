using System;
using UnityEngine;

public class UBHelper : MonoBehaviour
{
	public static bool GroupHeader(string text, bool isExpanded)
	{
		Rect rect = GUILayoutUtility.GetRect(16f, 22f, UBHelper.s_Styles.header);
		UBHelper.s_Styles.Backup();
		UBHelper.s_Styles.Apply();
		if (Event.current.type == 7)
		{
			UBHelper.s_Styles.header.Draw(rect, text, isExpanded, isExpanded, isExpanded, isExpanded);
		}
		Event current = Event.current;
		if (current.type == null && rect.Contains(current.mousePosition))
		{
			isExpanded = !isExpanded;
			current.Use();
		}
		UBHelper.s_Styles.Revert();
		return isExpanded;
	}

	private static UBHelper.Styles s_Styles = new UBHelper.Styles();

	private class Styles
	{
		internal Styles()
		{
			this.header.font = new GUIStyle("Label").font;
		}

		public void Backup()
		{
			this.m_Border = UBHelper.s_Styles.header.border;
			this.m_FixedHeight = UBHelper.s_Styles.header.fixedHeight;
			this.m_ContentOffset = UBHelper.s_Styles.header.contentOffset;
			this.m_TextAlign = UBHelper.s_Styles.header.alignment;
			this.m_TextStyle = UBHelper.s_Styles.header.fontStyle;
			this.m_FontSize = UBHelper.s_Styles.header.fontSize;
		}

		public void Apply()
		{
			UBHelper.s_Styles.header.border = new RectOffset(7, 7, 4, 4);
			UBHelper.s_Styles.header.fixedHeight = 22f;
			UBHelper.s_Styles.header.contentOffset = new Vector2(20f, -2f);
			UBHelper.s_Styles.header.alignment = 3;
			UBHelper.s_Styles.header.fontStyle = 1;
			UBHelper.s_Styles.header.fontSize = 12;
		}

		public void Revert()
		{
			UBHelper.s_Styles.header.border = this.m_Border;
			UBHelper.s_Styles.header.fixedHeight = this.m_FixedHeight;
			UBHelper.s_Styles.header.contentOffset = this.m_ContentOffset;
			UBHelper.s_Styles.header.alignment = this.m_TextAlign;
			UBHelper.s_Styles.header.fontStyle = this.m_TextStyle;
			UBHelper.s_Styles.header.fontSize = this.m_FontSize;
		}

		public GUIStyle header = "ShurikenModuleTitle";

		public GUIStyle headerArrow = "AC RightArrow";

		private RectOffset m_Border;

		private float m_FixedHeight;

		private Vector2 m_ContentOffset;

		private TextAnchor m_TextAlign;

		private FontStyle m_TextStyle;

		private int m_FontSize;
	}
}
