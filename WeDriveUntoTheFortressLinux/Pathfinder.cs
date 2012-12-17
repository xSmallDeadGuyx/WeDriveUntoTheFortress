using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class Pathfinder {

		public List<Vector2> FindPath(Vector2 start, Vector2 end, Vector2 size) {
			AStar finder = new AStar(start, end, size);
			return finder.Generate();
		}
	}

	internal class AStar {
		private Vector2 start;
		private Vector2 end;
		private int[,] gScore;
		private int[,] hScore;
		private int[,] fScore;
		private Vector2[,] cameFrom;

		public AStar(Vector2 s, Vector2 e, Vector2 d) {
			start = s;
			end = e;

			gScore = new int[(int) d.X, (int) d.Y];
			hScore = new int[(int) d.X, (int) d.Y];
			fScore = new int[(int) d.X, (int) d.Y];
			cameFrom = new Vector2[(int) d.X, (int) d.Y];
		}

		private int calculateHeuristic(Vector2 pos) {
			switch(Program.game.battlefield.map[(int) pos.X, (int) pos.Y]) {
				case MapObject.deadTank:
				case MapObject.box:
					return 50;
				case MapObject.empty:
					return 10;
			}
			return 1000000;
		}

		private int distanceBetween(Vector2 pos1, Vector2 pos2) {
			return (int) Math.Round(10 * Math.Sqrt(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2)));
		}

		private Vector2 getLowestPointIn(List<Vector2> list) {
			int lowest = -1;
			Vector2 found = new Vector2(-1, -1);
			foreach(Vector2 p in list) {
				int dist = cameFrom[(int) p.X, (int) p.Y] == new Vector2(-1, -1) ? 0 : gScore[(int) cameFrom[(int) p.X, (int) p.Y].X, (int) cameFrom[(int) p.X, (int) p.Y].Y] + distanceBetween(p, cameFrom[(int) p.X, (int) p.Y]) + calculateHeuristic(p);
				if(dist <= lowest || lowest == -1) {
					lowest = dist;
					found = p;
				}
			}
			return found;
		}

		private bool canMoveTo(int x, int y) {
			if(Program.game.battlefield.canMoveTo(x, y)) return true;
			if(x >= Battlefield.hTiles || x < 0 || y < 0 || y >= Battlefield.vTiles) return false;
			if(Program.game.battlefield.map[x, y] == MapObject.deadTank || Program.game.battlefield.map[x, y] == MapObject.box) return true;
			return end.X == x && end.Y == y;
		}

		private List<Vector2> getNeighbourPoints(Vector2 p) {
			List<Vector2> found = new List<Vector2>();
			if(canMoveTo((int) p.X + 1, (int) p.Y)) found.Add(new Vector2((int) p.X + 1, (int) p.Y));
			if(canMoveTo((int) p.X - 1, (int) p.Y)) found.Add(new Vector2((int) p.X - 1, (int) p.Y));
			if(canMoveTo((int) p.X, (int) p.Y + 1)) found.Add(new Vector2((int) p.X, (int) p.Y + 1));
			if(canMoveTo((int) p.X, (int) p.Y - 1)) found.Add(new Vector2((int) p.X, (int) p.Y - 1));
			return found;
		}

		private List<Vector2> reconstructPath(Vector2 p) {
			if(p != start) {
				List<Vector2> path = reconstructPath(cameFrom[(int) p.X, (int) p.Y]);
				path.Add(p);
				return path;
			}
			else
				return new List<Vector2>();
		}

		public List<Vector2> Generate() {
			List<Vector2> open = new List<Vector2>();
			List<Vector2> closed = new List<Vector2>();

			open.Add(start);
			gScore[(int) start.X, (int) start.Y] = 0;
			hScore[(int) start.X, (int) start.Y] = calculateHeuristic(start);
			fScore[(int) start.X, (int) start.Y] = hScore[(int) start.X, (int) start.Y];

			while(open.Count > 0) {
				Vector2 point = getLowestPointIn(open);
				if(point == end) return reconstructPath(cameFrom[(int) point.X, (int) point.Y]);
				open.Remove(point);
				closed.Add(point);

				List<Vector2> neighbours = getNeighbourPoints(point);
				foreach(Vector2 p in neighbours) {
					if(closed.Contains(p)) continue;

					int gPossible = gScore[(int) point.X, (int) point.Y] + distanceBetween(p, point);

					if(!open.Contains(p) || (open.Contains(p) && gPossible < gScore[(int) p.X, (int) p.Y])) {
						if(!open.Contains(p)) open.Add(p);
						cameFrom[(int) p.X, (int) p.Y] = point;
						gScore[(int) p.X, (int) p.Y] = gPossible;
						hScore[(int) p.X, (int) p.Y] = calculateHeuristic(p);
						fScore[(int) p.X, (int) p.Y] = gPossible + hScore[(int) p.X, (int) p.Y];
					}
				}
			}
			return null;
		}
	}
}