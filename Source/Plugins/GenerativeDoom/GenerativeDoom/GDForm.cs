using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Config;
using System.Threading;

namespace GenerativeDoom
{
    public partial class GDForm : Form
    {
        public const int TYPE_PLAYER_START = 1;

        private IList<DrawnVertex> points;

        Graph graph;
        Grammar gram;

        public int[] easyEnemies = { 3004, 9, 3001, 3006,  84 };
        public int[] mediumEnemies = { 3002, 58, 3005, 65 };
        public int[] hardEnemies = { 3003, 69, 71, 64, 66 };
        public int[] bosses = { 16, 7, 68 };

        public GDForm()
        {
            InitializeComponent();
            points = new List<DrawnVertex>();
        }

        // We're going to use this to show the form
        public void ShowWindow(Form owner)
        {
            // Position this window in the left-top corner of owner
            this.Location = new Point(owner.Location.X + 20, owner.Location.Y + 90);

            // Show it
            base.Show(owner);
        }

        // Form is closing event
        private void GDForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // When the user is closing the window we want to cancel this, because it
            // would also unload (dispose) the form. We only want to hide the window
            // so that it can be re-used next time when this editing mode is activated.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Just cancel the editing mode. This will automatically call
                // OnCancel() which will switch to the previous mode and in turn
                // calls OnDisengage() which hides this window.
                General.Editing.CancelMode();
                e.Cancel = true;
            }
        }

        private void GDForm_Load(object sender, EventArgs e)
        {

        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            General.Editing.CancelMode();
        }

        private void newSector(IList<DrawnVertex> points, int lumi, int ceil, int floor)
        {
            Console.Write("New sector: ");
            List<DrawnVertex> pSector = new List<DrawnVertex>();

            DrawnVertex v;
            v = new DrawnVertex();
            foreach (DrawnVertex p in points)
            {
                Console.Write(" p:"+p.pos);
                v.pos = p.pos;
                v.stitch = true;
                v.stitchline = true;
                pSector.Add(v);
            }

            Console.Write("\n");

            v.pos = points[0].pos;
            v.stitch = true;
            v.stitchline = true;

            pSector.Add(v);

            Tools.DrawLines(pSector);

            // Snap to map format accuracy
            General.Map.Map.SnapAllToAccuracy();

            // Clear selection
            General.Map.Map.ClearAllSelected();

            // Update cached values
            General.Map.Map.Update();

            // Edit new sectors?
            List<Sector> newsectors = General.Map.Map.GetMarkedSectors(true);

            foreach(Sector s in newsectors)
            {
                s.CeilHeight = ceil;
                s.FloorHeight = floor;
                s.Brightness = lumi;
            }

            // Update the used textures
            General.Map.Data.UpdateUsedTextures();

            // Map is changed
            General.Map.IsChanged = true;

            General.Interface.RedrawDisplay();

            //Ajoute, on enleve la marque sur les nouveaux secteurs
            General.Map.Map.ClearMarkedSectors(false);
        }

        private void newSector(DrawnVertex top_left, float width, float height, int lumi, int ceil, int floor)
        {
            points.Clear();
            DrawnVertex v;
            v = new DrawnVertex();
            v.pos = top_left.pos;
            points.Add(v);

            v.pos.y += height;
            points.Add(v);

            v.pos.x += width;
            
            points.Add(v);

            v.pos.y -= height;
            points.Add(v);

            newSector(points,lumi,ceil,floor);

            points.Clear();
        }

        private void addLinedef(Vertex begin, Vertex end)
        {
        }

        private Thing addThing(Vector2D pos, String category, float proba = 0.5f, int index = 0)
        {
            Thing t = addThing(pos);
            if (t != null)
            {

                IList<ThingCategory> cats = General.Map.Data.ThingCategories;
                Random r = new Random();

                bool found = false;
                if (index != 0)
                {
                    t.Type = index;
                    found = true;
                }
                else
                {
                    foreach (ThingTypeInfo ti in General.Map.Data.ThingTypes)
                    {
                        if (ti.Category.Name == category)
                        {
                            t.Type = ti.Index;
                            found = true;
                            if (r.NextDouble() > proba)
                                break;
                        }
                    }

                }
                if (!found)
                {
                    Console.WriteLine("###### Could not find category " + category + " for thing at pos " + pos);
                }else
                    t.Rotate(0);
            }
            else
            {
                Console.WriteLine("###### Could not add thing for cat " + category + " at pos " + pos);
            }

            return t;
        }

        private Thing addThing(Vector2D pos)
        {
            if (pos.x < General.Map.Config.LeftBoundary || pos.x > General.Map.Config.RightBoundary ||
                pos.y > General.Map.Config.TopBoundary || pos.y < General.Map.Config.BottomBoundary)
            {
                Console.WriteLine( "Error Generaetive Doom: Failed to insert thing: outside of map boundaries.");
                return null;
            }

            // Create thing
            Thing t = General.Map.Map.CreateThing();
            if (t != null)
            {
                General.Settings.ApplyDefaultThingSettings(t);

                t.Move(pos);

                t.UpdateConfiguration();

                // Update things filter so that it includes this thing
                General.Map.ThingsFilter.Update();

                // Snap to map format accuracy
                t.SnapToAccuracy();
            }

            return t;
        }

        private void correctMissingTex()
        {

            String defaulttexture = "-";
            if (General.Map.Data.TextureNames.Count > 1)
                defaulttexture = General.Map.Data.TextureNames[1];

            // Go for all the sidedefs
            foreach (Sidedef sd in General.Map.Map.Sidedefs)
            {
                // Check upper texture. Also make sure not to return a false
                // positive if the sector on the other side has the ceiling
                // set to be sky
                if (sd.HighRequired() && sd.HighTexture[0] == '-')
                {
                    if (sd.Other != null && sd.Other.Sector.CeilTexture != General.Map.Config.SkyFlatName)
                    {
                        sd.SetTextureHigh(General.Settings.DefaultCeilingTexture);
                    }
                }

                // Check middle texture
                if (sd.MiddleRequired() && sd.MiddleTexture[0] == '-')
                {
                    sd.SetTextureMid(General.Settings.DefaultTexture);
                }

                // Check lower texture. Also make sure not to return a false
                // positive if the sector on the other side has the floor
                // set to be sky
                if (sd.LowRequired() && sd.LowTexture[0] == '-')
                {
                    if (sd.Other != null && sd.Other.Sector.FloorTexture != General.Map.Config.SkyFlatName)
                    {
                        sd.SetTextureLow(General.Settings.DefaultFloorTexture);
                    }
                }

            }
        }

        private bool checkIntersect(Line2D measureline)
        {
            bool inter = false;
            foreach (Linedef ld2 in General.Map.Map.Linedefs)
            {
                // Intersecting?
                // We only keep the unit length from the start of the line and
                // do the real splitting later, when all intersections are known
                float u;
                if (ld2.Line.GetIntersection(measureline, out u))
                {
                    if (!float.IsNaN(u) && (u > 0.0f) && (u < 1.0f))
                    {
                        inter = true;
                        break;
                    }
                       
                }
            }

            Console.WriteLine("Chevk inter " + measureline + " is " + inter);

            return inter;
        }

        private bool checkIntersect(Line2D measureline, out float closestIntersect)
        {
            // Check if any other lines intersect this line
            List<float> intersections = new List<float>();
            foreach (Linedef ld2 in General.Map.Map.Linedefs)
            {
                // Intersecting?
                // We only keep the unit length from the start of the line and
                // do the real splitting later, when all intersections are known
                float u;
                if (ld2.Line.GetIntersection(measureline, out u))
                {
                   if (!float.IsNaN(u) && (u > 0.0f) && (u < 1.0f))
                        intersections.Add(u);
                }
            }

            if(intersections.Count() > 0)
            {
                // Sort the intersections
                intersections.Sort();

                closestIntersect = intersections.First<float>();

                return true;
            }

            closestIntersect = 0.0f;
            return false;

            
        }
    

        private void showCategories()
        {
            lbCategories.Items.Clear();
            IList<ThingCategory> cats = General.Map.Data.ThingCategories;
            foreach(ThingCategory cat in cats)
            {
                if (!lbCategories.Items.Contains(cat.Name))
                    lbCategories.Items.Add(cat.Name);
            }
            
        }

        private void makeOnePath(bool playerStart, int difficulty)
        {
            gram = new Grammar();
            graph = gram.Build();

            Linedef line = new Linedef(;
            
            Random r = new Random();

            DrawnVertex v = new DrawnVertex();

            float width = 512.0f;
            float height = 512.0f;

            int ceil = (int)(r.NextDouble() * 128 + 128);
            int floor = (int)(r.NextDouble() * 128 + 128);
            int pfloor = floor;
            int pceil = ceil;
            Vector2D pv = new Vector2D();

            floor = 0;
            ceil = 256;
            v.pos.y = 0;

            for (int i = 0; i < graph.size; i++)
            {
                Console.WriteLine("---------------------- Sector " + i);

                pv = v.pos;

                Vertex ver = graph.GetVertex(i);
                v.pos = ver.data.pos;

                newSector(v, ver.data.width,
                    ver.data.height,
                    200,
                    ver.data.ceil,
                    ver.data.floor);


                #region
                if (ver.data.type == "entry" && playerStart)
                {
                    Thing t = addThing(new Vector2D(v.pos.x + width / 2, v.pos.y + height / 2));
                    t.Type = TYPE_PLAYER_START;
                    t.Rotate(0);
                }
                else if (ver.data.type == "enemy")
                {
                    RandomNormal rz = new RandomNormal();
                    int bla = (int)rz.Generate(4, 2);

                    int enemyIndex = 0;
                    switch (difficulty)
                    {
                        case 0: enemyIndex = easyEnemies[new Random().Next(0, easyEnemies.Length - 1)];
                            break;
                        case 1:
                            enemyIndex = mediumEnemies[new Random().Next(0, mediumEnemies.Length - 1)];
                            break;
                        case 2:
                            enemyIndex = hardEnemies[new Random().Next(0, hardEnemies.Length - 1)];
                            break;
                    }


                    for (int j = 0; j <= bla; ++j)
                    {
                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "monsters", 0.5f, enemyIndex);
                    }
                }
                else if (ver.data.type == "bonus")
                {
                    do
                    {

                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "ammunition");
                    } while (r.NextDouble() > 0.3f);
                    do
                    {
                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "health");
                    } while (r.NextDouble() > 0.5f);
                }
                else if(ver.data.type == "weapon")
                {
                    addThing(new Vector2D(v.pos.x + width / 2, v.pos.y + height / 2), "weapons", 0.3f);
                }
                else if (ver.data.type == "boss")
                {
                    int enemyIndex = bosses[new Random().Next(0, bosses.Length - 1)];
                    addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "monsters", 0.5f, enemyIndex);
                }
                #endregion


                float cwidth = 0;
                float cheight = 0;

                float dheight = 0;
                float dwidth = 0;
                DrawnVertex d = new DrawnVertex();

                //Building of the corridors
                if(ver.data.type != "boss")
                {
                    Direction nextDir = ver.GetNeighbour(0);
                    Vertex next = ver.GetNeighbour(nextDir);
                    switch (nextDir)
                    {
                        case Direction.Up:
                            v.pos = new Vector2D(v.pos.x + (ver.data.height / 4), v.pos.y + ver.data.height);
                            d.pos = v.pos;
                            cwidth = ver.data.width / 2;
                            cheight = next.data.pos.y - v.pos.y;
                            dwidth = cwidth;
                            dheight = 20;
                            break;
                        case Direction.Right:
                            v.pos = new Vector2D(v.pos.x + ver.data.width, v.pos.y + (ver.data.height / 4));
                            d.pos = v.pos;
                            cwidth = next.data.pos.x - v.pos.x;
                            cheight = ver.data.height / 2;
                            dwidth = 20;
                            dheight = cheight;
                            break;
                        case Direction.Down:
                            v.pos = new Vector2D(v.pos.x + (ver.data.width / 4), v.pos.y);
                            d.pos = v.pos;
                            cwidth = ver.data.width / 2;
                            cheight = (next.data.pos.y + next.data.height) - v.pos.y;
                            dwidth = cwidth;
                            dheight = -20;
                            break;
                        case Direction.Left:
                            v.pos = new Vector2D(v.pos.x, v.pos.y + (ver.data.height / 4));
                            d.pos = v.pos;
                            cwidth = (next.data.pos.x + next.data.width) - v.pos.x;
                            cheight = ver.data.height / 2;
                            dwidth = -20;
                            dheight = cheight;
                            break;
                    }

                    newSector(v, cwidth,
                        cheight,
                        200,
                        ver.data.ceil,
                        ver.data.floor);

                    newSector(d, dwidth,
                        dheight,
                        200,
                        ver.data.floor,
                        ver.data.floor);
                }

              
                //Taille du prochain secteur
                //float width = (float)(r.Next() % 10) * 64.0f + 128.0f;
                //float height = (float)(r.Next() % 10) * 64.0f + 128.0f;

                //On checke ou on peut le poser
                Line2D l1 = new Line2D();
                bool[] dirOk = new bool[4];
                /*
                //A droite
                l1.v2 = l1.v1 = pv;
                l1.v1.x += pwidth;
                l1.v2.x += width + 2048;
                bool droite = !checkIntersect(l1);
                l1.v1.y = l1.v2.y = l1.v1.y + height;
                droite = droite && !checkIntersect(l1);
                dirOk[0] = droite;
                Console.WriteLine("Droite ok:" + droite);


                //A gauche
                l1.v2 = l1.v1 = pv;
                l1.v2.x -= width + 2048;
                bool gauche = !checkIntersect(l1);
                l1.v1.y = l1.v2.y = l1.v1.y + height;
                gauche = gauche && !checkIntersect(l1);
                dirOk[1] = gauche;
                Console.WriteLine("Gauche ok:" + gauche);

                //En haut
                l1.v2 = l1.v1 = pv;
                l1.v1.y = l1.v2.y = l1.v1.y + pheight;
                l1.v2.y += height + 2048;
                bool haut = !checkIntersect(l1);
                l1.v1.x = l1.v2.x = l1.v1.x + width;
                haut = haut && !checkIntersect(l1);
                dirOk[2] = haut;
                Console.WriteLine("Haut ok:" + haut);

                //En bas
                l1.v2 = l1.v1 = pv;
                l1.v2.y -= height + 2048;
                bool bas = !checkIntersect(l1);
                l1.v1.x = l1.v2.x = l1.v1.x + width;
                bas = bas && !checkIntersect(l1);
                dirOk[3] = bas;
                Console.WriteLine("Bas ok:" + bas);

                bool oneDirOk = haut || bas || gauche || droite;

                int nextDir = pdir;
                if (!oneDirOk)
                    Console.WriteLine("No dir available, on va croiser !!!");
                else
                {
                    int nbTry = 0;
                    while ((!dirOk[nextDir] || pdir == nextDir) && nbTry++ < 100)
                        nextDir = r.Next() % 4;
                }


                
                switch (nextDir)
                {
                    case 0: //droite
                        Console.WriteLine("On va a droite !");
                        v.pos.x += pwidth;
                        break;
                    case 1: //gauche
                        Console.WriteLine("On va a gauche !");
                        v.pos.x -= width;
                        break;
                    case 2: //haut
                        Console.WriteLine("On va en haut !");
                        v.pos.y += pheight;
                        break;
                    case 3: //bas
                        Console.WriteLine("On va en bas !");
                        v.pos.y -= height;
                        break;

                }





                /*if (r.NextDouble() > 0.5)
                    v.pos.x += pwidth; //(r.NextDouble() > 0.5 ? width : -nwidth);
                else
                    v.pos.y += pheight; //(r.NextDouble() > 0.5 ? height : -nheight);*/

                /*
                else if (i == 1)
                {
                    addThing(new Vector2D(v.pos.x + width / 2, v.pos.y + height / 2), "weapons");
                }
                else if (i % 3 == 0)
                {
                    while (r.NextDouble() > 0.3f)
                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "monsters");
                }
                if (i % 5 == 0)
                {
                    do
                    {

                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "ammunition");
                    } while (r.NextDouble() > 0.3f);
                }
                if (i % 20 == 0)
                {
                    do
                    {
                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "health");
                    } while (r.NextDouble() > 0.5f);
                    addThing(new Vector2D(v.pos.x + width / 2, v.pos.y + height / 2), "weapons", 0.3f);
                }

                while (r.NextDouble() > 0.5f)
                    addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                        v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "decoration", (float)r.NextDouble());


                pwidth = width;
                pheight = height;
                pceil = ceil;
                pfloor = floor;
                pdir = nextDir;

                // Handle thread interruption
                try { Thread.Sleep(0); }
                catch (ThreadInterruptedException) { Console.WriteLine(">>>> thread de generation interrompu at sector " + i); break; }
                */
            }
            
           


            
        }

        //Easy mode
        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            //foreach (ThingTypeInfo ti in General.Map.Data.ThingTypes)
            //Console.WriteLine(ti.Category.Name);
            //showCategories();

            makeOnePath(true, 0);

            correctMissingTex();
        }

        //Medium Mode
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            makeOnePath(true, 1);

            correctMissingTex();
        }

        //Hard mode
        private void btnDoMagic_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Trying to do some magic !!");
            /*
            makeOnePath(true);
            makeOnePath(false);
            makeOnePath(false);

            correctMissingTex();

            Console.WriteLine("Did I ? Magic ?");*/

            makeOnePath(true, 2);

            correctMissingTex();
        }
    }
}
