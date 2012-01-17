using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml; 
using System.Xml.Linq;

namespace SpriteStrife
{
    static class DBLoader
    {
        public static string LoadDB (string path, ref List<Item> items)
        {

            XDocument doc = XDocument.Load(path);
            
            foreach (var item in doc.Descendants("Item"))
            {
                Console.WriteLine("{0} - {1}\n", item.Name, item.Value);
            }
            
            return "Success";

        }

        public static void SaveDB(string path, List<Item> items)
        {

            List<XElement> itemsXml = new List<XElement>();
            int index = 0;

            foreach (var item in items)
            {
                itemsXml.Add(createItemNode(item, index++));
            }

            XElement trunk = new XElement("Items", itemsXml.ToArray());
            XDocument doc = new XDocument(trunk);
            doc.Save(path);
            
          
            

        }

        private static XElement createItemNode(Item item, int index)
        {
            XElement output = new XElement("Item",
                new XElement("ID", index),
                new XElement("Name", item.Name),
                new XElement("Stats",
                    new XElement("Strength", item.Stats.Strength),
                    new XElement("Toughness", item.Stats.Toughness),
                    new XElement("Health", item.Stats.Health),

                    new XElement("Intellect", item.Stats.Intellect),
                    new XElement("Perception", item.Stats.Perception),
                    new XElement("Sanity", item.Stats.Sanity),

                    new XElement("Spirit", item.Stats.Faith),
                    new XElement("Wisdom", item.Stats.Wisdom),
                    new XElement("Vitality", item.Stats.Vitality)),
                new XElement("Value", item.Value),
                new XElement("Type", item.Type),
                new XElement("Ammo", item.Ammo),
                new XElement("Count", item.Count),
                new XElement("Damage", item.Damage),
                new XElement("Quality", item.Quality));

            return output;
    
        }



    }
}
