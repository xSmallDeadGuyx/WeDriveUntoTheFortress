using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class Battlefield {
		public static Texture2D grass;
		public static Texture2D box;

		public Viewport port;
		
		public static readonly int tileSize = 32;
		public static readonly int hTiles = 20;
		public static readonly int vTiles = 13;

		public Random rand = new Random();

		public MapObject[,] map;

		public Battlefield(MapObject[,] m, Viewport v) {
			map = m;
			port = v;
		}

		public void draw() {
			for(int i = 0; i < hTiles; i++) for(int j = 0; j < vTiles; j++) {
				port.draw(grass, new Vector2(i * tileSize, j * tileSize), Color.White);
				if(map[i, j] != MapObject.empty)
					port.draw(map[i, j] == MapObject.box ? box : grass, new Vector2(i * tileSize, j * tileSize), Color.White);
			}
		}

		public void onUpdate() {

		}
	}
}
