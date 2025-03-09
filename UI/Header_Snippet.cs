﻿using Microsoft.Xna.Framework;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Header_Snippet_Handler : ITagHandler {
		public class Header_Snippet(string text, Color color = default) : TextSnippet(text.Replace(" ", "  "), color, 1.1f) { }
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new Header_Snippet(text, baseColor);
		}
	}
}
