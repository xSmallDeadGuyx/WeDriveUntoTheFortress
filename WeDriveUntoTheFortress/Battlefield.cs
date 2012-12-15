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
		
		public readonly int tileSize = 32;
		public int hTiles = 20;
		public int vTiles = 13;

		public Random rand = new Random();

		public bool[,] map;

		public Battlefield(Viewport v) {
			map = new bool[hTiles, vTiles];
			for(int i = 0; i < hTiles; i++) for(int j = 0; j < vTiles; j++)
				if(rand.Next(20) == 0)
					map[i, j] = true;
			
			port = v;
		}

		public void draw() {
			for(int i = 0; i < hTiles; i++) for(int j = 0; j < vTiles; j++)
				port.draw(map[i, j] ? box : grass, new Vector2(i * tileSize, j * tileSize), Color.White);
		}
	}
}
