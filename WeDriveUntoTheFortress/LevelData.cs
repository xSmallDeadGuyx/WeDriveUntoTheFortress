using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeDriveUntoTheFortress {
	public enum MapObject { empty, box, friendlyTank, enemyTank, deadTank };

	public class LevelData {
		private string[] levelData = {
			"......................#..............#....#..............#...F#..............#E.............................####................####................####.............................F#..............#E...#..............#....#..............#......................",
			"##................###.................E#......................................E..F#...##....#..###....#...#.#..#.#.#.....F#...#.#....#.##.....#...#.#...#....#...F###.##...###.##.....................E.....................#.................E###................##"
		};

		public int length {
			get {
				return levelData.Length;
			}
		}
		public MapObject[,] this[int index] {
			get {
				MapObject[,] data = new MapObject[Battlefield.hTiles, Battlefield.vTiles];
				for(int j = 0; j < Battlefield.vTiles; j++)
					for(int i = 0; i < Battlefield.hTiles; i++) {
						char c = levelData[index][i + j * Battlefield.hTiles];
						switch(c) {
							case '#':
								data[i, j] = MapObject.box;
								break;
							case 'F':
								data[i, j] = MapObject.friendlyTank;
								break;
							case 'E':
								data[i, j] = MapObject.enemyTank;
								break;
						}
					}
				return data;
			}
		}
	}
}
