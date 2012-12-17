using System;
using System.Collections.Generic;
using System.Linq;

namespace WeDriveUntoTheFortress {
	public static class Program {
		public static WeDriveUntoTheFortress game;

		[STAThread]
		static void Main() {
			game = new WeDriveUntoTheFortress();
			game.Run();
		}
	}
}
