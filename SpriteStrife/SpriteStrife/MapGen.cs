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
    public enum FloorTypes { Empty = 0, Floor, Wall, Feature };
    public enum Vision { Unexplored = 0, Explored, Visible };

    class SearchState
    {
        public int X, Y;
        public int Value;
        public List<Point> pathTo;

        public SearchState(int inX, int inY, int inV, List<Point> inP)
        {
            X = inX;
            Y = inY;
            Value = inV;
            pathTo = inP;
        }
    }

    [Serializable]
    struct MapObject
    {
        public int type;
        public int state;

        public MapObject(int otype, int ostate)
        {
            type = otype;
            state = ostate;
        }
    }

    [Serializable]
    class Map
    {
        public int xOffset, yOffset;
        public int tileSizeX, tileSizeY;
        public int tileSetSize;
        //public FloorTypes[,] floor;
        public FloorTypes[,] floor;
        public int[,] alts;
        public MapObject[,] objects;
        public bool[,] trans;
        public bool[,] blocks;
        public Vision[,] vision;
        public Point bounds;
        //public TextReader stream;
        public Point startingLocation;
        public int floorLevel;
        public List<Point> ringAround, boxAround;
        public List<Point> openSquares;
        public List<Monster> monsters;
        public List<Item> items;
        [NonSerialized] public IEnumerable<IEnumerable<Point>> rayList;


        public Map(int mapSizeX, int mapSizeY, int tilesizeX = 40, int tilesizeY = 40, int tilesetSize = 40)
        {
            floorLevel = 0;
            tileSizeX = tilesizeX;
            tileSizeY = tilesizeY;
            tileSetSize = tilesetSize;

            ringAround = new List<Point>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Point addpoint = new Point(i, j);
                    if (i != 0 || j != 0) { ringAround.Add(addpoint); }
                }
            }

            boxAround = new List<Point>();
            boxAround.Add(new Point(1, 0));
            boxAround.Add(new Point(-1, 0));
            boxAround.Add(new Point(0, 1));
            boxAround.Add(new Point(0, -1));

            floor = new FloorTypes[mapSizeX, mapSizeY];
            alts = new int[mapSizeX, mapSizeY];
            objects = new MapObject[mapSizeX, mapSizeY];
            trans = new bool[mapSizeX, mapSizeY];
            blocks = new bool[mapSizeX, mapSizeY];
            vision = new Vision[mapSizeX, mapSizeY];
            bounds = new Point(mapSizeX, mapSizeY);

            monsters = new List<Monster>();
            items = new List<Item>();

            openSquares = new List<Point>();

            //build ray table for LoS
            rayList = BuildRayList(5);
        }

        /// <summary>
        /// Converts screen coordinates to map coordinates.
        /// </summary>
        public Point XYtoMap(int inX, int inY)
        {
            int mx = (inX - xOffset) / tileSizeX;
            int my = (inY - yOffset) / tileSizeY;
            return new Point(mx, my);
        }

        /// <summary>
        /// Determines whether a given point is within the map to prevent errors.
        /// </summary>
        /// <param name="tpoint">Specifies the point to be tested.</param>
        public bool IsInMap(Point tpoint)
        {
            return ((tpoint.X >= 0) && (tpoint.Y >= 0) && (tpoint.X < bounds.Y) && (tpoint.Y < bounds.Y));
        }

        public void Recalc()
        {
            for (int x = 0; x < bounds.X; x++)
            {
                for (int y = 0; y < bounds.Y; y++)
                {
                    if (floor[x, y] == FloorTypes.Empty || floor[x, y] == FloorTypes.Wall)
                    {
                        blocks[x, y] = false;
                        trans[x, y] = false;
                    }
                    else if (floor[x, y] == FloorTypes.Floor)
                    {
                        blocks[x, y] = true;
                        trans[x, y] = true;
                        openSquares.Add(new Point(x, y));
                    }
                    else if (floor[x, y] == FloorTypes.Feature)
                    {
                        blocks[x, y] = false;
                        trans[x, y] = true;
                    }
                }
            }
            foreach (Monster fiend in monsters)
            {
                blocks[fiend.mapX, fiend.mapY] = false;
            }
        }

        /// <summary>
        /// Attempts to find a path between two points using A* search.
        /// </summary>
        /// <param name="reqVision">If true, only plots paths through explored areas.</param>
        public List<Point> PathTo(Point pointA, Point pointB, bool reqVision = true, bool ignoreMobs = false)
        {
            //initialize search variables
            int totalnodes = 0;
            int expiry = 50;
            List<SearchState> nodes = new List<SearchState>();
            int[,] searchSpace = new int[bounds.X, bounds.Y];
            searchSpace[pointA.X, pointA.Y] = 1;

            //create first node
            int cx = pointA.X;
            int cy = pointA.Y;
            int tv = Math.Abs(cx - pointB.X) + Math.Abs(cy - pointB.Y) + 1;
            nodes.Add(new SearchState(cx, cy, tv, new List<Point>()));


            while (expiry > 0)
            {
                //failure due to incontiguity (no valid nodes)
                if (nodes.Count == 0)
                {
                    //Console.WriteLine("Unable to find path!");
                    return new List<Point>();
                }

                //expand from cheapest node
                nodes.Sort(delegate(SearchState s1, SearchState s2) { return s1.Value.CompareTo(s2.Value); });
                cx = nodes[0].X;
                cy = nodes[0].Y;
                nodes[0].pathTo.Add(new Point(cx, cy));
                totalnodes += 1;

                //Console.WriteLine(string.Format("Expanding node (cost {0}, path length {4}) of {1} total at {2}, {3}...",nodes[0].Value,nodes.Count,cx,cy, nodes[0].pathTo.Count));

                //test adjacent points
                foreach (Point p in boxAround)
                {
                    int tx = cx + p.X;
                    int ty = cy + p.Y;

                    //check for goal
                    if (tx == pointB.X && ty == pointB.Y)
                    {
                        nodes[0].pathTo.Add(new Point(tx, ty));
                        nodes[0].pathTo.RemoveAt(0);
                        //Console.WriteLine(string.Format("Found path! (length {0}, {1} nodes expanded)", nodes[0].pathTo.Count, totalnodes));
                        return nodes[0].pathTo;
                    }

                    //add valid successors to node list
                    if (searchSpace[tx, ty] != 1 && (vision[tx, ty] > 0 || !reqVision) && ((floor[tx, ty] == FloorTypes.Floor && ignoreMobs) || (blocks[tx, ty] && !ignoreMobs)))
                    {
                        tv = Math.Abs(tx - pointB.X) + Math.Abs(ty - pointB.Y) + nodes[0].pathTo.Count;  //heuristic for node cost: x distance remaining + y distance remaining + distance traveled
                        nodes.Add(new SearchState(tx, ty, tv, new List<Point>(nodes[0].pathTo)));
                    }
                    searchSpace[tx, ty] = 1;

                }
                nodes.RemoveAt(0);
                expiry -= 1;
            }

            //failure due to expiry (too many tries)
            //Console.WriteLine("Unable to find path!");
            return new List<Point>();
        }

        public bool LoSLine(int xA, int yA, int xB, int yB)
        {
            foreach (Point thisPoint in BresLine.RenderLine(new Point(xA, yA), new Point(xB, yB)))
            {
                if (!trans[thisPoint.X, thisPoint.Y])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates LoS based on a pregenerated ray table.
        /// </summary>
        public void DoSight(Hero refmob, bool globalSight = false)
        {
            for (int x = 0; x < bounds.X; x++)
            {
                for (int y = 0; y < bounds.Y; y++)
                {
                    if (vision[x, y] == Vision.Visible) { vision[x, y] = Vision.Explored; }

                    //DEBUG SIGHT
                    if (globalSight) vision[x, y] = Vision.Visible;
                }
            }

            Point cpoint = new Point(refmob.mapX, refmob.mapY);
            foreach (IEnumerable<Point> thisray in rayList)
            {
                foreach (Point thispoint in thisray)
                {

                    Point tpoint = new Point(thispoint.X + cpoint.X, thispoint.Y + cpoint.Y);
                    if (IsInMap(tpoint))
                    {
                        vision[tpoint.X, tpoint.Y] = Vision.Visible;
                        if (!trans[tpoint.X, tpoint.Y]) { break; }
                    }
                }
            }



            //IEnumerable<Point> visarea = rasterCircle(cpoint, 5);
            //foreach (Point thispoint in visarea)
            //{
            //    if (IsInMap(thispoint)) { dMap.vision[thispoint.X, thispoint.Y] = 1; }
            //}
        }

        /// <summary>
        /// Generates a ray table for LoS calculations.
        /// </summary>
        /// <param name="vrange">Maximum view distance.</param>
        public IEnumerable<IEnumerable<Point>> BuildRayList(int vrange)
        {
            Point orig = new Point(0, 0);
            for (int crange = 2; crange <= vrange; crange++)
            {
                IEnumerable<Point> thisrad = rasterCircle(orig, crange);
                foreach (Point thispoint in thisrad)
                {
                    yield return BresLine.RenderLine(orig, thispoint);
                }
            }
        }

        // Bresenham line algorithm (optimized)
        public static class BresLine
        {
            /// <summary>
            /// Creates a line from Begin to End starting at (x0,y0) and ending at (x1,y1)
            /// </summary>
            public static IEnumerable<Point> RenderLine(Point begin, Point end)
            {
                List<Point> retpath = new List<Point>();
                bool swapped = false;
                bool steep = Math.Abs(end.Y - begin.Y) > Math.Abs(end.X - begin.X);
                if (steep)
                {
                    int j = begin.X;
                    begin.X = begin.Y;
                    begin.Y = j;
                    int i = end.X;
                    end.X = end.Y;
                    end.Y = i;

                }
                if (begin.X > end.X)
                {
                    int j = begin.X;
                    begin.X = end.X;
                    end.X = j;
                    int i = begin.Y;
                    begin.Y = end.Y;
                    end.Y = i;
                    swapped = true;
                }
                int deltax = end.X - begin.X;

                int deltay = Math.Abs(end.Y - begin.Y);
                double error = deltax / 2.0;
                int ystep;
                if (begin.Y < end.Y) { ystep = 1; }
                else { ystep = -1; }

                int y = begin.Y;

                //Point thispoint = new Point(begin.X, y);
                for (int x = begin.X; x <= end.X; x++)
                {
                    Point thispoint = new Point();
                    if (steep)
                    {
                        thispoint.X = y;
                        thispoint.Y = x;

                    }

                    else
                    {
                        thispoint.X = x;
                        thispoint.Y = y;

                    }

                    retpath.Add(thispoint);

                    error -= deltay;
                    if (error < 0)
                    {
                        y += ystep;
                        error += deltax;
                    }

                }

                if (swapped) { retpath.Reverse(); }

                return retpath;
            }
        }

        // Midpoint circle algorithm
        public static IEnumerable<Point> rasterCircle(Point midpoint, int radius)
        {
            Point returnPoint = new Point();
            int f = 1 - radius;
            int ddF_x = 1;
            int ddF_y = -2 * radius;
            int x = 0;
            int y = radius;

            returnPoint.X = midpoint.X;
            returnPoint.Y = midpoint.Y + radius;
            yield return returnPoint;
            returnPoint.Y = midpoint.Y - radius;
            yield return returnPoint;
            returnPoint.X = midpoint.X + radius;
            returnPoint.Y = midpoint.Y;
            yield return returnPoint;
            returnPoint.X = midpoint.X - radius;
            yield return returnPoint;

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;
                returnPoint.X = midpoint.X + x;
                returnPoint.Y = midpoint.Y + y;
                yield return returnPoint;
                returnPoint.X = midpoint.X - x;
                returnPoint.Y = midpoint.Y + y;
                yield return returnPoint;
                returnPoint.X = midpoint.X + x;
                returnPoint.Y = midpoint.Y - y;
                yield return returnPoint;
                returnPoint.X = midpoint.X - x;
                returnPoint.Y = midpoint.Y - y;
                yield return returnPoint;
                returnPoint.X = midpoint.X + y;
                returnPoint.Y = midpoint.Y + x;
                yield return returnPoint;
                returnPoint.X = midpoint.X - y;
                returnPoint.Y = midpoint.Y + x;
                yield return returnPoint;
                returnPoint.X = midpoint.X + y;
                returnPoint.Y = midpoint.Y - x;
                yield return returnPoint;
                returnPoint.X = midpoint.X - y;
                returnPoint.Y = midpoint.Y - x;
                yield return returnPoint;
            }
        }
    }

    class MapGen
    {
        Random rand;
        //FloorTypes[,] map;
        //int[,] alts;
        int xSize, ySize;
        List<Point> ringAround;
        List<Point> boxAround;

        public MapGen(int xsize = 64, int ysize = 64)
        {
            xSize = xsize;
            ySize = ysize;

            rand = new Random();

            ringAround = new List<Point>();
            for (int i = -1; i < 2; i++)
			{
			    for (int j = -1; j < 2; j++)
			    {
                    Point addpoint = new Point(i, j);
			        if (i != 0 || j != 0) { ringAround.Add(addpoint); }
			    }
			}

            boxAround = new List<Point>();
            boxAround.Add(new Point(1, 0));
            boxAround.Add(new Point(-1, 0));
            boxAround.Add(new Point(0, 1));
            boxAround.Add(new Point(0, -1));

            
        }

        public Map LoadMap(string loadName, int floorLevel)
        {
            Map map;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream reader = File.OpenRead(string.Format("{0}\\floor{1}.map", loadName, floorLevel));
            map = (Map)bf.Deserialize(reader);
            reader.Close();

            map.rayList = map.BuildRayList(5);
            map.Recalc();

            return map;
        }

        public void SaveMap(string saveName, Map mapToSave)
        {
            //try
            //{
                if (!System.IO.Directory.Exists(saveName)) System.IO.Directory.CreateDirectory(saveName);
                BinaryFormatter bf = new BinaryFormatter();
                FileStream outstream = File.OpenWrite(string.Format("{0}\\floor{1}.map", saveName, mapToSave.floorLevel));
                bf.Serialize(outstream, mapToSave);
                outstream.Close();
            //}
            //catch (Exception)
            //{
                
            //}
            
        }

        public Map GenerateMap(int floorLevel = 0, int genMethod = 1)
        {

            //initialize map array
            Map newMap = new Map(xSize, ySize);
            //newMap.floor = new FloorTypes[xSize, ySize];
            //newMap.alts = new int[xSize, ySize];
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    newMap.floor[x, y] = FloorTypes.Empty;
                    newMap.alts[x, y] = 0;
                    newMap.objects[x, y] = new MapObject(-1, 0);
                }
            }

            //generate map
            if (genMethod == 1)
            {
                //SEED + EROSION
                List<Point> seeds = new List<Point>();
                for (int s = 0; s < 3; s++) seeds.Add(new Point(rand.Next((xSize / 2) - 6) + 3, rand.Next((ySize / 2) - 6) + 3));
                for (int s = 0; s < 3; s++) seeds.Add(new Point(rand.Next((xSize / 2) - 6) + 2 + (xSize / 2), rand.Next((ySize / 2) - 6) + 3));
                for (int s = 0; s < 3; s++) seeds.Add(new Point(rand.Next((xSize / 2) - 6) + 2 + (xSize / 2), rand.Next((ySize / 2) - 6) + 2 + (ySize / 2)));
                for (int s = 0; s < 3; s++) seeds.Add(new Point(rand.Next((xSize / 2) - 6) + 3, rand.Next((ySize / 2) - 6) + 2 + (ySize / 2)));

                //erode rooms
                foreach (Point s in seeds)
                {
                    List<Point> erode = new List<Point>();
                    int esize = 15 + rand.Next(15);
                    
                    foreach (Point thispoint in ringAround)
                    {
                        if (newMap.floor[s.X + thispoint.X, s.Y + thispoint.Y] == FloorTypes.Empty)
                        {
                            newMap.floor[s.X + thispoint.X, s.Y + thispoint.Y] = FloorTypes.Floor;
                            if (rand.Next(2) == 1) erode.Add(new Point(s.X + thispoint.X, s.Y + thispoint.Y));
                        }
                    }

                    //newMap.objects[s.X, s.Y] = new MapObject(0, 0);

                    //Console.WriteLine(string.Format("Eroding {0} points from seed...", erode.Count));

                    while (erode.Count > 0)
                    {
                        foreach (Point thispoint in ringAround)
                        {
                            if (newMap.floor[erode[0].X + thispoint.X, erode[0].Y + thispoint.Y] == FloorTypes.Empty)
                            {
                                if (rand.Next(10) < 7 && erode[0].X > 2 && erode[0].X < xSize - 2 && erode[0].Y > 2 && erode[0].Y < ySize - 2)
                                {
                                    newMap.floor[erode[0].X + thispoint.X, erode[0].Y + thispoint.Y] = FloorTypes.Floor;
                                    if (rand.Next(3) == 1 && esize > 0)
                                    {
                                        erode.Add(new Point(erode[0].X + thispoint.X, erode[0].Y + thispoint.Y));
                                        esize -= 1;

                                        //Console.WriteLine(string.Format("New erosion seed at {0}, {1} ({2} unexpanded, {3} remaining)", erode[0].X + thispoint.X, erode[0].Y + thispoint.Y, erode.Count, esize));
                                    }
                                }
                            }
                        }

                        erode.RemoveAt(0);
                    }
                }

                //connect hallways
                int tx = seeds[seeds.Count - 1].X;
                int ty = seeds[seeds.Count - 1].Y;
                while (tx != seeds[0].X)
                {
                    if (tx < seeds[0].X) tx += 1;
                    else if (tx > seeds[0].X) tx -= 1;
                    newMap.floor[tx, ty] = FloorTypes.Floor;
                }
                while (ty != seeds[0].Y)
                {
                    if (ty < seeds[0].Y) ty += 1;
                    else if (ty > seeds[0].Y) ty -= 1;
                    newMap.floor[tx, ty] = FloorTypes.Floor;
                }
                for (int i = 1; i < seeds.Count; i++)
                {
                    tx = seeds[i - 1].X;
                    ty = seeds[i - 1].Y;
                    while (tx != seeds[i].X)
                    {
                        if (tx < seeds[i].X) tx += 1;
                        else if (tx > seeds[i].X) tx -= 1;
                        newMap.floor[tx, ty] = FloorTypes.Floor;
                    }
                    while (ty != seeds[i].Y)
                    {
                        if (ty < seeds[i].Y) ty += 1;
                        else if (ty > seeds[i].Y) ty -= 1;
                        newMap.floor[tx, ty] = FloorTypes.Floor;
                    }
                }


                //expand walls
                for (int y = 1; y < ySize - 1; y++)
                {
                    for (int x = 1; x < xSize - 1; x++)
                    {
                        if (newMap.floor[x, y] == FloorTypes.Floor)
                        {
                            foreach (Point thispoint in ringAround)
                            {
                                if (newMap.floor[x + thispoint.X, y + thispoint.Y] == FloorTypes.Empty)
                                {
                                    newMap.floor[x + thispoint.X, y + thispoint.Y] = FloorTypes.Wall;
                                }
                            }
                        }
                    }
                }

                //clean up bits
                for (int y = 1; y < ySize - 1; y++)
                {
                    for (int x = 1; x < xSize - 1; x++)
                    {
                        if (newMap.floor[x, y] == FloorTypes.Wall)
                        {
                            int floorcheck = 0;
                            foreach (Point thispoint in boxAround)
                            {
                                if (newMap.floor[x + thispoint.X, y + thispoint.Y] == FloorTypes.Floor)
                                {
                                    floorcheck += 1;
                                }
                            }
                            if (floorcheck == 4)
                            {
                                newMap.floor[x, y] = FloorTypes.Floor;
                                //Console.WriteLine(string.Format("Removing stray wall bit at {0}, {1}", x, y));
                            }
                        }
                        else if (newMap.floor[x, y] == FloorTypes.Floor)
                        {
                            int floorcheck = 0;
                            foreach (Point thispoint in boxAround)
                            {
                                if (newMap.floor[x + thispoint.X, y + thispoint.Y] == FloorTypes.Floor)
                                {
                                    floorcheck += 1;
                                }
                            }
                            if (floorcheck == 0)
                            {
                                newMap.floor[x, y] = FloorTypes.Wall;
                                //Console.WriteLine(string.Format("Removing stray floor bit at {0}, {1}", x, y));
                            }
                        }
                    }
                }

                //random alts
                for (int y = 0; y < ySize; y++)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        if (newMap.floor[x, y] == FloorTypes.Floor || newMap.floor[x, y] == FloorTypes.Wall)
                        {
                            if (rand.Next(10) == 0) //common
                            {
                                if (rand.Next(10) == 0) //rare
                                {
                                    newMap.alts[x, y] = rand.Next(6, 11);
                                }
                                else
                                {
                                    newMap.alts[x, y] = rand.Next(1, 6);
                                }
                            }
                        }
                    }
                }

                foreach (Point s in seeds)
                {
                    newMap.floor[s.X, s.Y] = FloorTypes.Feature;
                    newMap.alts[s.X, s.Y] = 3;
                }
            }

            newMap.Recalc();

            //throw in some items
            for (int i = 0; i < 10; i++)
            {
                Point itemloc = newMap.openSquares[rand.Next(newMap.openSquares.Count)];
                //newMap.items.Add(new Item(String.Format("I{0}", newMap.items.Count), 0, itemloc.X, itemloc.Y));
            }

            newMap.startingLocation = newMap.openSquares[rand.Next(newMap.openSquares.Count)];

            return newMap;
        }
    }

    // Bresenham line algorithm (optimized)
    public static class BresLine
    {
        /// <summary>
        /// Creates a line from Begin to End starting at (x0,y0) and ending at (x1,y1)
        /// </summary>
        public static IEnumerable<Point> RenderLine(Point begin, Point end)
        {
            List<Point> retpath = new List<Point>();
            bool swapped = false;
            bool steep = Math.Abs(end.Y - begin.Y) > Math.Abs(end.X - begin.X);
            if (steep)
            {
                int j = begin.X;
                begin.X = begin.Y;
                begin.Y = j;
                int i = end.X;
                end.X = end.Y;
                end.Y = i;

            }
            if (begin.X > end.X)
            {
                int j = begin.X;
                begin.X = end.X;
                end.X = j;
                int i = begin.Y;
                begin.Y = end.Y;
                end.Y = i;
                swapped = true;
            }
            int deltax = end.X - begin.X;

            int deltay = Math.Abs(end.Y - begin.Y);
            double error = deltax / 2.0;
            int ystep;
            if (begin.Y < end.Y) { ystep = 1; }
            else { ystep = -1; }

            int y = begin.Y;

            //Point thispoint = new Point(begin.X, y);
            for (int x = begin.X; x <= end.X; x++)
            {
                Point thispoint = new Point();
                if (steep)
                {
                    thispoint.X = y;
                    thispoint.Y = x;

                }

                else
                {
                    thispoint.X = x;
                    thispoint.Y = y;

                }

                retpath.Add(thispoint);



                error -= deltay;
                if (error < 0)
                {
                    y += ystep;
                    error += deltax;
                }

            }

            if (swapped) { retpath.Reverse(); }

            return retpath;
        }
    }
}
