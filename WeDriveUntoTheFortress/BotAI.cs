﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class BotAI {

		public Battlefield battlefield;

		public int pauseTimer;

		private Pathfinder pathfinder = new Pathfinder();

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
				float min = -1F;
				Tank closest = null;
				foreach(Tank t in battlefield.friendlyTanks) {
					float dist = (t.position - me.position).Length();
					if(min < 0 || dist < min) {
						min = dist;
						closest = t;
					}
				}
				if(closest != null) {
					List<Vector2> path = pathfinder.FindPath(me.position / Battlefield.tileSize, closest.position / Battlefield.tileSize, new Vector2(Battlefield.hTiles, Battlefield.vTiles));
					if(path != null && path.Count > 0) {
						Vector2 dir = me.position / Battlefield.tileSize - path[0];
						me.dir = me.gunDir = dir.X == 1 ? Tank.Dir.left : dir.X == -1 ? Tank.Dir.right : dir.Y == -1 ? Tank.Dir.down : Tank.Dir.up;
						battlefield.moving = true;
					}
				}
				else
					lookForTarget();
			}
		}

		public void lookForTarget() {
			if(pauseTimer > 0)
				pauseTimer--;
			else {
				pauseTimer = Program.game.rand.Next(50) + 50;
				if(battlefield.turn % 2 == 0) return;
				Tank me = battlefield.enemyTanks[(battlefield.turn / 2) % battlefield.enemyTanks.Count];
				for(int i = 0; i < 4; i++) {
					Tank.Dir dir = (Tank.Dir) i;
					Vector2 checkPos = me.position / Battlefield.tileSize + battlefield.dirToVector(dir);
					while(true) {
						if(checkPos.X < 0 || checkPos.Y < 0 || checkPos.X >= Battlefield.hTiles || checkPos.Y >= Battlefield.vTiles)
							break;
						if((checkPos - me.position / 32).Length() > 16)
							break;
						if(battlefield.map[(int) checkPos.X, (int) checkPos.Y] == MapObject.friendlyTank) {
							me.gunDir = dir;
							battlefield.shooting = true;
							battlefield.targetPos = battlefield.targetStart = me.position + battlefield.dirToVector(me.gunDir) * (Battlefield.tileSize / 2) + new Vector2(Battlefield.tileSize / 2, Battlefield.tileSize / 2);
							battlefield.targetDir = battlefield.dirToVector(me.gunDir);
							break;
						}
						checkPos += battlefield.dirToVector(dir);
					}
				}
			}
		}

		public bool finishedShooting() {
			if(battlefield.map[(int) battlefield.targetPos.X / 32, (int) battlefield.targetPos.Y / 32] == MapObject.friendlyTank)
				return true;
			return false;
		}
	}
}
