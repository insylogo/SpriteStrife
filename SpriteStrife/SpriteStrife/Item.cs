using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
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
    enum ItemType
    {
        None = 0, Hands, Feet,
        Chest, Head, Neck, Ring, Charm,
        OneHandedMelee, TwoHandedMelee, OneHandedRanged,
        TwoHandedRanged, Ammunition, Shield, Potion,
        Food, Scroll, Gold
    }

    enum ItemQuality
    {
        None = 0, Normal, Damaged,
        Superior, Magic, Legendary
    }

    enum AmmoType
    {
        None = 0, Arrows, Bolts,
        Thrown, Bullets, Magic
    }

    enum DamageType
    {
        None = 0, Physical, Magical,
        Spiritual, Fire, Cold, Lightning
    }


    [Serializable]
    class Item
    {

        public static Rectangle Shape = new Rectangle(280, 280, 40, 40);

        public int MapX;
        public int MapY;

        StatSystem attributes;
        ItemType type;

        DamageType damage;

        AmmoType ammo;

        public double Value { get; set; }

        public string Name { get; set; }
        public int Count { get; set; }
        public ItemQuality Quality { get; set; }

        public Item(
            string itemName = "Default",
            ItemType itemType = ItemType.None,
            StatSystem itemStats = null,
            DamageType damageType = DamageType.None,
            AmmoType itemShoots = AmmoType.None,
            ItemQuality itemQuality = ItemQuality.None,
            double value = 0.0,
            int x = 0,
            int y = 0,
            int quantity = 1)
        {
            if (itemStats != null)
            {
                Stats = itemStats;
            }
            else
            {
                Stats = new StatSystem();
            }
            Name = itemName;
            type = itemType;
            ammo = itemShoots;
            Count = quantity;
            Quality = itemQuality;
            damage = damageType;
            Value = value;
            MapX = x;
            MapY = y;
        }

        public AmmoType Ammo
        {
            get
            {
                return ammo;
            }
            set
            {
                ammo = value;
            }
        }

        public DamageType Damage
        {
            get
            {
                return damage;
            }
            set
            {
                damage = value;
            }
        }


        public ItemType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }



        public StatSystem Stats
        {
            get
            {
                return attributes;
            }
            set
            {
                attributes = value;
            }
        }

    }

    class ItemGen
    {

    }
}
