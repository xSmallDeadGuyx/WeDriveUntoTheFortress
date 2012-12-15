using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WeDriveUntoTheFortress {
	public class SaveData {
		public bool[] levelsComplete;

		public SaveData(int levels) {
			levelsComplete = new bool[levels];
		}

		public SaveData(bool[] complete) {
			levelsComplete = complete;
		}

		public void saveDataToFile() {
			BinaryWriter bw = new BinaryWriter(new FileStream(".save", FileMode.Create));
			foreach(bool b in levelsComplete)
				bw.Write((byte) (b ? 255 : Program.game.rand.Next(255)));
			bw.BaseStream.Close();
			bw.Close();
		}

		public void readDataFromFile() {
			if(File.Exists(".save")) {
				BinaryReader br = new BinaryReader(new FileStream(".save", FileMode.Open));
				byte[] data = br.ReadBytes((int) br.BaseStream.Length);
				levelsComplete = new bool[data.Length];
				for(int i = 0; i < data.Length; i++) {
					levelsComplete[i] = data[i] == 255;
					if(levelsComplete[i]) Program.game.selectedLevel = i == data.Length - 1 ? i : i + 1;
				}
				br.BaseStream.Close();
				br.Close();
			}
		}
	}
}
