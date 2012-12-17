using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class FontRenderer {
		public FontRenderer(FontFile fontFile, Texture2D fontTexture) {
			_fontFile = fontFile;
			_texture = fontTexture;
			_characterMap = new Dictionary<char, FontChar>();

			foreach(var fontCharacter in _fontFile.Chars) {
				char c = (char) fontCharacter.ID;
				_characterMap.Add(c, fontCharacter);
			}
		}

		private Dictionary<char, FontChar> _characterMap;
		private FontFile _fontFile;
		private Texture2D _texture;
		public void DrawText(SpriteBatch spriteBatch, int x, int y, string text, Color col) {
			int dx = x;
			int dy = y;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc)) {
					var sourceRectangle = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
					var position = new Vector2(dx + fc.XOffset, dy + fc.YOffset);

					spriteBatch.Draw(_texture, position, sourceRectangle, col);
					dx += fc.XAdvance;
				}
			}
		}

		public int GetTextWidth(string text) {
			int width = 0;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc))
					width += fc.XAdvance;
			}
			return width;
		}

		public int GetMaxCharHeight(string text) {
			int maxHeight = 0;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc))
					if(fc.Height > maxHeight)
						maxHeight = fc.Height;
			}
			return maxHeight;
		}
	}
}
