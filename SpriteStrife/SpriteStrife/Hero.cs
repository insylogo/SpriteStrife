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
    [Serializable]
    class Hero
    {
        public string name;
        public static Rectangle Shape = new Rectangle(280, 280, 40, 40);
        public int type;
        public int mapX, mapY;
        public List<Point> mQueue;
        public float delay;

        public Hero(string hname, int htype)
        {
            mapX = 1;
            mapY = 1;
            type = htype;
            mQueue = new List<Point>();
            delay = 0;
            name = hname;
        }

        public void Move(int newX, int newY, Map dMap, bool clearQueue = true)
        {
            if (clearQueue) mQueue.Clear();
            if (delay == 0 && dMap.IsInMap(new Point(newX, newY)) && dMap.blocks[newX, newY])
            {
                dMap.blocks[mapX, mapY] = true;
                mapX = newX;
                mapY = newY;
                delay += 1;
            }
            dMap.blocks[mapX, mapY] = false;
        }

        public void MoveFromQueue(Map dMap)
        {
            if (delay == 0 && mQueue.Count > 0)
            {
                Move(mQueue[0].X, mQueue[0].Y, dMap, false);
                mQueue.RemoveAt(0);
                if (mQueue.Count == 0 || delay == 0) mQueue.Clear();
            }
            else mQueue.Clear();
        }

    }
    class HeroGen
    {
        public HeroGen()
        {

        }

        public Hero LoadHero(string loadName)
        {
            Hero hero;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream reader = File.OpenRead(string.Format("{0}\\hero.mob", loadName));
            hero = (Hero)bf.Deserialize(reader);
            reader.Close();

            return hero;
        }

        public void SaveHero(Hero heroToSave)
        {
            //try
            //{
            if (!System.IO.Directory.Exists(heroToSave.name)) System.IO.Directory.CreateDirectory(heroToSave.name);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream outstream = File.OpenWrite(string.Format("{0}\\hero.mob", heroToSave.name));
            bf.Serialize(outstream, heroToSave);
            outstream.Close();
            //}
            //catch (Exception)
            //{

            //}

        }
    }
}
