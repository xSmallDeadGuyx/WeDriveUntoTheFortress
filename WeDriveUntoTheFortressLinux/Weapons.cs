using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class WeaponController {

		public Battlefield battlefield;

		public virtual void onHitTank(Tank t, Vector2 point) {
			battlefield.createExplosion((int) point.X, (int) point.Y);
		}
		public virtual void onHitBox(int x, int y) {
			battlefield.map[x, y] = MapObject.empty;
			battlefield.createExplosion(x * Battlefield.tileSize + Battlefield.tileSize / 2, y * Battlefield.tileSize + Battlefield.tileSize / 2);

		}
		public virtual void onHitNothing(int x, int y) {
			battlefield.createExplosion(x * Battlefield.tileSize + Battlefield.tileSize / 2, y * Battlefield.tileSize + Battlefield.tileSize / 2);
		}

		public bool penetratesTanks = true;
		public bool penetratesBoxes = false;

		public int range = 16;
		public int targetSpeed = 6;
		public Vector2 targetOffset = new Vector2(-16, -16);

		public WeaponController(Battlefield b) {
			battlefield = b;
		}
	}

	public class CannonController : WeaponController {

		public CannonController(Battlefield b) : base(b) { }

		public override void onHitTank(Tank t, Vector2 point) {
			t.health -= 40;
			battlefield.createExplosion((int) point.X, (int) point.Y);
		}
	}

	public class ClusterController : WeaponController {

		public ClusterController(Battlefield b) : base(b) {
			targetOffset = new Vector2(-24, -24);
			targetSpeed = 8;
			range = 12;
		}

		public override void onHitTank(Tank t, Vector2 point) {
			t.health -= 18;

			battlefield.createExplosion((int) point.X, (int) point.Y);
			int dist = 30;
			for(int i = 0; i < 9; i++) {
				double x = dist * Math.Cos(i * MathHelper.PiOver4);
				double y = dist * Math.Sin(i * MathHelper.PiOver4);
				battlefield.createDelayedExplosion((int) (point.X + x), (int) (point.Y + y), 3);
			}

			foreach(Tank t2 in battlefield.friendlyTanks)
				if(Math.Abs(t2.position.X - t.position.X) / Battlefield.tileSize == 1 && Math.Abs(t2.position.Y - t.position.Y) / Battlefield.tileSize == 1) //diagonal
					t2.health -= 7;
				else if(Math.Abs(t2.position.X - t.position.X) / Battlefield.tileSize <= 1 && Math.Abs(t2.position.Y - t.position.Y) / Battlefield.tileSize <= 1) //next to
					t2.health -= 11;

			foreach(Tank t2 in battlefield.enemyTanks)
				if(Math.Abs(t2.position.X - t.position.X) / Battlefield.tileSize == 1 && Math.Abs(t2.position.Y - t.position.Y) / Battlefield.tileSize == 1) //diagonal
					t2.health -= 7;
				else if(Math.Abs(t2.position.X - t.position.X) / Battlefield.tileSize <= 1 && Math.Abs(t2.position.Y - t.position.Y) / Battlefield.tileSize <= 1) //next to
					t2.health -= 11;
		}

		public override void onHitBox(int x, int y) {
			battlefield.map[x, y] = MapObject.empty;
			onHitNothing(x, y);
		}

		public override void onHitNothing(int x, int y) {
			battlefield.createExplosion(x * Battlefield.tileSize + Battlefield.tileSize / 2, y * Battlefield.tileSize + Battlefield.tileSize / 2);
			int dist = 30;
			for(int i = 0; i < 9; i++) {
				double nx = dist * Math.Cos(i * MathHelper.PiOver4);
				double ny = dist * Math.Sin(i * MathHelper.PiOver4);
				battlefield.createDelayedExplosion(x * Battlefield.tileSize + Battlefield.tileSize / 2 + (int) nx, y * Battlefield.tileSize + Battlefield.tileSize / 2 + (int) ny, 3);
			}

			foreach(Tank t in battlefield.friendlyTanks)
				if(Math.Abs(t.position.X - x) / Battlefield.tileSize == 1 && Math.Abs(t.position.Y - y) == 1) //diagonal
					t.health -= 7;
				else if(Math.Abs(t.position.X - x) / Battlefield.tileSize <= 1 && Math.Abs(t.position.Y - y) <= 1) //next to
					t.health -= 11;

			foreach(Tank t in battlefield.enemyTanks)
				if(Math.Abs(t.position.X - x) / Battlefield.tileSize == 1 && Math.Abs(t.position.Y - y) / Battlefield.tileSize == 1) //diagonal
					t.health -= 7;
				else if(Math.Abs(t.position.X - x) / Battlefield.tileSize <= 1 && Math.Abs(t.position.Y - y) / Battlefield.tileSize <= 1) //next to
					t.health -= 11;
		}
	}
}
