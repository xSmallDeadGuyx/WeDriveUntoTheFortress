using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WeDriveUntoTheFortress {
	public class Viewport {
		public SpriteBatch spriteBatch = Program.game.spriteBatch;
		public FontRenderer largeFont = Program.game.largeFont;
		public FontRenderer smallFont = Program.game.smallFont;

		public MouseState lastMouseState;
		public MouseState mouseState;

		public Rectangle port;

		public Viewport() {
			port = new Rectangle(0, 0, Program.game.width, Program.game.height);
		}

		public Viewport(Rectangle p) {
			port = p;
		}

		public Viewport(int x, int y, int width, int height) {
			port = new Rectangle(x, y, width, height);
		}

		public MouseState getMouse() {
			return new MouseState(mouseState.X - port.X, mouseState.Y - port.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
		}

		public MouseState getLastMouse() {
			return new MouseState(lastMouseState.X - port.X, lastMouseState.Y - port.Y, lastMouseState.ScrollWheelValue, lastMouseState.LeftButton, lastMouseState.MiddleButton, lastMouseState.RightButton, lastMouseState.XButton1, lastMouseState.XButton2);
		}

		public Vector2 addOffset(Vector2 v) {
			return new Vector2(v.X + port.X, v.Y + port.Y);
		}

		public Rectangle addOffset(Rectangle r) {
			return new Rectangle(r.X + port.X, r.Y + port.Y, r.Width, r.Height);
		}

		public void draw(Texture2D tex, Rectangle rect, Color c) {
			Rectangle newRect = addOffset(rect);
			spriteBatch.Draw(tex, newRect, c);
		}

		public void draw(Texture2D tex, Vector2 pos, Color c) {
			Vector2 newPos = addOffset(pos);
			spriteBatch.Draw(tex, newPos, c);
		}

		public void draw(Texture2D tex, Rectangle destinationRect, Rectangle sourceRect, Color c) {
			Rectangle newDestinationRect = addOffset(destinationRect);
			spriteBatch.Draw(tex, newDestinationRect, sourceRect, c);
		}

		public void draw(Texture2D tex, Vector2 pos, Rectangle sourceRect, Color c) {
			Vector2 newPos = addOffset(pos);
			spriteBatch.Draw(tex, newPos, sourceRect, c);
		}

		public void drawLargeString(string str, Vector2 pos, Color c) {
			Vector2 newPos = addOffset(pos);
			largeFont.DrawText(spriteBatch, (int) newPos.X, (int) newPos.Y, str, c);
		}

		public void drawSmallString(string str, Vector2 pos, Color c) {
			Vector2 newPos = addOffset(pos);
			smallFont.DrawText(spriteBatch, (int) newPos.X, (int) newPos.Y, str, c);
		}

		public void drawLargeStringCentered(string str, Vector2 pos, Color c) {
			Vector2 newPos = addOffset(pos) - new Vector2(largeFont.GetTextWidth(str) / 2, largeFont.GetMaxCharHeight(str) / 2);
			largeFont.DrawText(spriteBatch, (int) newPos.X, (int) newPos.Y, str, c);
		}

		public void drawSmallStringCentered(string str, Vector2 pos, Color c) {
			Vector2 newPos = addOffset(pos) - new Vector2(smallFont.GetTextWidth(str) / 2, smallFont.GetMaxCharHeight(str) / 2);
			smallFont.DrawText(spriteBatch, (int) newPos.X, (int) newPos.Y, str, c);
		}

		public void updateMouseState() {
			lastMouseState = mouseState;
			mouseState = Mouse.GetState();
		}
	}
}
