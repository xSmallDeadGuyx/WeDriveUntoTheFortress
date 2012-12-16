using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class BotAI {

		public Battlefield battlefield;

		public int pauseTimer;

		public int shootRequests = 0;

		public BotAI(Battlefield b) {
			battlefield = b;
			pauseTimer = Program.game.rand.Next(50) + 50;
		}

		public void moveRequestAvailable() {
			if(pauseTimer > 0)
				pauseTimer--;
			else {
				pauseTimer = Program.game.rand.Next(50) + 50;
				if(battlefield.turn % 2 == 0) return;
				Tank me = battlefield.enemyTanks[(battlefield.turn / 2) % battlefield.enemyTanks.Count];
				bool[] check = { false, false, false, false };
				bool moved = false;
				while(!moved && (!check[0] || !check[1] || !check[2] || !check[3])) {
					int n = Program.game.rand.Next(4);
					check[n] = true;
					Vector2 nextPos = me.position / 32 + battlefield.dirToVector((Tank.Dir) n);
					if(battlefield.canMoveTo((int) nextPos.X, (int) nextPos.Y)) {
						Console.WriteLine("(" + me.position.X / 32 + ", " + me.position.Y / 32 + ")\t" + (Tank.Dir) n + "\t(" + nextPos.X + ", " + nextPos.Y + ")");
						me.dir = me.gunDir = (Tank.Dir) n;
						battlefield.moving = true;
						moved = true;
					}
				}
				if(!moved)
					startShoot();
			}
		}

		public void startShoot() {
			if(pauseTimer > 0)
				pauseTimer--;
			else {
				pauseTimer = Program.game.rand.Next(50) + 50;
				if(battlefield.turn % 2 == 0) return;
				Tank me = battlefield.enemyTanks[(battlefield.turn / 2) % battlefield.enemyTanks.Count];
				foreach(Tank t in battlefield.friendlyTanks) {
					if(t.position.X == me.position.X || t.position.Y == me.position.Y) {
						Vector2 offset = me.position - t.position;
						if(offset.Length() < 16) continue;
						Vector2 dir = offset;
						dir.Normalize();
						bool obstruction = false;
						for(int i = 1; i < offset.Length(); i++) {
							Vector2 pos = t.position / 32 + dir * i;
							if(battlefield.map[(int) pos.X, (int) pos.Y] != MapObject.empty) {
								obstruction = true;
								break;
							}
						}
						if(!obstruction) {
							Tank.Dir targetDir = dir.X == 1 ? Tank.Dir.left : dir.X == -1 ? Tank.Dir.right : dir.Y == -1 ? Tank.Dir.down : Tank.Dir.up;
							me.gunDir = targetDir;
							battlefield.shooting = true;
							battlefield.targetPos = battlefield.targetStart = me.position + battlefield.dirToVector(me.gunDir) * (Battlefield.tileSize / 2) + new Vector2(Battlefield.tileSize / 2, Battlefield.tileSize / 2);
							battlefield.targetDir = battlefield.dirToVector(me.gunDir);
							return;
						}
					}
				}
				me.gunDir = (Tank.Dir) Program.game.rand.Next(4);
				battlefield.shooting = true;
				battlefield.targetPos = battlefield.targetStart = me.position + battlefield.dirToVector(me.gunDir) * (Battlefield.tileSize / 2) + new Vector2(Battlefield.tileSize / 2, Battlefield.tileSize / 2);
				battlefield.targetDir = battlefield.dirToVector(me.gunDir);
			}
		}

		public bool finishedShooting() {
			shootRequests++;
			if(shootRequests > 300) {
				shootRequests = 0;
				return true;
			}
			if((battlefield.targetPos - battlefield.targetStart).Length() / 32 > 10)
				return true;
			return false;
		}
	}
}
