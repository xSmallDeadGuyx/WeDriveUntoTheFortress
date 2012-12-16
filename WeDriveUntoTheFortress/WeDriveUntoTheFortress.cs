using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace WeDriveUntoTheFortress {
	public class WeDriveUntoTheFortress : Game {
		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;

		public Random rand = new Random();

		public SoundEffectInstance menuMusic;
		public SoundEffectInstance gameMusic;

		public readonly int vBorder = 32;
		public readonly int width = 640;
		public readonly int height = 480;

		public enum GameState { mainMenu, levelSelect, inBattle };
		public readonly int numLevels = 5;

		public Texture2D title;
		public Texture2D hudMain;
		public FontRenderer smallFont;
		public FontRenderer largeFont;

		public Texture2D village;
		public Texture2D fortress;
		public Texture2D[] story;

		public Texture2D circle_large_darkred;
		public Texture2D circle_large_red;
		public Texture2D circle_large_green;
		public Texture2D circle_small_darkred;
		public Texture2D circle_small_red;
		public Texture2D circle_small_green;

		public Texture2D circle_large_outline;

		private GameState state = GameState.mainMenu;
		public GameState gameState {
			get { return state; }
			set {
				switch(value) {
					case GameState.mainMenu:
						createMainMenu();
						state = value;
						break;
					case GameState.levelSelect:
						createLevelSelectMenu();
						state = value;
						break;
					default:
						state = value;
						break;
				}
			}
		}

		public Menu mainMenu;
		public Menu levelMenu;
		public int selectedLevel = 0;
		public int hoveredLevel = -1;
		public LevelData levelData = new LevelData();

		public SaveData saveData;

		public Battlefield battlefield;
		public Tutorial tutorial;

		public WeDriveUntoTheFortress() : base() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferWidth = width;
			graphics.PreferredBackBufferHeight = height;
			graphics.IsFullScreen = false;
			graphics.ApplyChanges();
		}

		private void createMainMenu() {
			mainMenu = new Menu(new Viewport(0, vBorder, width, height - 2 * vBorder));

			gameMusic.Stop();
			menuMusic.Play();

			string[] buttons = { "One Player", "Two Player", "Quit" };
			for(int i = 0; i < buttons.Length; i++) {
				int x = width / 2 - 128;
				int y = height / 2 + (i - buttons.Length / 2) * 72;
				mainMenu.controls.Add(new MenuButton(mainMenu, i, buttons[i], new Vector2(x, y)));
			}

			mainMenu.performEvent = delegate(int id) {
				switch(id) {
					case 0:
						saveData = new SaveData(numLevels);
						saveData.readDataFromFile();
						gameState = GameState.levelSelect;
						break;
					case 1:
						battlefield = new Battlefield(levelData[rand.Next(levelData.length)], new Viewport(0, vBorder, width, height - 2 * vBorder), true);
						gameState = GameState.inBattle;
						break;
					case 2:
						Exit();
						break;
				}
			};
		}

		private void createLevelSelectMenu() {
			levelMenu = new Menu(new Viewport(0, vBorder, width, height - 2 * vBorder));

			gameMusic.Stop();
			menuMusic.Play();

			levelMenu.controls.Add(new MenuButton(levelMenu, 0, "Back", new Vector2(width/2 - 288, height / 2 - vBorder + 64)));
			levelMenu.controls.Add(new MenuButton(levelMenu, 1, "Go!", new Vector2(width / 2 + 32, height / 2 - vBorder + 64)));
			levelMenu.performEvent = delegate(int id) {
				switch(id) {
					case 0:
						gameState = GameState.mainMenu;
						break;
					case 1:
						battlefield = new Battlefield(levelData[selectedLevel], new Viewport(0, vBorder, width, height - 2 * vBorder));
						if(selectedLevel == 0) tutorial = new Tutorial();
						gameState = GameState.inBattle;
						break;
				}
			};
		}

		protected override void Initialize() {
			base.Initialize();
		}

		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);

			hudMain = Content.Load<Texture2D>("hud_main");
			title = Content.Load<Texture2D>("title");

			SoundEffect menuSe = Content.Load<SoundEffect>("music_menu");
			SoundEffect mainSe = Content.Load<SoundEffect>("music_battle");
			SoundEffect expSe = Content.Load<SoundEffect>("explosion");

			menuMusic = menuSe.CreateInstance();
			menuMusic.Volume = 1.0F;
			menuMusic.IsLooped = true;
			gameMusic = mainSe.CreateInstance();
			gameMusic.Volume = 1.0F;
			gameMusic.IsLooped = true;

			Battlefield.explosion = expSe.CreateInstance();
			Battlefield.explosion.Volume = 1.0F;
			Battlefield.explosion.IsLooped = false;

			FontFile smallFile = FontLoader.Load(Path.Combine(Content.RootDirectory, "AncienSmall.fnt"));
			Texture2D smallTex = Content.Load<Texture2D>("AncienSmall_0.png");
			FontFile largeFile = FontLoader.Load(Path.Combine(Content.RootDirectory, "AncienLarge.fnt"));
			Texture2D largeTex = Content.Load<Texture2D>("AncienLarge_0.png");
			smallFont = new FontRenderer(smallFile, smallTex);
			largeFont = new FontRenderer(largeFile, largeTex);

			MenuButton.button_up = Content.Load<Texture2D>("button_up");
			MenuButton.button_down = Content.Load<Texture2D>("button_down");
			MenuButton.button_over = Content.Load<Texture2D>("button_over");

			story = new Texture2D[] { Content.Load<Texture2D>("story1"), Content.Load<Texture2D>("story2"), Content.Load<Texture2D>("story3"), Content.Load<Texture2D>("story4"), Content.Load<Texture2D>("story5") };

			Battlefield.grass = Content.Load<Texture2D>("grass");
			Battlefield.box = Content.Load<Texture2D>("box");

			Battlefield.tankFriendly = Content.Load<Texture2D>("tank_green");
			Battlefield.tankEnemy = Content.Load<Texture2D>("tank_red");
			Battlefield.gunFriendly = Content.Load<Texture2D>("tank_gun_green");
			Battlefield.gunEnemy = Content.Load<Texture2D>("tank_gun_red");

			Battlefield.turnIndicator = Content.Load<Texture2D>("turn_indicator");

			Battlefield.youWin = Content.Load<Texture2D>("you_win");
			Battlefield.youLose = Content.Load<Texture2D>("you_lose");
			Battlefield.p1Win = Content.Load<Texture2D>("player_1_wins");
			Battlefield.p2Win = Content.Load<Texture2D>("player_2_wins");

			Explosion.texture = Content.Load<Texture2D>("explosion");

			Battlefield.weaponTextures = new Texture2D[] { Content.Load<Texture2D>("impact_target"), Content.Load<Texture2D>("cluster_target") };

			village = Content.Load<Texture2D>("village");
			fortress = Content.Load<Texture2D>("fortress");

			circle_large_darkred = Content.Load<Texture2D>("circle_large_darkred");
			circle_large_red = Content.Load<Texture2D>("circle_large_red");
			circle_large_green = Content.Load<Texture2D>("circle_large_green");
			circle_small_darkred = Content.Load<Texture2D>("circle_small_darkred");
			circle_small_red = Content.Load<Texture2D>("circle_small_red");
			circle_small_green = Content.Load<Texture2D>("circle_small_green");

			circle_large_outline = Content.Load<Texture2D>("circle_large_outline");

			createMainMenu();
		}

		protected override void UnloadContent() {
			
		}

		protected override void Update(GameTime gameTime) {
			switch(state) {
				case GameState.mainMenu:
					mainMenu.onUpdate();
					break;
				case GameState.levelSelect:
					levelMenu.onUpdate();
					hoveredLevel = -1;
					MouseState mouse = Mouse.GetState();
					for(int i = 0; i < numLevels; i++) {
						int x = width / 2 + (96 * (i - numLevels / 2));
						int y = height / 2;
						if(mouse.X > x - (i == numLevels - 1 ? 32 : 16) && mouse.X < x + (i == numLevels - 1 ? 32 : 16) && mouse.Y > y - (i == numLevels - 1 ? 40 : 16) && mouse.Y < y + (i == numLevels - 1 ? 40 : 16))
							hoveredLevel = i;
						if(mouse.X > x - circle_large_red.Width / 2 && mouse.X < x + circle_large_red.Width / 2 && mouse.Y > y + 40 - circle_large_red.Height / 2 && mouse.Y < y + 40 + circle_large_red.Height / 2)
							hoveredLevel = i;
					}
					if(mouse.LeftButton == ButtonState.Pressed && hoveredLevel > -1 && (saveData.levelsComplete[hoveredLevel] || hoveredLevel == 0 || saveData.levelsComplete[hoveredLevel - 1]))
						selectedLevel = hoveredLevel;
					break;
				case GameState.inBattle:
					battlefield.onUpdate();
					if(!battlefield.is2Player && selectedLevel == 0)
						tutorial.onUpdate();
					break;
			}
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Gray);

			switch(state) {
				case GameState.mainMenu:
					spriteBatch.Begin();
					spriteBatch.Draw(hudMain, new Rectangle(0, 0, width, height), Color.White);
					spriteBatch.Draw(title, new Vector2(width / 2 - title.Width / 2, 64), Color.White); 
					mainMenu.draw();
					spriteBatch.End();
					break;
				case GameState.levelSelect:
					spriteBatch.Begin();
					spriteBatch.Draw(hudMain, new Rectangle(0, 0, width, height), Color.White);
					spriteBatch.Draw(title, new Vector2(width / 2 - title.Width / 2, 64), Color.White); 
					for(int i = 0; i < numLevels; i++) {
						int x = width / 2 + (96 * (i - numLevels / 2));
						int y = height / 2;
						spriteBatch.Draw(i == numLevels - 1 ? fortress : village, new Vector2(x - (i == numLevels - 1 ? 32 : 16), y - (i == numLevels - 1 ? 40 : 16)), Color.White);
						spriteBatch.Draw(saveData.levelsComplete[i] ? circle_large_green : i == 0 || saveData.levelsComplete[i - 1] ? circle_large_red : circle_large_darkred, new Vector2(x - circle_large_red.Width / 2, y + 40 - circle_large_red.Height / 2), Color.White);
						if(hoveredLevel == i)
							spriteBatch.Draw(circle_large_outline, new Vector2(x - circle_large_red.Width / 2, y + 40 - circle_large_red.Height / 2), Color.LightGray);
						if(selectedLevel == i)
							spriteBatch.Draw(circle_large_outline, new Vector2(x - circle_large_red.Width / 2, y + 40 - circle_large_red.Height / 2), Color.White);
						if(i > 0)
							for(int n = 3; n > 0; n--)
								spriteBatch.Draw(saveData.levelsComplete[i] ? circle_small_green : i == 0 || saveData.levelsComplete[i - 1] ? circle_small_red : circle_small_darkred, new Vector2(x - circle_small_red.Width / 2 - n * 24, y + 40 - circle_small_red.Height / 2), Color.White);
					}
					levelMenu.draw();
					spriteBatch.End();
					break;
				case GameState.inBattle:
					spriteBatch.Begin();
					if(!battlefield.draw())
						spriteBatch.Draw(hudMain, new Rectangle(0, 0, width, height), Color.White);
					battlefield.drawHUD();
					if(!battlefield.is2Player && selectedLevel == 0)
						tutorial.draw();
					spriteBatch.End();
					break;
			}

			base.Draw(gameTime);
		}
	}
}
