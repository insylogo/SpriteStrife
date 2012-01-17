using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpriteStrife
{
    class GUI
    {
        public class Box
        {
            public GraphicsDevice gD;
            public Rectangle Rect, contRect;
            public RenderTarget2D Surf;
            public int scrSizeX, scrSizeY;
            public int xOff, yOff;
            public Color bgCol;
            public Color bdCol;
            public Texture2D bg;
            public Texture2D frameBorders;
            public bool needUpdate;

            public Box(GraphicsDevice gd, int x, int y, int wd, int ht, Color bgc, Color bdc, Texture2D fb)
            {
                frameBorders = fb;
                gD = gd;
                Rect = new Rectangle(x, y, wd, ht);
                MoveTo(x, y);

                bgCol = bgc;
                bdCol = bdc;

                Color[] tbg = new Color[1];
                tbg[0] = Color.White;
                bg = new Texture2D(gd, 1, 1);
                bg.SetData<Color>(tbg);

                needUpdate = true;
            }

            public void MoveTo(int x, int y)
            {
                xOff = x;
                yOff = y;
                scrSizeX = gD.Viewport.Width;
                scrSizeY = gD.Viewport.Height;
                if (xOff < 0) Rect.X = scrSizeX - Rect.Width + xOff;
                else Rect.X = xOff;
                if (yOff < 0) Rect.Y = scrSizeY - Rect.Height + yOff;
                else Rect.Y = yOff;

                contRect = Rect;
                contRect.Inflate(-5, -5);
            }

            public void Update(SpriteBatch screen)
            {
                if (needUpdate)
                {
                    needUpdate = false;

                    MoveTo(xOff, yOff);

                    screen.Begin();
                    
                    Surf = new RenderTarget2D(gD, Rect.Width, Rect.Height);
                    gD.SetRenderTarget(Surf);

                    gD.Clear(Color.Transparent);

                    //draw borders from template image
                    Rectangle cutter = new Rectangle(5, 0, 5, 5); //top
                    for (int i = 5; i < Rect.Width - 5; i+= 5) screen.Draw(frameBorders, new Rectangle(i, 0, 5, 5), cutter, bdCol);

                    cutter = new Rectangle(5, 10, 5, 5); //bottom
                    for (int i = 5; i < Rect.Width - 5; i += 5) screen.Draw(frameBorders, new Rectangle(i, Rect.Height - 5, 5, 5), cutter, bdCol);

                    cutter = new Rectangle(10, 5, 5, 5); //right
                    for (int i = 5; i < Rect.Height - 5; i += 5) screen.Draw(frameBorders, new Rectangle(Rect.Width - 5, i, 5, 5), cutter, bdCol);

                    cutter = new Rectangle(0, 5, 5, 5); //left
                    for (int i = 5; i < Rect.Height - 5; i += 5) screen.Draw(frameBorders, new Rectangle(0, i, 5, 5), cutter, bdCol);

                    //draw corners from template image
                    screen.Draw(frameBorders, new Rectangle(0, 0, 5, 5), new Rectangle(0, 0, 5, 5), bdCol);
                    screen.Draw(frameBorders, new Rectangle(0, Rect.Height - 5, 5, 5), new Rectangle(0, 10, 5, 5), bdCol);
                    screen.Draw(frameBorders, new Rectangle(Rect.Width - 5, Rect.Height - 5, 5, 5), new Rectangle(10, 10, 5, 5), bdCol);
                    screen.Draw(frameBorders, new Rectangle(Rect.Width - 5, 0, 5, 5), new Rectangle(10, 0, 5, 5), bdCol);

                    //fill in background
                    screen.Draw(bg, new Rectangle(5, 5, contRect.Width, contRect.Height), bgCol);

                    screen.End();
                    gD.SetRenderTarget(null);
                }
            }

            public void Draw(SpriteBatch screen)
            {
                screen.Draw(Surf, Rect, Color.White);
            }
        }

        public class MenuItem
        {
            public Rectangle rect;
            public Vector2 labelLoc;
            public string labelText;
            public bool highlight;
            public bool enabled;

            public MenuItem(string itemText, int x, int y, SpriteFont mfont)
            {
                labelText = itemText;
                labelLoc = new Vector2((int)(x - (mfont.MeasureString(labelText).X / 2)), (int)(y - (mfont.MeasureString(labelText).Y / 2)));
                rect = new Rectangle((int)labelLoc.X, (int)labelLoc.Y, (int)mfont.MeasureString(labelText).X, (int)mfont.MeasureString(labelText).Y);
                rect.Inflate(5, 5);
                highlight = false;
                enabled = true;
            }
        }

        [Serializable]
        public class DeathCertificate
        {
            public int level, score;
            public string name;

            public DeathCertificate(int Level, int Score, string Name)
            {
                level = Level;
                score = Score;
                name = Name;
            }
        }

        public GraphicsDevice gD;
        public int scrSizeX, scrSizeY;
        public List<Box> boxes;
        public Color bgCol, bdCol, hlCol;
        public double osc;
        public Texture2D frameBorders, mmtile, mmmark, mmblock, itemcircle;
        public List<Texture2D> actIcons;
        public SpriteFont logFont, menuFont;
        public List<string> logLines;
        public List<Color> logColors;
        public int logOff;
        public int selAct;
        public Texture2D statBar;
        public List<MenuItem> mainMenu;
        public List<MenuItem> gyMenu;
        public List<DeathCertificate> graves;
        public Texture2D graveTex;
        public List<Texture2D> overlays;

        public GUI(GraphicsDevice gd, ContentManager cm, Color bgc, Color bdc)
        {
            frameBorders = cm.Load<Texture2D>("gui_frameborders");
            gD = gd;
            scrSizeX = gD.Viewport.Width;
            scrSizeY = gD.Viewport.Height;

            osc = 0;

            bgCol = bgc;
            bdCol = bdc;
            hlCol = Color.Black;

            mmtile = cm.Load<Texture2D>("mm_tile");
            mmmark = cm.Load<Texture2D>("mm_mark");
            mmblock = cm.Load<Texture2D>("mm_block");

            overlays = new List<Texture2D>();
            overlays.Add(cm.Load<Texture2D>("gui_overlay_foot"));
            overlays.Add(cm.Load<Texture2D>("gui_overlay_loc"));

            itemcircle = cm.Load<Texture2D>("gui_itemcircle");
            actIcons = new List<Texture2D>();
            actIcons.Add(cm.Load<Texture2D>("item\\item_1hwep_scimitar"));
            actIcons.Add(cm.Load<Texture2D>("power\\power_fireball"));
            actIcons.Add(cm.Load<Texture2D>("item\\item_potion_red"));
            actIcons.Add(cm.Load<Texture2D>("item\\item_potion_simplered"));
            selAct = 0;

            menuFont = cm.Load<SpriteFont>("font_atari24s");

            logFont = cm.Load<SpriteFont>("font_atari10s");
            logLines = new List<string>();
            logColors = new List<Color>();
            logOff = 0;

            statBar = cm.Load<Texture2D>("gui_statbar");

            boxes = new List<Box>();
            boxes.Add(new Box(gD, 10, -10, 300, 100, bgCol, bdCol, frameBorders)); //log
            boxes.Add(new Box(gD, 10, 10, 170, 170, bgCol, bdCol, frameBorders)); //minimap
            boxes.Add(new Box(gD, -10, 10, 180, 300, bgCol, bdCol, frameBorders)); //stats
            boxes.Add(new Box(gD, -10, -10, 510, 60, bgCol, bdCol, frameBorders)); //action bar

            mainMenu = new List<MenuItem>();
            mainMenu.Add(new MenuItem("New Game", gD.Viewport.Width / 2, 150, menuFont));
            mainMenu.Add(new MenuItem("Continue", gD.Viewport.Width / 2, 200, menuFont));
            mainMenu.Add(new MenuItem("Tutorial", gD.Viewport.Width / 2, 250, menuFont));
            mainMenu.Add(new MenuItem("Options", gD.Viewport.Width / 2, 300, menuFont));
            mainMenu.Add(new MenuItem("Graveyard", gD.Viewport.Width / 2, 350, menuFont));
            mainMenu.Add(new MenuItem("Exit", gD.Viewport.Width / 2, 400, menuFont));
            mainMenu[2].enabled = false;
            mainMenu[3].enabled = false;

            graveTex = cm.Load<Texture2D>("gravestones");
            gyMenu = new List<MenuItem>();
            gyMenu.Add(new MenuItem("< Back", 150, 80, menuFont));

            //graves = new List<DeathCertificate>();
            //Random grand = new Random();
            //for (int i = 0; i < 15; i++)
            //{
            //    graves.Add(new DeathCertificate(grand.Next(1, 20), 10, string.Format("No-Name Number {0}", i)));
            //}
            //SaveGraves();

            graves = LoadGraves();
        }

        public void DrawAll(GameState gameState, SpriteBatch screen, Hero hero, Map dMap)
        {
            if (gameState == GameState.Running)
            {
                //map overlays
                if (hero.mQueue.Count > 0)
                {
                    Color olcol = new Color(70, 210, 100, 50);
                    screen.Draw(overlays[1], new Rectangle(hero.mQueue[hero.mQueue.Count - 1].X * dMap.tileSizeX + dMap.xOffset, hero.mQueue[hero.mQueue.Count - 1].Y * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), olcol);
                    if (hero.mQueue.Count > 1)
                    {
                        for (int i = 0; i < hero.mQueue.Count - 1; i++)
                        {
                            screen.Draw(overlays[0], new Rectangle(hero.mQueue[i].X * dMap.tileSizeX + dMap.xOffset, hero.mQueue[i].Y * dMap.tileSizeY + dMap.yOffset, dMap.tileSizeX, dMap.tileSizeY), olcol);
                        }
                    }
                }

                //box backgrounds
                foreach (Box thisbox in boxes)
                {
                    thisbox.Draw(screen);
                }

                //log
                int firstL = logLines.Count - 6 - logOff;
                if (firstL < 0) firstL = 0;
                int lastL = logLines.Count - 1 - logOff;
                for (int i = firstL; i <= lastL; i++)
                {
                    Vector2 FontPos = new Vector2(boxes[0].contRect.Left + 3, boxes[0].contRect.Top + (15 * (i - firstL)));
                    screen.DrawString(logFont, logLines[i], FontPos, logColors[i]);
                }

                //minimap
                int lX, lY, uX, uY;
                int tX = hero.mapX - (boxes[1].contRect.Width / 8);
                if (tX < 0)
                {
                    lX = ((-4 * tX) + boxes[1].contRect.Left);
                    tX = 0;
                }
                else lX = (boxes[1].contRect.Left);

                int tY = hero.mapY - (boxes[1].contRect.Height / 8);
                if (tY < 0)
                {
                    lY = ((-4 * tY) + boxes[1].contRect.Top);
                    tY = 0;
                }
                else lY = (boxes[1].contRect.Left);

                uX = boxes[1].contRect.Left + boxes[1].contRect.Width;
                uY = boxes[1].contRect.Top + boxes[1].contRect.Height;

                Rectangle prect;
                int dx = lX;
                int dy = lY;
                while (tX < dMap.bounds.X && dx < uX)
                {
                    while (tY < dMap.bounds.Y && dy < uY)
                    {
                        if (dMap.vision[tX, tY] > 0)
                        {
                            prect = new Rectangle(dx, dy, 4, 4);
                            if (dMap.floor[tX, tY] == FloorTypes.Wall) screen.Draw(mmtile, prect, Color.DarkSlateGray);
                            else if (dMap.floor[tX, tY] == FloorTypes.Floor) screen.Draw(mmtile, prect, Color.LightGray);
                            else if (dMap.floor[tX, tY] == FloorTypes.Feature) screen.Draw(mmblock, prect, Color.Gray);
                        }
                        dy += 4;
                        tY += 1;
                    }
                    dy = lY;
                    tY = hero.mapY - (boxes[1].contRect.Height / 8);
                    if (tY < 0) tY = 0;
                    tX += 1;
                    dx += 4;
                }

                foreach (Item wonder in dMap.items)
                {
                    int mx = boxes[1].contRect.Center.X - (4 * (hero.mapX - wonder.MapX));
                    int my = boxes[1].contRect.Center.Y - (4 * (hero.mapY - wonder.MapY));
                    if (dMap.vision[wonder.MapX, wonder.MapY] == Vision.Visible && boxes[1].contRect.Contains(new Point(mx, my)))
                    {
                        prect = new Rectangle(mx, my, 4, 4);
                        screen.Draw(mmmark, prect, Color.Gold);
                    }
                }

                foreach (Monster fiend in dMap.monsters)
                {
                    int mx = boxes[1].contRect.Center.X - (4 * (hero.mapX - fiend.mapX));
                    int my = boxes[1].contRect.Center.Y - (4 * (hero.mapY - fiend.mapY));
                    if (dMap.vision[fiend.mapX, fiend.mapY] == Vision.Visible && boxes[1].contRect.Contains(new Point(mx, my)))
                    {
                        prect = new Rectangle(mx, my, 4, 4);
                        screen.Draw(mmmark, prect, Color.Red);
                    }
                }

                prect = new Rectangle(boxes[1].contRect.Center.X, boxes[1].contRect.Center.Y, 4, 4);
                screen.Draw(mmmark, prect, Color.Lime);

                //stats
                screen.Draw(statBar, new Rectangle(boxes[2].contRect.Left + 5, boxes[2].contRect.Top + 5, boxes[2].contRect.Width - 10, 20), Color.Green);
                screen.Draw(statBar, new Rectangle(boxes[2].contRect.Left + 5, boxes[2].contRect.Top + 30, boxes[2].contRect.Width - 10, 20), Color.BlueViolet);

                //action bar
                for (int i = 0; i < 10; i++)
                {
                    if (i == selAct) screen.Draw(itemcircle, new Rectangle(boxes[3].contRect.Left + (50 * i), boxes[3].contRect.Top, 50, 50), Color.DarkGreen);
                    else screen.Draw(itemcircle, new Rectangle(boxes[3].contRect.Left + (50 * i), boxes[3].contRect.Top, 50, 50), Color.Blue);
                    Vector2 FontPos = new Vector2(boxes[3].contRect.Left + 1 + (50 * i), boxes[3].contRect.Top + 1);
                    screen.DrawString(logFont, ((i + 1) % 10).ToString(), FontPos, Color.White);
                }
                for (int i = 0; i < actIcons.Count && i < 10; i++)
                {
                    screen.Draw(actIcons[i], new Rectangle(boxes[3].contRect.Left + 5 + (50 * i), boxes[3].contRect.Top + 5, 40, 40), Color.White);
                }
            }
            else if (gameState == GameState.MainMenu)
            {
                foreach (MenuItem mitem in mainMenu)
                {
                    if (mitem.enabled && mitem.highlight)
                    {
                        screen.DrawString(menuFont, mitem.labelText, mitem.labelLoc, hlCol);
                    }
                    else if (mitem.enabled)
                    {
                        screen.DrawString(menuFont, mitem.labelText, mitem.labelLoc, Color.White);
                    }
                    else
                    {
                        screen.DrawString(menuFont, mitem.labelText, mitem.labelLoc, Color.Gray);
                    }
                }
            }
            else if (gameState == GameState.Graveyard)
            {
                for (int i = graves.Count - 1; i >= 0; i--)
                {
                    int row = (int)Math.Floor((double)i / 4);
                    int col = i % 4;
                    Color drawcol = new Color(220 - (40 * row), 220 - (40 * row), 220 - (40 * row));
                    screen.Draw(graveTex, new Rectangle(20 + (200 - (30 * row)) * col + (60 * row), (1500 / (10 + row ^ 2)), 180 - 50 * row, 360 - 100 * row), new Rectangle(10 * (graves[i].level / 4), 0, 10, 20), drawcol);
                }

                foreach (MenuItem mitem in gyMenu)
                {
                    if (mitem.enabled && mitem.highlight)
                    {
                        screen.DrawString(menuFont, mitem.labelText, mitem.labelLoc, hlCol);
                    }
                    else if (mitem.enabled)
                    {
                        screen.DrawString(menuFont, mitem.labelText, mitem.labelLoc, Color.White);
                    }
                    else
                    {
                        screen.DrawString(menuFont, mitem.labelText, mitem.labelLoc, Color.Gray);
                    }
                }
            }
        }

        public void LogEntry(string linetext, Color linecolor)
        {
            int maxLen = boxes[0].contRect.Width / 11;
            while (linetext.Length > maxLen)
            {
                logLines.Add(linetext.Substring(0, maxLen));
                linetext = linetext.Remove(0, maxLen);
                logColors.Add(linecolor);
            }
            logLines.Add(linetext);
            logColors.Add(linecolor);
        }

        public void LogScrollUp()
        {
            if (logOff < (logLines.Count - 6)) logOff += 1;
        }

        public void LogScrollDown()
        {
            if (logOff > 0) logOff -= 1;
        }

        public void UpdateAll(SpriteBatch screen, bool forceUpdate = false)
        {
            //redraw boxes
            foreach (Box thisbox in boxes)
            {
                if (forceUpdate) thisbox.needUpdate = true;
                thisbox.Update(screen);
            }

            //center menu items
            if (forceUpdate)
            {
                foreach (MenuItem mitem in mainMenu)
                {
                    mitem.labelLoc = new Vector2((int)((gD.Viewport.Width / 2) - (menuFont.MeasureString(mitem.labelText).X / 2)), (int)(mitem.labelLoc.Y));
                    mitem.rect = new Rectangle((int)mitem.labelLoc.X, (int)mitem.labelLoc.Y, (int)menuFont.MeasureString(mitem.labelText).X, (int)menuFont.MeasureString(mitem.labelText).Y);
                }
            }

            //multipurpose oscillator
            osc += 0.05;
            if (osc > Math.PI) osc -= Math.PI;

            //update highlight color
            hlCol = new Color(0.2f, (float)Math.Sin(osc), 1f-(float)Math.Sin(osc));
        }

        public bool CatchClick(Point clickLoc, MouseState mouse)
        {
            bool caught = false;
            foreach (Box thisbox in boxes)
            {
                if (thisbox.Rect.Contains(clickLoc))
                {
                    caught = true;
                }
            }
            return caught;
        }

        public GameState ProcessInput(GameLogic gamelogic, GameState gameState, KeyboardState oKS, KeyboardState nKS, MouseState oMS, MouseState nMS, Hero hero, Map dMap)
        {
            if (gameState == GameState.Running)
            {
                //adjust log
                if (nKS.IsKeyDown(Keys.OemMinus) && oKS.IsKeyUp(Keys.OemMinus)) LogScrollUp();
                else if (nKS.IsKeyDown(Keys.OemPlus) && oKS.IsKeyUp(Keys.OemPlus)) LogScrollDown();

                //select actions
                if (nKS.IsKeyDown(Keys.D1) && oKS.IsKeyUp(Keys.D1)) selAct = 0;
                else if (nKS.IsKeyDown(Keys.D2) && oKS.IsKeyUp(Keys.D2)) selAct = 1;
                else if (nKS.IsKeyDown(Keys.D3) && oKS.IsKeyUp(Keys.D3)) selAct = 2;
                else if (nKS.IsKeyDown(Keys.D4) && oKS.IsKeyUp(Keys.D4)) selAct = 3;
                else if (nKS.IsKeyDown(Keys.D5) && oKS.IsKeyUp(Keys.D5)) selAct = 4;
                else if (nKS.IsKeyDown(Keys.D6) && oKS.IsKeyUp(Keys.D6)) selAct = 5;
                else if (nKS.IsKeyDown(Keys.D7) && oKS.IsKeyUp(Keys.D7)) selAct = 6;
                else if (nKS.IsKeyDown(Keys.D8) && oKS.IsKeyUp(Keys.D8)) selAct = 7;
                else if (nKS.IsKeyDown(Keys.D9) && oKS.IsKeyUp(Keys.D9)) selAct = 8;
                else if (nKS.IsKeyDown(Keys.D0) && oKS.IsKeyUp(Keys.D0)) selAct = 9;

                //Mouse Input
                Point mPoint = new Point(nMS.X, nMS.Y);
                Point mapMLoc = dMap.XYtoMap(nMS.X, nMS.Y);
                if (gD.Viewport.Bounds.Contains(mPoint))
                {
                    if (oMS.LeftButton != ButtonState.Pressed && nMS.LeftButton == ButtonState.Pressed)
                    {
                        if (CatchClick(mPoint, nMS))
                        {

                        }
                        else
                        {
                            //clicks on map
                            //Console.WriteLine(string.Format("Click at {0}, {1}",tarpoint.X, tarpoint.Y));
                            if (dMap.IsInMap(mapMLoc) && dMap.vision[mapMLoc.X, mapMLoc.Y] > 0 && dMap.blocks[mapMLoc.X, mapMLoc.Y] && !(hero.mapX == mapMLoc.X && hero.mapY == mapMLoc.Y))
                            {
                                hero.mQueue = dMap.PathTo(new Point(hero.mapX, hero.mapY), mapMLoc);
                            }
                        }
                    }

                    if (oMS.RightButton != ButtonState.Pressed && nMS.RightButton == ButtonState.Pressed)
                    {
                        if (CatchClick(mPoint, nMS))
                        {

                        }
                        else
                        {
                            //clicks on map
                            //Console.WriteLine(string.Format("Click at {0}, {1}",tarpoint.X, tarpoint.Y));
                            if (dMap.IsInMap(mapMLoc) && dMap.blocks[mapMLoc.X, mapMLoc.Y] && !(hero.mapX == mapMLoc.X && hero.mapY == mapMLoc.Y))
                            {
                                dMap.monsters.Add(new Monster(string.Format("M{0}", dMap.monsters.Count), 0, mapMLoc.X, mapMLoc.Y));
                                dMap.monsters[dMap.monsters.Count - 1].Move(mapMLoc.X, mapMLoc.Y, dMap);
                            }
                            else
                            {
                                foreach (Monster fiend in dMap.monsters)
                                {
                                    if (fiend.mapX == mapMLoc.X && fiend.mapY == mapMLoc.Y) fiend.Kill(dMap);
                                }
                            }
                        }
                    }
                }
            }
            else if (gameState == GameState.MainMenu)
            {
                foreach (MenuItem mitem in mainMenu)
                {
                    mitem.highlight = false;
                }
                foreach (MenuItem mitem in mainMenu)
                {
                    if (mitem.rect.Contains(new Point(nMS.X, nMS.Y)))
                    {
                        if (nMS.LeftButton == ButtonState.Pressed && oMS.LeftButton == ButtonState.Released)
                        {
                            if (mainMenu.IndexOf(mitem) == 0)
                            {
                                gamelogic.NewGame("testchar", 0);
                                return GameState.Running;
                            }
                            else if (mainMenu.IndexOf(mitem) == 1)
                            {
                                gamelogic.ContinueGame("testchar");
                                return GameState.Running;
                            }
                            else if (mainMenu.IndexOf(mitem) == 4)
                            {
                                return GameState.Graveyard;
                            }
                            else if (mainMenu.IndexOf(mitem) == 5)
                            {
                                gamelogic.Exit();
                            }
                        }
                        else if (mitem.highlight == false && mitem.enabled)
                        {
                            mitem.highlight = true;
                        }
                    }
                    
                }
            }
            else if (gameState == GameState.Graveyard)
            {
                foreach (MenuItem mitem in gyMenu)
                {
                    mitem.highlight = false;
                }
                foreach (MenuItem mitem in gyMenu)
                {
                    if (mitem.rect.Contains(new Point(nMS.X, nMS.Y)))
                    {
                        if (nMS.LeftButton == ButtonState.Pressed && oMS.LeftButton == ButtonState.Released)
                        {
                            if (gyMenu.IndexOf(mitem) == 0)
                            {
                                return GameState.MainMenu;
                            }
                        }
                        else if (mitem.highlight == false && mitem.enabled)
                        {
                            mitem.highlight = true;
                        }
                    }
                }
            }

            return gameState;
        }

        public void SaveGraves()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream outstream = File.OpenWrite("graves.bin");
            bf.Serialize(outstream, graves);
            outstream.Close();
        }

        public List<DeathCertificate> LoadGraves()
        {
            List<DeathCertificate> tgraves;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream reader = File.OpenRead("graves.bin");
            tgraves = (List<DeathCertificate>)bf.Deserialize(reader);
            reader.Close();

            return tgraves;
        }

    }
}
