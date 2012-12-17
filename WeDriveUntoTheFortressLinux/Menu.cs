using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WeDriveUntoTheFortress {
	public abstract class MenuControl {
		public int id;
		public Rectangle dimensions;
		public bool enabled = true;
		public abstract void onUpdate();
		public abstract void draw();
	}
	 
	public enum MenuButtonState{up, over, down};
	public class MenuButton : MenuControl {
		public static Texture2D button_up;
		public static Texture2D button_over;
		public static Texture2D button_down;

		public Menu sourceMenu;
		public string text;

		private MenuButtonState state;

		public MenuButton(Menu menu, int i, string t, Vector2 pos) : this(menu, i, t, new Rectangle((int) pos.X, (int) pos.Y, 256, 64)) { }

		public MenuButton(Menu menu, int i, string t, Rectangle size) {
			sourceMenu = menu;
			id = i;
			text = t;
			dimensions = size;
		}

		public override void onUpdate() {
			MouseState mouse = sourceMenu.viewport.getMouse();
			if(enabled) {
				if(mouse.X - sourceMenu.viewport.port.X > dimensions.X && mouse.X - sourceMenu.viewport.port.X < dimensions.X + dimensions.Width && mouse.Y - sourceMenu.viewport.port.Y > dimensions.Y && mouse.Y - sourceMenu.viewport.port.Y < dimensions.Y + dimensions.Height) {
					if(mouse.LeftButton == ButtonState.Pressed)
						state = MenuButtonState.down;
					else {
						if(sourceMenu.viewport.getLastMouse() != null && sourceMenu.viewport.getLastMouse().LeftButton == ButtonState.Pressed)
							sourceMenu.performEvent(id);
						state = MenuButtonState.over;
					}
				}
				else
					state = MenuButtonState.up;
			}
		}

		public override void draw() {
			if(enabled) {
				sourceMenu.viewport.draw(state == MenuButtonState.up ? button_up : state == MenuButtonState.down ? button_down : button_over, dimensions, Color.White);
				sourceMenu.viewport.drawLargeStringCentered(text, new Vector2(dimensions.X + dimensions.Width / 2, dimensions.Y + dimensions.Height / 2), Color.Black);
			}
			else {
				sourceMenu.viewport.draw(button_up, dimensions, Color.LightGray);
				sourceMenu.viewport.drawLargeStringCentered(text, new Vector2(dimensions.X + dimensions.Width / 2, dimensions.Y + dimensions.Height / 2), Color.Black);
			}
		}
	}

	public class Menu {
		public Viewport viewport;
		public List<MenuControl> controls = new List<MenuControl>();
		public delegate void PerformEvent(int id);
		public PerformEvent performEvent;

		public Menu(Viewport v) {
			viewport = v;
		}

		public void onUpdate() {
			viewport.updateMouseState();
			foreach(MenuControl c in controls)
				c.onUpdate();
		}

		public void draw() {
			foreach(MenuControl c in controls)
				c.draw();
		}
	}
}
