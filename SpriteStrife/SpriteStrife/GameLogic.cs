using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SpriteStrife
{
    public enum GameState { Running = 0, MainMenu, CharSelectMenu, CharMenu, LevelUpMenu, Graveyard, Tutorial, OptionMenu };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameLogic : Microsoft.Xna.Framework.Game
    {
        GameState gameState;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont debugFont;
        MapGen mapGen;
        Map dMap;
        GUI gui;
        Texture2D tileSetImg;
        Texture2D objectSetImg;
        KeyboardState oldKState;
        MouseState oldMState;
        HeroGen heroGen;
        Hero hero;
        TimeSpan lastTurnTime;
        int frameCounter, FPS;
        int turnCount;
        int repTime;
        bool debugmode;
        
        List<Point> boxAround;
        TimeSpan elapsedTime;
        Random rando;

        List<Texture2D> heroTex, itemTex, monsterTex, powerTex;

        public GameLogic()
        {
            graphics = new GraphicsDeviceManager(this);
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += new System.EventHandler<System.EventArgs>(Window_ClientSizeChanged);

            InitGraphicsMode(840, 600, false);

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //StatSystem playerStats = new StatSystem();
            //for (int i = 0; i < 9; ++i) {
            //    playerStats.SetBaseStat((StatType)i, i);
            //}

         
            //IEnumerable<Stat> statistics = from s in playerStats.Stats
            //                               where s.BaseValue >= 5
            //                               orderby s.Type
            //                               select s;
            //foreach (var stat in statistics)
            //{
            //    Console.WriteLine("{0}: {1}", stat.Type, stat.Value);
            //}

            gameState = GameState.MainMenu;
            debugmode = false;

            rando = new Random();

            base.IsMouseVisible = true;

            oldKState = Keyboard.GetState();
            oldMState = Mouse.GetState();

            repTime = 150;

            boxAround = new List<Point>();
            boxAround.Add(new Point(1, 0));
            boxAround.Add(new Point(-1, 0));
            boxAround.Add(new Point(0, 1));
            boxAround.Add(new Point(0, -1));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            debugFont = Content.Load<SpriteFont>("font_atari10s");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //load various textures
            heroTex = new List<Texture2D>();
            heroTex.Add(Content.Load<Texture2D>("hero\\hero_pirate"));
            monsterTex = new List<Texture2D>();
            monsterTex.Add(Content.Load<Texture2D>("monster\\monster_goblin"));
            itemTex = new List<Texture2D>();
            itemTex.Add(Content.Load<Texture2D>("item\\item_treasure_sack"));
            powerTex = new List<Texture2D>();
            powerTex.Add(Content.Load<Texture2D>("power\\power_fireball"));

            //load textures for gui elements and create gui
            Texture2D fb = this.Content.Load<Texture2D>("gui_frameborders");
            gui = new GUI(GraphicsDevice, Content, new Color(20, 20, 100, 180), new Color(100, 100, 220, 180));
            
            //load tileset and generate map
            tileSetImg = this.Content.Load<Texture2D>("tileset_basic");
            objectSetImg = this.Content.Load<Texture2D>("objectset_basic");
            mapGen = new MapGen();

            heroGen = new HeroGen();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            if (hero != null && dMap != null)
            {
                mapGen.SaveMap(hero.name, dMap);
                heroGen.SaveHero(hero);
            }
        }

        public void NewGame(string hname, int hclass)
        {
            dMap = mapGen.GenerateMap();
            
            gui.LogEntry("Welcome to Sprite Strife!", Color.LightGray);

            //create the hero
            hero = new Hero(hname, 0);
            hero.mapX = dMap.startingLocation.X;
            hero.mapY = dMap.startingLocation.Y;

            FixMap();

            mapGen.SaveMap("testchar", dMap);
            heroGen.SaveHero(hero);

            gameState = GameState.Running;
        }

        public void ContinueGame(string hname)
        {
            dMap = mapGen.LoadMap("testchar", 0);
            hero = heroGen.LoadHero("testchar");

            FixMap();

            gameState = GameState.Running;
        }

        public void BuryGame(string hname)
        {
            //code to delete map files and add hero to graveyard on game end
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (gameState == GameState.Running)
            {
                //bring out yer dead
                for (int i = dMap.monsters.Count - 1; i >= 0; i--)
                {
                    if (i >= 0 && !dMap.monsters[i].alive)
                    {
                        dMap.monsters[i] = null;
                        dMap.monsters.RemoveAt(i);
                        //Console.WriteLine("cleaned up a monster");
                    }
                }
            }
            
            //handle keyboard and mouse input
            UpdateInput(gameTime);

            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                FPS = frameCounter;
                frameCounter = 0;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles keyboard and mouse input.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void UpdateInput(GameTime gameTime)
        {
            KeyboardState newKState = Keyboard.GetState();
            MouseState newMState = Mouse.GetState();

            //gui interaction
            Point mpoint = new Point(0,0);
            gameState = gui.ProcessInput(this, gameState, oldKState, newKState, oldMState, newMState, hero, dMap);

            if (gameState == GameState.Running)
            {
                //save & menu functions
                if (newKState.IsKeyDown(Keys.Escape) && !oldKState.IsKeyDown(Keys.Escape))
                {
                    mapGen.SaveMap(hero.name, dMap);
                    heroGen.SaveHero(hero);
                    gameState = GameState.MainMenu;
                }
                if (newKState.IsKeyDown(Keys.F5) && !oldKState.IsKeyDown(Keys.F5))
                {
                    heroGen.SaveHero(hero);
                    mapGen.SaveMap(hero.name, dMap);
                    gui.LogEntry(string.Format("Saving {0}...", hero.name), Color.Brown);
                }
                if (newKState.IsKeyDown(Keys.F9) && !oldKState.IsKeyDown(Keys.F9))
                {
                    dMap = mapGen.LoadMap(hero.name, 0);
                    hero = heroGen.LoadHero(hero.name);
                    AlignMap(hero);
                    FixMap();
                    gui.LogEntry(string.Format("Loading {0}...", hero.name), Color.Brown);
                }

                //movement
                if ((gameTime.TotalGameTime - lastTurnTime).Milliseconds >= repTime)
                {
                    if (newKState.IsKeyDown(Keys.Down) || newKState.IsKeyDown(Keys.S))
                    {
                        hero.Move(hero.mapX, hero.mapY + 1, dMap);
                    }
                    if (newKState.IsKeyDown(Keys.Up) || newKState.IsKeyDown(Keys.W))
                    {
                        hero.Move(hero.mapX, hero.mapY - 1, dMap);
                    }
                    if (newKState.IsKeyDown(Keys.Left) || newKState.IsKeyDown(Keys.A))
                    {
                        hero.Move(hero.mapX - 1, hero.mapY, dMap);
                    }
                    if (newKState.IsKeyDown(Keys.Right) || newKState.IsKeyDown(Keys.D))
                    {
                        hero.Move(hero.mapX + 1, hero.mapY, dMap);
                    }
                    if (newKState.IsKeyDown(Keys.Space))
                    {
                        if (hero.delay == 0) hero.delay += 1;
                    }
                }

                //queued moves
                if (hero.mQueue.Count > 0 && (gameTime.TotalGameTime - lastTurnTime).Milliseconds >= repTime)
                {
                    hero.MoveFromQueue(dMap);
                }

                // Realign the map
                if (hero.delay > 0)
                {
                    FixMap();
                    EndTurn(gameTime);
                }
            }

            // Update saved state.
            oldKState = newKState;
            oldMState = newMState;
        }

        private void FixMap()
        {
            dMap.vision[hero.mapX, hero.mapY] = Vision.Explored;
            dMap.DoSight(hero, debugmode);
            AlignMap(hero);
        }

        /// <summary>
        /// This is called after the hero has acted to advance the turn.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void EndTurn(GameTime gameTime)
        {
            foreach (Monster fiend in dMap.monsters)
            {
                fiend.delay -= hero.delay;
                while (fiend.delay <= 0 && fiend.alive)
                {
                    fiend.Act(hero, dMap);
                }
            }

            hero.delay = 0;
            lastTurnTime = gameTime.TotalGameTime;
            turnCount += 1;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //redraw any gui frames that have changed
            gui.UpdateAll(spriteBatch);

            //clear the background
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            if (gameState == GameState.Running)
            {
                //draw the map
                Rectangle cutter;
                for (int x = 0; x < dMap.floor.GetLength(0); x++)
                {
                    for (int y = 0; y < dMap.floor.GetLength(1); y++)
                    {

                        if (dMap.vision[x, y] == Vision.Visible)
                        {
                            cutter = new Rectangle(dMap.alts[x, y] * dMap.tileSetSize, (int)dMap.floor[x, y] * dMap.tileSetSize, dMap.tileSetSize, dMap.tileSetSize);
                            spriteBatch.Draw(tileSetImg, new Rectangle(x * dMap.tileSizeX + dMap.xOffset, y * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), cutter, Color.White);
                            if (dMap.objects[x, y].type >= 0)
                            {
                                cutter = new Rectangle(dMap.objects[x, y].type * dMap.tileSetSize, 0, dMap.tileSetSize, dMap.tileSetSize);
                                spriteBatch.Draw(objectSetImg, new Rectangle(x * dMap.tileSizeX + dMap.xOffset, y * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), cutter, Color.White);
                            }
                        }
                        else if (dMap.vision[x, y] == Vision.Explored)
                        {
                            cutter = new Rectangle(dMap.alts[x, y] * dMap.tileSetSize, (int)dMap.floor[x, y] * dMap.tileSetSize, dMap.tileSetSize, dMap.tileSetSize);
                            spriteBatch.Draw(tileSetImg, new Rectangle(x * dMap.tileSizeX + dMap.xOffset, y * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), cutter, Color.DarkSlateGray);
                            if (dMap.objects[x, y].type >= 0)
                            {
                                cutter = new Rectangle(dMap.objects[x, y].type * dMap.tileSetSize, 0, dMap.tileSetSize, dMap.tileSetSize);
                                spriteBatch.Draw(objectSetImg, new Rectangle(x * dMap.tileSizeX + dMap.xOffset, y * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), cutter, Color.DarkSlateBlue);
                            }
                        }

                        //if (debugmode)
                        //{
                        //    string output = dMap.floor[x, y].ToString();
                        //    Vector2 FontPos = new Vector2(x * dMap.TileSizeX + dMap.xOffset, y * dMap.TileSizeY + dMap.yOffset);
                        //    spriteBatch.DrawString(CourierNew, output, FontPos, Color.White);
                        //}
                    }
                }

                //draw items
                //foreach (Item wonder in dMap.items)
                //{
                //    if (dMap.vision[wonder.MapX, wonder.MapY] == Vision.Visible)
                //    {
                //        spriteBatch.Draw(itemTex[wonder.type], new Rectangle(wonder.MapX * dMap.tileSizeX + dMap.xOffset, wonder.MapY * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), Color.White);
                //    }
                //}

                //draw monsters
                foreach (Monster fiend in dMap.monsters)
                {
                    if (dMap.vision[fiend.mapX, fiend.mapY] == Vision.Visible)
                    {
                        spriteBatch.Draw(monsterTex[fiend.type], new Rectangle(fiend.mapX * dMap.tileSizeX + dMap.xOffset, fiend.mapY * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), Color.White);
                        if (debugmode)
                        {
                            spriteBatch.DrawString(debugFont, string.Format("{0}\n{1}{2}", fiend.name, fiend.curAIMode.ToString().Substring(0, 2), fiend.delay), new Vector2(fiend.mapX * dMap.tileSizeX + dMap.xOffset, fiend.mapY * dMap.tileSizeY + dMap.yOffset), Color.Red);
                        }
                    }
                }
                //draw the hero
                spriteBatch.Draw(heroTex[hero.type], new Rectangle(GraphicsDevice.Viewport.Width / 2 - ((GraphicsDevice.Viewport.Width / 2) % dMap.tileSizeX), GraphicsDevice.Viewport.Height / 2 - ((GraphicsDevice.Viewport.Height / 2) % dMap.tileSizeX), dMap.tileSizeX, dMap.tileSizeY), Color.White);

                //draw the debug status information
                if (true)//debugmode)
                {
                    string output = string.Format("Pos: {0}, {1}   Turn: {2},   FPS: {3}", hero.mapX, hero.mapY, turnCount, FPS);
                    Vector2 FontPos = new Vector2(220, 15);
                    spriteBatch.DrawString(debugFont, output, FontPos, Color.White);
                }
            }

            //draw the gui overlay
            gui.DrawAll(gameState, spriteBatch, hero, dMap);
            
            

            spriteBatch.End();

            frameCounter++;

            base.Draw(gameTime);
        }

        /// <summary>
        /// Centers the map; called when window size is changed.
        /// </summary>
        /// <param name="refmob">The hero to center the map on.</param>
        private void AlignMap(Hero refmob)
        {
            dMap.xOffset = GraphicsDevice.Viewport.Width / 2 - ((GraphicsDevice.Viewport.Width / 2) % dMap.tileSizeX) - (hero.mapX * dMap.tileSizeX);
            dMap.yOffset = GraphicsDevice.Viewport.Height / 2 - ((GraphicsDevice.Viewport.Height / 2) % dMap.tileSizeY) - (hero.mapY * dMap.tileSizeY);
        }


        protected void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            if (GraphicsDevice.Viewport.Width < 840) InitGraphicsMode(840, GraphicsDevice.Viewport.Height, false);
            if (GraphicsDevice.Viewport.Height < 600) InitGraphicsMode(GraphicsDevice.Viewport.Width, 600, false);
            gui.UpdateAll(spriteBatch, true);
            AlignMap(hero);
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Iterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
