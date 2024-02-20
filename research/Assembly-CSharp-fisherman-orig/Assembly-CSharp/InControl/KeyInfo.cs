using System;
using UnityEngine;

namespace InControl
{
	public struct KeyInfo
	{
		private KeyInfo(Key key, string name, params KeyCode[] keyCodes)
		{
			this.key = key;
			this.name = name;
			this.macName = name;
			this.keyCodes = keyCodes;
		}

		private KeyInfo(Key key, string name, string macName, params KeyCode[] keyCodes)
		{
			this.key = key;
			this.name = name;
			this.macName = macName;
			this.keyCodes = keyCodes;
		}

		public bool IsPressed
		{
			get
			{
				int num = this.keyCodes.Length;
				for (int i = 0; i < num; i++)
				{
					if (Input.GetKey(this.keyCodes[i]))
					{
						return true;
					}
				}
				return false;
			}
		}

		public string Name
		{
			get
			{
				if (Application.platform == null || Application.platform == 1)
				{
					return this.macName;
				}
				return this.name;
			}
		}

		public Key Key
		{
			get
			{
				return this.key;
			}
		}

		private readonly Key key;

		private readonly string name;

		private readonly string macName;

		private readonly KeyCode[] keyCodes;

		public static readonly KeyInfo[] KeyList = new KeyInfo[]
		{
			new KeyInfo(Key.None, "None", new KeyCode[1]),
			new KeyInfo(Key.Shift, "Shift", new KeyCode[] { 304, 303 }),
			new KeyInfo(Key.Alt, "Alt", "Option", new KeyCode[] { 308, 307 }),
			new KeyInfo(Key.Command, "Command", new KeyCode[] { 310, 309 }),
			new KeyInfo(Key.Control, "Ctrl", new KeyCode[] { 306, 305 }),
			new KeyInfo(Key.LeftShift, "Left Shift", new KeyCode[] { 304 }),
			new KeyInfo(Key.LeftAlt, "Left Alt", "Left Option", new KeyCode[] { 308 }),
			new KeyInfo(Key.LeftCommand, "Left Command", new KeyCode[] { 310 }),
			new KeyInfo(Key.LeftControl, "Left Ctrl", new KeyCode[] { 306 }),
			new KeyInfo(Key.RightShift, "Right Shift", new KeyCode[] { 303 }),
			new KeyInfo(Key.RightAlt, "Right Alt", "Right Option", new KeyCode[] { 307 }),
			new KeyInfo(Key.RightCommand, "Right Command", new KeyCode[] { 309 }),
			new KeyInfo(Key.RightControl, "Right Ctrl", new KeyCode[] { 305 }),
			new KeyInfo(Key.Escape, "Escape", new KeyCode[] { 27 }),
			new KeyInfo(Key.F1, "F1", new KeyCode[] { 282 }),
			new KeyInfo(Key.F2, "F2", new KeyCode[] { 283 }),
			new KeyInfo(Key.F3, "F3", new KeyCode[] { 284 }),
			new KeyInfo(Key.F4, "F4", new KeyCode[] { 285 }),
			new KeyInfo(Key.F5, "F5", new KeyCode[] { 286 }),
			new KeyInfo(Key.F6, "F6", new KeyCode[] { 287 }),
			new KeyInfo(Key.F7, "F7", new KeyCode[] { 288 }),
			new KeyInfo(Key.F8, "F8", new KeyCode[] { 289 }),
			new KeyInfo(Key.F9, "F9", new KeyCode[] { 290 }),
			new KeyInfo(Key.F10, "F10", new KeyCode[] { 291 }),
			new KeyInfo(Key.F11, "F11", new KeyCode[] { 292 }),
			new KeyInfo(Key.F12, "F12", new KeyCode[] { 293 }),
			new KeyInfo(Key.Key0, "0", new KeyCode[] { 48 }),
			new KeyInfo(Key.Key1, "1", new KeyCode[] { 49 }),
			new KeyInfo(Key.Key2, "2", new KeyCode[] { 50 }),
			new KeyInfo(Key.Key3, "3", new KeyCode[] { 51 }),
			new KeyInfo(Key.Key4, "4", new KeyCode[] { 52 }),
			new KeyInfo(Key.Key5, "5", new KeyCode[] { 53 }),
			new KeyInfo(Key.Key6, "6", new KeyCode[] { 54 }),
			new KeyInfo(Key.Key7, "7", new KeyCode[] { 55 }),
			new KeyInfo(Key.Key8, "8", new KeyCode[] { 56 }),
			new KeyInfo(Key.Key9, "9", new KeyCode[] { 57 }),
			new KeyInfo(Key.A, "A", new KeyCode[] { 97 }),
			new KeyInfo(Key.B, "B", new KeyCode[] { 98 }),
			new KeyInfo(Key.C, "C", new KeyCode[] { 99 }),
			new KeyInfo(Key.D, "D", new KeyCode[] { 100 }),
			new KeyInfo(Key.E, "E", new KeyCode[] { 101 }),
			new KeyInfo(Key.F, "F", new KeyCode[] { 102 }),
			new KeyInfo(Key.G, "G", new KeyCode[] { 103 }),
			new KeyInfo(Key.H, "H", new KeyCode[] { 104 }),
			new KeyInfo(Key.I, "I", new KeyCode[] { 105 }),
			new KeyInfo(Key.J, "J", new KeyCode[] { 106 }),
			new KeyInfo(Key.K, "K", new KeyCode[] { 107 }),
			new KeyInfo(Key.L, "L", new KeyCode[] { 108 }),
			new KeyInfo(Key.M, "M", new KeyCode[] { 109 }),
			new KeyInfo(Key.N, "N", new KeyCode[] { 110 }),
			new KeyInfo(Key.O, "O", new KeyCode[] { 111 }),
			new KeyInfo(Key.P, "P", new KeyCode[] { 112 }),
			new KeyInfo(Key.Q, "Q", new KeyCode[] { 113 }),
			new KeyInfo(Key.R, "R", new KeyCode[] { 114 }),
			new KeyInfo(Key.S, "S", new KeyCode[] { 115 }),
			new KeyInfo(Key.T, "T", new KeyCode[] { 116 }),
			new KeyInfo(Key.U, "U", new KeyCode[] { 117 }),
			new KeyInfo(Key.V, "V", new KeyCode[] { 118 }),
			new KeyInfo(Key.W, "W", new KeyCode[] { 119 }),
			new KeyInfo(Key.X, "X", new KeyCode[] { 120 }),
			new KeyInfo(Key.Y, "Y", new KeyCode[] { 121 }),
			new KeyInfo(Key.Z, "Z", new KeyCode[] { 122 }),
			new KeyInfo(Key.Backquote, "Backquote", new KeyCode[] { 96 }),
			new KeyInfo(Key.Minus, "Minus", new KeyCode[] { 45 }),
			new KeyInfo(Key.Equals, "Equals", new KeyCode[] { 61 }),
			new KeyInfo(Key.Backspace, "Backspace", "Delete", new KeyCode[] { 8 }),
			new KeyInfo(Key.Tab, "Tab", new KeyCode[] { 9 }),
			new KeyInfo(Key.LeftBracket, "Left Bracket", new KeyCode[] { 91 }),
			new KeyInfo(Key.RightBracket, "Right Bracket", new KeyCode[] { 93 }),
			new KeyInfo(Key.Backslash, "Backslash", new KeyCode[] { 92 }),
			new KeyInfo(Key.Semicolon, "Semicolon", new KeyCode[] { 59 }),
			new KeyInfo(Key.Quote, "Quote", new KeyCode[] { 39 }),
			new KeyInfo(Key.Return, "Return", new KeyCode[] { 13 }),
			new KeyInfo(Key.Comma, "Comma", new KeyCode[] { 44 }),
			new KeyInfo(Key.Period, "Period", new KeyCode[] { 46 }),
			new KeyInfo(Key.Slash, "Slash", new KeyCode[] { 47 }),
			new KeyInfo(Key.Space, "Space", new KeyCode[] { 32 }),
			new KeyInfo(Key.Insert, "Insert", new KeyCode[] { 277 }),
			new KeyInfo(Key.Delete, "Delete", "Forward Delete", new KeyCode[] { 127 }),
			new KeyInfo(Key.Home, "Home", new KeyCode[] { 278 }),
			new KeyInfo(Key.End, "End", new KeyCode[] { 279 }),
			new KeyInfo(Key.PageUp, "PageUp", new KeyCode[] { 280 }),
			new KeyInfo(Key.PageDown, "PageDown", new KeyCode[] { 281 }),
			new KeyInfo(Key.LeftArrow, "Left Arrow", new KeyCode[] { 276 }),
			new KeyInfo(Key.RightArrow, "Right Arrow", new KeyCode[] { 275 }),
			new KeyInfo(Key.UpArrow, "Up Arrow", new KeyCode[] { 273 }),
			new KeyInfo(Key.DownArrow, "Down Arrow", new KeyCode[] { 274 }),
			new KeyInfo(Key.Pad0, "Num 0", new KeyCode[] { 256 }),
			new KeyInfo(Key.Pad1, "Num 1", new KeyCode[] { 257 }),
			new KeyInfo(Key.Pad2, "Num 2", new KeyCode[] { 258 }),
			new KeyInfo(Key.Pad3, "Num 3", new KeyCode[] { 259 }),
			new KeyInfo(Key.Pad4, "Num 4", new KeyCode[] { 260 }),
			new KeyInfo(Key.Pad5, "Num 5", new KeyCode[] { 261 }),
			new KeyInfo(Key.Pad6, "Num 6", new KeyCode[] { 262 }),
			new KeyInfo(Key.Pad7, "Num 7", new KeyCode[] { 263 }),
			new KeyInfo(Key.Pad8, "Num 8", new KeyCode[] { 264 }),
			new KeyInfo(Key.Pad9, "Num 9", new KeyCode[] { 265 }),
			new KeyInfo(Key.Numlock, "Numlock", new KeyCode[] { 300 }),
			new KeyInfo(Key.PadDivide, "Pad Divide", new KeyCode[] { 267 }),
			new KeyInfo(Key.PadMultiply, "Pad Multiply", new KeyCode[] { 268 }),
			new KeyInfo(Key.PadMinus, "Pad Minus", new KeyCode[] { 269 }),
			new KeyInfo(Key.PadPlus, "Pad Plus", new KeyCode[] { 270 }),
			new KeyInfo(Key.PadEnter, "Pad Enter", new KeyCode[] { 271 }),
			new KeyInfo(Key.PadPeriod, "Pad Period", new KeyCode[] { 266 }),
			new KeyInfo(Key.Clear, "Clear", new KeyCode[] { 12 }),
			new KeyInfo(Key.PadEquals, "Pad Equals", new KeyCode[] { 272 }),
			new KeyInfo(Key.F13, "F13", new KeyCode[] { 294 }),
			new KeyInfo(Key.F14, "F14", new KeyCode[] { 295 }),
			new KeyInfo(Key.F15, "F15", new KeyCode[] { 296 }),
			new KeyInfo(Key.AltGr, "Alt Gr", new KeyCode[] { 313 }),
			new KeyInfo(Key.CapsLock, "Caps Lock", new KeyCode[] { 301 })
		};
	}
}
