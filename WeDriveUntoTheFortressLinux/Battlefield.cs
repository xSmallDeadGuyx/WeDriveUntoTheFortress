using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

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
				if(delay == 0)
					Battlefield.explosion.Play();
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

		public static Texture2D turnIndicator;

		public static Texture2D[] weaponTextures;

		public static Texture2D youWin;
		public static Texture2D youLose;
		public static Texture2D p1Win;
		public static Texture2D p2Win;

		public static SoundEffectInstance explosion;

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
		public int nextTurnTimer = 1800;
		public Tank beingMoved;

		public Vector2 lastPos;

		public bool shot = false;
		public bool shooting = false;
		public Vector2 targetPos;
		public Vector2 targetDir;
		public Vector2 targetStart;

		public BotAI botAI;
		public bool is2Player = false;
		public bool showWinner = false;
		public int winTimer = 0;

		public int storyTimer = 0;

		public TankWeapon[] weapon = { TankWeapon.cannon, TankWeapon.cannon };

		public Battlefield(MapObject[,] m, Viewport v, bool p2) {
			map = m;
			port = v;
			is2Player = p2;

			Program.game.menuMusic.Stop();
			Program.game.gameMusic.Play();

			if(!p2) {
				botAI = new BotAI(this);
				storyTimer = 600;
			}
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
			explosion.Play();
		}

		public void endExplosion(Explosion e) {
			if(explosions.Contains(e))
				toRemove.Add(e);
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

		public bool canMoveTo(int x, int y) {
			if(x < 0 || y < 0 || x >= hTiles || y >= vTiles) return false;
			return map[x, y] == MapObject.empty;
		}

		public void onUpdate() {
			KeyboardState keyboard = Keyboard.GetState();
			if(storyTimer > 0) {
				if(keyboard.IsKeyDown(Keys.Enter))
					storyTimer = 0;
			}
			else {
				if(!showWinner) {
					List<Tank> deadTanks = new List<Tank>();
					foreach(Tank t in friendlyTanks)
						if(t.health <= 0) {
							deadTanks.Add(t);
							map[(int) t.position.X / tileSize, (int) t.position.Y / tileSize] = MapObject.deadTank;
						}
					foreach(Tank t in enemyTanks)
						if(t.health <= 0) {
							deadTanks.Add(t);
							map[(int) t.position.X / tileSize, (int) t.position.Y / tileSize] = MapObject.deadTank;
						}
					foreach(Tank t in deadTanks) {
						if(friendlyTanks.Contains(t)) friendlyTanks.Remove(t);
						if(enemyTanks.Contains(t)) enemyTanks.Remove(t);
					}
					if(friendlyTanks.Count == 0 || enemyTanks.Count == 0)
						showWinner = true;

					beingMoved = turn % 2 == 0 ? friendlyTanks[(turn / 2) % friendlyTanks.Count] : enemyTanks[(turn / 2) % enemyTanks.Count];

					WeaponController controller = controllers[(int) weapon[turn % 2]];

					if(nextTurnTimer > 0) {
						nextTurnTimer--;
						if(nextTurnTimer == 0) {
							movesLeft = 5;
							moving = false;
							shot = false;
							shooting = false;
							turn++;
							nextTurnTimer = 1800;
						}
					}

					if(movesLeft == 0 && nextTurnTimer > (shot ? 90 : 300)) {
						nextTurnTimer = shot ? 90 : 300;
						moving = false;
					}
					else if(!moving && !shooting) {
						lastPos = beingMoved.position / tileSize;
						if(turn % 2 == 0 || is2Player) {
							if(movesLeft > 0) {
								if(keyboard.IsKeyDown(Keys.Up) && canMoveTo((int) beingMoved.position.X / tileSize, (int) beingMoved.position.Y / tileSize - 1)) {
									moving = true;
									beingMoved.dir = Tank.Dir.up;
								}
								else if(keyboard.IsKeyDown(Keys.Right) && canMoveTo((int) beingMoved.position.X / tileSize + 1, (int) beingMoved.position.Y / tileSize)) {
									moving = true;
									beingMoved.dir = Tank.Dir.right;
								}
								else if(keyboard.IsKeyDown(Keys.Down) && canMoveTo((int) beingMoved.position.X / tileSize, (int) beingMoved.position.Y / tileSize + 1)) {
									moving = true;
									beingMoved.dir = Tank.Dir.down;
								}
								else if(keyboard.IsKeyDown(Keys.Left) && canMoveTo((int) beingMoved.position.X / tileSize - 1, (int) beingMoved.position.Y / tileSize)) {
									moving = true;
									beingMoved.dir = Tank.Dir.left;
								}
							}

							if(!moving && !shot && keyboard.IsKeyDown(Keys.Space)) {
								shooting = true;
								targetPos = targetStart = beingMoved.position + dirToVector(beingMoved.gunDir) * (tileSize / 2) + new Vector2(tileSize / 2, tileSize / 2);
								targetDir = dirToVector(beingMoved.gunDir);
							}
						}
						else {
							if(movesLeft > 0)
								botAI.moveRequestAvailable();
							if(!moving && !shooting && !shot)
								botAI.lookForTarget();
						}
					}
					else if(!shooting) {
						beingMoved.position += 2 * dirToVector(beingMoved.dir);
						if((beingMoved.position.X % tileSize == 0 && beingMoved.position.Y % tileSize == 0) || (beingMoved.position - lastPos * tileSize).Length() > tileSize) {
							beingMoved.position.X = ((int) (beingMoved.position.X / tileSize)) * tileSize;
							beingMoved.position.Y = ((int) (beingMoved.position.Y / tileSize)) * tileSize;
							map[(int) lastPos.X, (int) lastPos.Y] = MapObject.empty;
							map[(int) beingMoved.position.X / tileSize, (int) beingMoved.position.Y / tileSize] = turn % 2 == 0 ? MapObject.friendlyTank : MapObject.enemyTank;
							movesLeft--;
							moving = false;
						}
					}
					else if((keyboard.IsKeyDown(Keys.Space) && (turn % 2 == 0 || is2Player)) || (turn % 2 == 1 && !is2Player && !botAI.finishedShooting())) {
						targetPos += controller.targetSpeed * targetDir;
						Vector2 offset = targetPos - targetStart;
						double dist = offset.Length() / (double) tileSize;
						if(dist >= controller.range || offset.X * dirToVector(beingMoved.gunDir).X < 0 || offset.Y * dirToVector(beingMoved.gunDir).Y < 0)
							targetDir *= -1;

						if(targetPos.X < 0) {
							targetPos.X = 0;
							targetDir.X = Math.Abs(targetDir.X);
						}
						if(targetPos.X > tileSize * hTiles - 1) {
							targetPos.X = tileSize * hTiles - 1;
							targetDir.X = -Math.Abs(targetDir.X);
						}
						if(targetPos.Y < 0) {
							targetPos.Y = 0;
							targetDir.Y = Math.Abs(targetDir.Y);
						}
						if(targetPos.Y > tileSize * vTiles - 1) {
							targetPos.Y = tileSize * vTiles - 1;
							targetDir.Y = -Math.Abs(targetDir.Y);
						}
					}
					else {
						shooting = false;
						shot = true;
						if(movesLeft > 0) movesLeft = 1;
						nextTurnTimer = 180;
						Vector2 dir = dirToVector(beingMoved.gunDir);
						Vector2 checkPos = beingMoved.position / tileSize + dir;
						bool checking = true;
						bool hit = false;
						int maxRange = (int) (targetPos - targetStart).Length() / tileSize;
						while(checking) {
							if(checkPos.X < 0 || checkPos.Y < 0 || checkPos.X >= hTiles || checkPos.Y >= vTiles)
								break;
							if((checkPos - beingMoved.position / 32).Length() > maxRange)
								break;
							switch(map[(int) checkPos.X, (int) checkPos.Y]) {
								case MapObject.deadTank:
								case MapObject.box:
									checking = controller.penetratesBoxes;
									if(!checking) {
										hit = true;
										controller.onHitBox((int) checkPos.X, (int) checkPos.Y);
									}
									break;
								case MapObject.enemyTank:
								case MapObject.friendlyTank:
									Tank t = getTankAt(checkPos);
									if(t != null) {
										checking = controller.penetratesTanks;
										if(!checking) {
											hit = true;
											controller.onHitTank(t, checkPos * tileSize + new Vector2(Battlefield.tileSize / 2, Battlefield.tileSize / 2) - Battlefield.tileSize / 2 * dirToVector(beingMoved.gunDir));
										}
									}
									break;
							}
							checkPos += dir;
						}
						if(!hit)
							switch(map[(int) checkPos.X, (int) checkPos.Y]) {
								case MapObject.empty:
									controller.onHitNothing((int) checkPos.X, (int) checkPos.Y);
									break;
								case MapObject.deadTank:
								case MapObject.box:
									controller.onHitBox((int) checkPos.X, (int) checkPos.Y);
									break;
								case MapObject.enemyTank:
								case MapObject.friendlyTank:
									Tank t = getTankAt(checkPos);
									if(t != null)
										controller.onHitTank(t, checkPos * tileSize + new Vector2(Battlefield.tileSize / 2, Battlefield.tileSize / 2) - Battlefield.tileSize / 2 * dirToVector(beingMoved.gunDir));
									break;
							}
					}

					if(!shooting && (turn % 2 == 0 || is2Player)) {
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

		public bool draw() {
			if(storyTimer > 0) {
				port.draw(Program.game.story[Program.game.selectedLevel], new Vector2(0, -32), Color.White);
				return true;
			}
			else {
				WeaponController controller = controllers[(int) weapon[turn % 2]];
				for(int i = 0; i < hTiles; i++) for(int j = 0; j < vTiles; j++) {
						port.draw(grass, new Vector2(i * tileSize, j * tileSize), Color.White);
						if(map[i, j] == MapObject.box)
							port.draw(box, new Vector2(i * tileSize, j * tileSize), Color.White);
						if(map[i, j] == MapObject.deadTank)
							port.draw(tankFriendly, new Vector2(i * tileSize, j * tileSize), new Rectangle(0, 0, tileSize, tileSize), Color.Black);
					}
				foreach(Tank t in friendlyTanks) {
					float c = t.health / 100.0F;
					port.draw(tankFriendly, t.position, new Rectangle(t.dir == Tank.Dir.up || t.dir == Tank.Dir.down ? tileSize : 0, 0, tileSize, tileSize), new Color(c, c, c));
					port.draw(gunFriendly, t.position, new Rectangle((int) t.gunDir * tileSize, 0, tileSize, tileSize), new Color(c, c, c));
				}
				foreach(Tank t in enemyTanks) {
					float c = t.health / 100.0F;
					port.draw(tankEnemy, t.position, new Rectangle(t.dir == Tank.Dir.up || t.dir == Tank.Dir.down ? tileSize : 0, 0, tileSize, tileSize), new Color(c, c, c));
					port.draw(gunEnemy, t.position, new Rectangle((int) t.gunDir * tileSize, 0, tileSize, tileSize), new Color(c, c, c));
				}

				Tank current = turn % 2 == 0 ? friendlyTanks[(turn / 2) % friendlyTanks.Count] : enemyTanks[(turn / 2) % enemyTanks.Count];
				port.draw(turnIndicator, current.position - new Vector2(8, 8), Color.White);
				if(movesLeft > 0) port.drawSmallStringCentered("" + movesLeft, current.position + new Vector2(tileSize / 2, tileSize + 12), Color.Black);
				port.drawSmallStringCentered("" + (int) Math.Ceiling(nextTurnTimer / 60.0D), new Vector2(Program.game.width / 2, 12), nextTurnTimer < (shot ? 90 : 300) ? Color.DarkRed : Color.Black);

				foreach(Explosion e in explosions)
					e.draw();
				foreach(Explosion e in toRemove)
					explosions.Remove(e);
				toRemove.Clear();

				if(shooting)
					port.draw(weaponTextures[(int) weapon[turn % 2]], targetPos + controller.targetOffset, Color.White);
				return false;
			}
		}

		public void drawHUD() {
			if(storyTimer == 0) {
				port.drawSmallStringCentered("Turn " + turn, new Vector2(Program.game.width / 2, -16), Color.Black);

				for(int i = 0; i < friendlyTanks.Count; i++) {
					Tank t = friendlyTanks[i];
					float c = t.health / 100.0F;
					port.draw(tankFriendly, new Vector2(128 + i * 36 - tileSize / 2, -32), new Rectangle(0, 0, tileSize, tileSize), new Color(c, c, c));
					port.draw(gunFriendly, new Vector2(128 + i * 36 - tileSize / 2, -32), new Rectangle(0, 0, tileSize, tileSize), new Color(c, c, c));
				}

				for(int i = 0; i < enemyTanks.Count; i++) {
					Tank t = enemyTanks[i];
					float c = t.health / 100.0F;
					port.draw(tankEnemy, new Vector2(Program.game.width - (128 + i * 36) - tileSize / 2, -32), new Rectangle(0, 0, tileSize, tileSize), new Color(c, c, c));
					port.draw(gunEnemy, new Vector2(Program.game.width - (128 + i * 36) - tileSize / 2, -32), new Rectangle(tileSize * 2, 0, tileSize, tileSize), new Color(c, c, c));
				}

				if(showWinner) {
					winTimer++;
					if(winTimer > 300) {
						if(!is2Player) {
							if(enemyTanks.Count == 0) {
								Program.game.saveData.levelsComplete[Program.game.selectedLevel] = true;
								if(Program.game.selectedLevel < Program.game.levelData.length - 1) Program.game.selectedLevel++;
								Program.game.saveData.saveDataToFile();
							}
							Program.game.gameState = WeDriveUntoTheFortress.GameState.levelSelect;
						}
						else
							Program.game.gameState = WeDriveUntoTheFortress.GameState.mainMenu;
					}
					port.draw(is2Player ? (friendlyTanks.Count == 0 ? p2Win : p1Win) : (friendlyTanks.Count == 0 ? youLose : youWin), new Vector2(0, -32), Color.White);
				}
			}
		}
	}
}