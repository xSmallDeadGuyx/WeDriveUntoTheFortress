using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WeDriveUntoTheFortress {
	public class Tank {
		public enum Dir { right, down, left, up };
		public Vector2 position;
		public Dir dir;
		public Dir gunDir;
		public int health = 100;

		public Tank(Vector2 p, Dir d) {
			position = p;
			dir = d;
			gunDir = d;
		}
	}

	public class Explosion {
		public static Texture2D texture;

		public Battlefield battlefield;
		public int frame = 0;
		public Vector2 pos;
		public int delay = 0;

		public Explosion(Vector2 p, Battlefield b) {
			pos = p;
			battlefield = b;
		}

		public void draw() {
			if(delay > 0) {
				delay--;
				return;
			}
			battlefield.port.draw(texture, pos - new Vector2(Battlefield.tileSize / 2, Battlefield.tileSize / 2), new Rectangle(Battlefield.tileSize * frame, 0, Battlefield.tileSize, Battlefield.tileSize), Color.White);
			frame++;
			if(Battlefield.tileSize * frame >= texture.Width)
				battlefield.endExplosion(this);
		}

		public override bool Equals(object obj) {
			if(obj == null || GetType() != obj.GetType())
				return false;
			Explosion e = (Explosion) obj;
			return e.pos.X == pos.X && e.pos.Y == pos.Y;
		}
	}

	public enum TankWeapon { cannon, clusterBomb };

	public class Battlefield {
		public static Texture2D grass;
		public static Texture2D box;

		public static Texture2D tankFriendly;
		public static Texture2D gunFriendly;
		public static Texture2D tankEnemy;
		public static Texture2D gunEnemy;

		public static Texture2D[] weaponTextures;

		public List<Explosion> explosions = new List<Explosion>();
		public List<Explosion> toRemove = new List<Explosion>();

		public Viewport port;
		
		public static readonly int tileSize = 32;
		public static readonly int hTiles = 20;
		public static readonly int vTiles = 13;

		public Random rand = new Random();

		public WeaponController[] controllers;

		public MapObject[,] map;

		public List<Tank> friendlyTanks = new List<Tank>();
		public List<Tank> enemyTanks = new List<Tank>();

		public int turn = 0;
		public int movesLeft = 5;
		public bool moving = false;
		public int nextTurnTimer = 0;
		public Tank beingMoved;

		public Vector2 lastPos;

		public bool shot = false;
		public bool shooting = false;
		public Vector2 targetPos;
		public Vector2 targetDir;
		public Vector2 targetStart;

		public bool is2Player = true;

		public TankWeapon[] weapon = { TankWeapon.clusterBomb, TankWeapon.cannon };

		public Battlefield(MapObject[,] m, Viewport v, bool p2) {
			map = m;
			port = v;
			is2Player = p2;
			controllers = new WeaponController[] { new CannonController(this), new ClusterController(this) };

			for(int i = 0; i < hTiles; i++) for(int j = 0; j < vTiles; j++)
				if(map[i, j] == MapObject.friendlyTank)
					friendlyTanks.Add(new Tank(new Vector2(i * tileSize, j * tileSize), Tank.Dir.right));
				else if(map[i, j] == MapObject.enemyTank)
					enemyTanks.Add(new Tank(new Vector2(i * tileSize, j * tileSize), Tank.Dir.left));
		}

		public Battlefield(MapObject[,] m, Viewport v) : this(m, v, false) {}

		public void createDelayedExplosion(int x, int y, int delay) {
			Explosion e = new Explosion(new Vector2(x, y), this);
			e.delay = delay;
			explosions.Add(e);
		}

		public void createExplosion(int x, int y) {
			explosions.Add(new Explosion(new Vector2(x, y), this));
		}

		public void endExplosion(Explosion e) {
			if(explosions.Contains(e))
				toRemove.Add(e);
		}

		public void draw() {
			WeaponController controller = controllers[(int) weapon[turn % 2]];
			for(int i = 0; i < hTiles; i++) for(int j = 0; j < vTiles; j++) {
				port.draw(grass, new Vector2(i * tileSize, j * tileSize), Color.White);
				if(map[i, j] == MapObject.box)
					port.draw(box, new Vector2(i * tileSize, j * tileSize), Color.White);
			}
			foreach(Tank t in friendlyTanks) {
				port.draw(tankFriendly, t.position, new Rectangle(t.dir == Tank.Dir.up || t.dir == Tank.Dir.down ? tileSize : 0, 0, tileSize, tileSize), Color.White);
				port.draw(gunFriendly, t.position, new Rectangle((int) t.gunDir * tileSize, 0, tileSize, tileSize), Color.White);
			}
			foreach(Tank t in enemyTanks) {
				port.draw(tankEnemy, t.position, new Rectangle(t.dir == Tank.Dir.up || t.dir == Tank.Dir.down ? tileSize : 0, 0, tileSize, tileSize), Color.White);
				port.draw(gunEnemy, t.position, new Rectangle((int) t.gunDir * tileSize, 0, tileSize, tileSize), Color.White);
			}

			foreach(Explosion e in explosions)
				e.draw();
			foreach(Explosion e in toRemove)
				explosions.Remove(e);
			toRemove.Clear();

			if(shooting)
				port.draw(weaponTextures[(int) weapon[turn % 2]], targetPos + controller.targetOffset, Color.White);
		}

		public Vector2 dirToVector(Tank.Dir d) {
			return new Vector2(d == Tank.Dir.left ? -1 : d == Tank.Dir.right ? 1 : 0, d == Tank.Dir.up ? -1 : d == Tank.Dir.down ? 1 : 0);
		}

		public Tank getTankAt(Vector2 tile) {
			foreach(Tank t in friendlyTanks)
				if(t.position.X / tileSize == tile.X && t.position.Y / tileSize == tile.Y)
					return t;
			foreach(Tank t in enemyTanks)
				if(t.position.X / tileSize == tile.X && t.position.Y / tileSize == tile.Y)
					return t;
			return null;
		}

		public void onUpdate() {
			KeyboardState keyboard = Keyboard.GetState();
			beingMoved = turn % 2 == 0 ? friendlyTanks[turn / 2] : enemyTanks[turn / 2];
			WeaponController controller = controllers[(int) weapon[turn % 2]];
			if(movesLeft == 0) {
				movesLeft = 5;
				nextTurnTimer = shot ? 90 : 300;
				moving = false;
			}
			else if(nextTurnTimer > 0) {
				nextTurnTimer--;
				if(nextTurnTimer == 0) {
					moving = false;
					shot = false;
					shooting = false;
					turn++;
				}
			}
			else if(!moving && !shooting) {
				if(turn % 2 == 0 || is2Player) {
					if(keyboard.IsKeyDown(Keys.Up) && beingMoved.position.Y / tileSize > 0) {
						moving = true;
						beingMoved.dir = Tank.Dir.up;
					}
					else if(keyboard.IsKeyDown(Keys.Right) && beingMoved.position.X / tileSize < hTiles - 1) {
						moving = true;
						beingMoved.dir = Tank.Dir.right;
					}
					else if(keyboard.IsKeyDown(Keys.Down) && beingMoved.position.Y / tileSize < vTiles - 1) {
						moving = true;
						beingMoved.dir = Tank.Dir.down;
					}
					else if(keyboard.IsKeyDown(Keys.Left) && beingMoved.position.X / tileSize > 0) {
						moving = true;
						beingMoved.dir = Tank.Dir.left;
					}

					if(moving)
						lastPos = beingMoved.position / tileSize;
					else if(!shot && keyboard.IsKeyDown(Keys.Space)) {
						shooting = true;
						targetPos = targetStart = beingMoved.position + dirToVector(beingMoved.gunDir) * (tileSize / 2);
						targetDir = dirToVector(beingMoved.gunDir);
					}
				}
			}
			else if(!shooting) {
				beingMoved.position += 2 * dirToVector(beingMoved.dir);
				if(beingMoved.position.X % tileSize == 0 && beingMoved.position.Y % tileSize == 0) {
					map[(int) lastPos.X, (int) lastPos.Y] = MapObject.empty;
					map[(int) beingMoved.position.X / tileSize, (int) beingMoved.position.Y / tileSize] = turn % 2 == 0 ? MapObject.friendlyTank : MapObject.enemyTank;
					movesLeft--;
					moving = false;
				}
			}
			else if(keyboard.IsKeyDown(Keys.Space)) {
				targetPos += controller.targetSpeed * targetDir;
				Vector2 offset = targetPos - targetStart;
				double dist = offset.Length() / (double) tileSize;
				if(dist >= controller.range || offset.X * dirToVector(beingMoved.gunDir).X < 0 || offset.Y * dirToVector(beingMoved.gunDir).Y < 0)
					targetDir *= -1;
			}
			else {
				shooting = false;
				shot = true;
				movesLeft = 1;
				int hitRange = (int) Math.Sqrt(Math.Pow((targetPos.X - beingMoved.position.X) / (double) tileSize, 2) + Math.Pow((targetPos.Y - beingMoved.position.Y) / (double) tileSize, 2));
				Vector2 dir = dirToVector(beingMoved.gunDir);
				Vector2 checkPos = beingMoved.position / tileSize;
				while(checkPos.Length() < hitRange - 1 && (map[(int) checkPos.X, (int) checkPos.Y] == MapObject.empty || (map[(int) checkPos.X, (int) checkPos.Y] == MapObject.box && controller.penetratesBoxes) || ((map[(int) checkPos.X, (int) checkPos.Y] == MapObject.friendlyTank || map[(int) checkPos.X, (int) checkPos.Y] == MapObject.enemyTank) && (controller.penetratesTanks || beingMoved.Equals(getTankAt(checkPos))))))
					checkPos += dir;
				switch(map[(int) checkPos.X, (int) checkPos.Y]) {
					case MapObject.empty:
					case MapObject.box:
						controller.onHitBox((int) checkPos.X, (int) checkPos.Y);
						break;
					case MapObject.friendlyTank:
					case MapObject.enemyTank:
						Tank t = getTankAt(checkPos);
						if(t != null && !t.Equals(beingMoved))
							controller.onHitTank(t, t.position - dirToVector(beingMoved.dir) * tileSize / 2);
						break;
				}
			}

			if(!shot && !shooting && (turn % 2 == 0 || is2Player)) {
				if(keyboard.IsKeyDown(Keys.W))
					beingMoved.gunDir = Tank.Dir.up;
				else if(keyboard.IsKeyDown(Keys.D))
					beingMoved.gunDir = Tank.Dir.right;
				else if(keyboard.IsKeyDown(Keys.S))
					beingMoved.gunDir = Tank.Dir.down;
				else if(keyboard.IsKeyDown(Keys.A))
					beingMoved.gunDir = Tank.Dir.left;
			}
		}
	}
}
