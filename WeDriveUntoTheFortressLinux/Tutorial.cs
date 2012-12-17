using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WeDriveUntoTheFortress {
	public class Tutorial {

		public Battlefield battlefield = Program.game.battlefield;
		public int stage = 0;
		public int endTimer = 0;
		public Tank.Dir dir = Tank.Dir.right;

		public void onUpdate() {
			if(endTimer < 300) {
				battlefield.nextTurnTimer = 60;
				switch(stage) {
					case 0:
						if(battlefield.beingMoved != null) {
							dir = battlefield.beingMoved.gunDir;
							if(battlefield.moving)
								stage++;
						}
						break;
					case 1:
						if(battlefield.beingMoved.gunDir != dir)
							stage++;
						break;
					case 2:
						if(battlefield.shooting || battlefield.shot)
							stage++;
						break;
					case 3:
						endTimer++;
						break;
				}
			}
		}

		public void draw() {
			if(endTimer < 300)
				switch(stage) {
					case 0:
						battlefield.port.drawSmallStringCentered("Use Arrow keys to move", new Vector2(Program.game.width / 2, Program.game.height / 2 - 32), Color.Black);
						break;
					case 1:
						battlefield.port.drawSmallStringCentered("Use WASD to aim", new Vector2(Program.game.width / 2, Program.game.height / 2 - 32), Color.Black);
						break;
					case 2:
						battlefield.port.drawSmallStringCentered("Hold space to shoot", new Vector2(Program.game.width / 2, Program.game.height / 2 - 32), Color.Black);
						break;
					case 3:
						battlefield.port.drawLargeStringCentered("KILL THEM ALL", new Vector2(Program.game.width / 2, Program.game.height / 2 - 32), Color.Black);
						break;
				}
		}
	}
}
