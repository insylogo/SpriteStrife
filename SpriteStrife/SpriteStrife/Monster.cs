using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpriteStrife
{
    [Serializable]
    public enum AIMode { Wander = 0, Alert, Aggro };

    [Serializable]
    class Monster
    {
        public static Rectangle Shape = new Rectangle(280, 280, 40, 40);
        public int type;
        public int mapX, mapY;
        public List<Point> mQueue;
        public float delay;
        public AIMode curAIMode;
        public Point curTarLoc;
        public Random rando;
        public bool alive;
        public string name;

        public Monster(string mName, int mtype, int mapx, int mapy)
        {
            name = mName;
            mapX = mapx;
            mapY = mapy;
            type = mtype;
            mQueue = new List<Point>();
            delay = 0;
            curAIMode = 0;
            curTarLoc = new Point(0, 0);
            rando = new Random();
            alive = true;
        }

        public void Act(Hero hero, Map dMap)
        {

            //check for hero in sight
            if (Vector2.Distance(new Vector2(hero.mapX, hero.mapY), new Vector2(mapX, mapY)) < 6 && dMap.LoSLine(mapX, mapY, hero.mapX, hero.mapY))
            {
                curAIMode = AIMode.Aggro;
                curTarLoc = new Point(hero.mapX, hero.mapY);
                if (Math.Abs(mapX - hero.mapX) + Math.Abs(mapY - hero.mapY) != 1)
                {
                    mQueue = dMap.PathTo(new Point(mapX, mapY), curTarLoc, false, false);
                    if (mQueue.Count > 20 || mQueue.Count == 0)
                    {
                        //Console.WriteLine(string.Format("{0} found unusable path (length {1}) - calculating direct path",name,mQueue.Count));
                        mQueue = dMap.PathTo(new Point(mapX, mapY), curTarLoc, false, true);
                        //Console.WriteLine(string.Format("{0} found direct path (length {1})", name, mQueue.Count));
                    }
                }
            }

            //act accordingly
            if (curAIMode == AIMode.Wander)
            {
                while (mQueue.Count == 0)
                {
                    curTarLoc = dMap.openSquares[rando.Next(dMap.openSquares.Count)];
                    if (dMap.blocks[curTarLoc.X, curTarLoc.Y])
                    {
                        mQueue = dMap.PathTo(new Point(mapX, mapY), curTarLoc, false);
                    }
                }
                MoveFromQueue(dMap);
            }
            if (curAIMode == AIMode.Alert)
            {
                MoveFromQueue(dMap);
                if (mQueue.Count == 0) curAIMode = AIMode.Wander;
            }
            if (curAIMode == AIMode.Aggro)
            {
                if (dMap.LoSLine(mapX, mapY, hero.mapX, hero.mapY))
                {
                    //Console.WriteLine(string.Format("Moving from {0}, {1} to {2}, {3}", mapX, mapY, mQueue[0].X, mQueue[0].Y));
                    if (Math.Abs(mapX - hero.mapX) + Math.Abs(mapY - hero.mapY) == 1)
                    {
                        Attack(hero);
                    }
                    else  MoveFromQueue(dMap);
                }
                else
                {
                    curAIMode = AIMode.Alert;
                }
            }
        }

        public void Attack(Hero hero)
        {
            delay += 1;
            Console.WriteLine(string.Format("{0} Attacked a hero!", name));
        }

        public bool Move(int newX, int newY, Map dMap, bool clearQueue = true)
        {
            if (clearQueue) mQueue.Clear();
            if (dMap.IsInMap(new Point(newX, newY)) && dMap.blocks[newX, newY])
            {
                dMap.blocks[mapX, mapY] = true;
                dMap.blocks[newX, newY] = false;
                mapX = newX;
                mapY = newY;
                delay += 1;
                //Console.WriteLine(string.Format("{0} moved!",name));
                return true;
            }
            //Console.WriteLine(string.Format("{0} unable to move!", name));
            return false;
        }

        public void MoveFromQueue(Map dMap)
        {
            bool didMove = false;
            if (mQueue.Count > 0)
            {
                didMove = Move(mQueue[0].X, mQueue[0].Y, dMap, false);
                mQueue.RemoveAt(0); 
            }
            if (!didMove)
            {
                mQueue.Clear();
                delay += 0.5f;
            }
        }

        public void RandLocation(Map dMap)
        {
            while (dMap.blocks[mapX, mapY])
            {
                int pointchoice = rando.Next(dMap.openSquares.Count);
                mapX = dMap.openSquares[pointchoice].X;
                mapY = dMap.openSquares[pointchoice].Y;
            }
        }

        public void Kill(Map dMap)
        {
            alive = false;
            dMap.blocks[mapX, mapY] = true;
            Console.WriteLine("Killed a monster!");
        }
    }
}
