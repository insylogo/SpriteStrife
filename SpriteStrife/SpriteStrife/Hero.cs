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
    enum HeroClasses
    {
        Warrior = 0, Wizard, Priest
    }

    [Serializable]
    class Hero
    {
        public string name;
        public static Rectangle Shape = new Rectangle(280, 280, 40, 40);
        public int type;
        public int mapX, mapY;
        public List<Point> mQueue;
        public float delay;
        public StatSystem stats;
        public HeroClass chosenClass;

        public Hero(string hname, int htype)
        {
            mapX = 1;
            mapY = 1;
            type = htype;
            mQueue = new List<Point>();
            delay = 0;
            name = hname;
            stats = new StatSystem();
            stats.SetBaseStat(StatType.Health, 23);
            stats.SetBaseStat(StatType.Vitality, 13);
            stats.SetBaseStat(StatType.Sanity, 8);
            stats.DrainStat(StatType.Health, 9);

            if ((HeroClasses)type == HeroClasses.Warrior)
            {
                chosenClass = new HeroClass("Warrior",
                                            new StatSystem(
                                                strength: 10,
                                                toughness: 5,
                                                health: 30,
                                                intellect: 3,
                                                perception: 5,
                                                sanity: 15,
                                                faith: 3,
                                                wisdom: 2,
                                                vitality: 20),
                                            new StatSystem(
                                                strength: 3,
                                                toughness: 2,
                                                health: 30,
                                                intellect: 3,
                                                perception: 5,
                                                sanity: 15,
                                                faith: 3,
                                                wisdom: 2,
                                                vitality: 20));
                chosenClass.AddPower(new Power(
                                                 name: "Rage", 
                                                 aoePattern: new TargetPattern(), 
                                                 cd: 30, 
                                                 passive: true,
                                                 statEffect:
                                                            new StatSystem(
                                                            strength: 10,
                                                            toughness: 10,
                                                            health: 10,
                                                            intellect: -3,
                                                            perception: -1,
                                                            sanity: 0,
                                                            faith: 0,
                                                            wisdom: -2,
                                                            vitality: 0),
                                                  statCost: 
                                                            new StatSystem(
                                                            strength: 0,
                                                            toughness: 0,
                                                            health: 0,
                                                            intellect: 0,
                                                            perception: 0,
                                                            sanity: 5,
                                                            faith: 0,
                                                            wisdom: 0,
                                                            vitality: 5),
                                                   dur: 5,
                                                   firingRange: 0), 
                                       levelReq: 0);
                                      

            }
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
