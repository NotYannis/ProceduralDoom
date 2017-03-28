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

        static Random rand = new Random();

        int[] easyEnemies = { 3004, 9, 3001, 3006,  84 };
        int[] mediumEnemies = { 3002, 58, 3005, 65 };
        int[] hardEnemies = { 3003, 69, 71, 64, 66 };
        int[] bosses = { 16, 7, 68 };


        int[] weapons = { 2002, 2004, 2003, 2001, 82 };

        //A dictionnary with a weapon's ID on key and its ammo on value
        Dictionary<int, int> ammo = new Dictionary<int, int>()
        {
            {2002, 2007},
            {2004, 2047},
            {2003, 2010},
            {2001, 2008},
            {82, 2008}
        };

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

        private Thing addThing(Vector2D pos, String category, float proba = 0.5f, int index = 0)
        {
            Thing t = addThing(pos);
            if (t != null)
            {
                IList<ThingCategory> cats = General.Map.Data.ThingCategories;

                bool found = false;
                if (index != 0)
                {
                    t.Type = index;
                    t.SetFlag("8", true);
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
                            if (rand.NextDouble() > proba)
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
            gram = new Grammar(difficulty);
            graph = gram.Build();
            
            Random r = new Random();

            DrawnVertex v = new DrawnVertex();
            float width;
            float height;

            v.pos.y = 0;

            for (int i = 0; i < graph.size; i++)
            {
                Console.WriteLine("---------------------- Sector " + i);

                Vertex room = graph.GetVertex(i);
                v.pos = room.data.pos;

                width = room.data.width;
                height = room.data.height;

                newSector(v, room.data.width,
                    room.data.height,
                    200,
                    room.data.ceil,
                    room.data.floor);

                #region
                RandomNormal rz = new RandomNormal();

                if (room.data.type == "entry" && playerStart)
                {
                    Thing t = addThing(new Vector2D(v.pos.x + width / 2, v.pos.y + height / 2));
                    t.Type = TYPE_PLAYER_START;
                    t.Rotate(0);
                }
                else if (room.data.type == "enemy")
                {
                    int numberOfEnemy = (int)rz.Generate(3, 2);

                    //Each room of enemies is composed of 2 groups : an easy one a an other one dependant of the difficulty
                    int enemyIndex = easyEnemies[rand.Next(0, easyEnemies.Length - 1)];
                    int enemyIndex2 = 0;

                    switch (difficulty)
                    {
                        case 0:
                            enemyIndex2 = easyEnemies[rand.Next(0, easyEnemies.Length - 1)];
                            break;
                        case 1:
                            enemyIndex2 = mediumEnemies[rand.Next(0, easyEnemies.Length - 1)];
                            break;
                        case 2:
                            enemyIndex2 = hardEnemies[rand.Next(0, easyEnemies.Length - 1)];
                            break;
                    }

                    for (int j = 0; j <= numberOfEnemy; ++j)
                    {
                        addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                            v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "monsters", 0.5f, enemyIndex);
                    }

                    if (enemyIndex2 != 0)
                    {
                        numberOfEnemy = (int)rz.Generate(2, 1);
                        for (int j = 0; j <= numberOfEnemy; ++j)
                        {
                            addThing(new Vector2D(v.pos.x + width / 2 + ((float)r.NextDouble() * (width / 2)) - width / 4,
                                v.pos.y + height / 2 + ((float)r.NextDouble() * (height / 2)) - height / 4), "monsters", 0.5f, enemyIndex2);
                        }
                    }
                }
                else if (room.data.type == "bonus")
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
                else if(room.data.type == "weapon")
                {
                    int weaponIndex = weapons[rand.Next(0, weapons.Length - 1)];
                    int ammoIndex;

                    addThing(new Vector2D(v.pos.x + width / 2, v.pos.y + height / 2), "weapons", 0.3f, weaponIndex);

                    bool dede = ammo.TryGetValue(weaponIndex, out ammoIndex);

                    for (int j = 0; j < 6; ++j)
                    {
                        addThing(new Vector2D(v.pos.x + width / 2 + (float)rz.Generate(0, 30),
                                                v.pos.y + height / 2 + (float)rz.Generate(0, 30)), "weapons", 0.3f, ammoIndex);
                    }
                }
                else if (room.data.type == "boss")
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

                //Building corridors
                if(room.data.type != "boss")
                {
                    Direction nextDir = room.GetNeighbour(0);
                    Vertex next = room.GetNeighbour(nextDir);
                    switch (nextDir)
                    {
                        case Direction.Up:
                            v.pos = new Vector2D(v.pos.x + (room.data.height / 4), v.pos.y + room.data.height);
                            d.pos = v.pos;
                            cwidth = room.data.width / 2;
                            cheight = next.data.pos.y - v.pos.y;
                            dwidth = cwidth;
                            dheight = 20;
                            break;
                        case Direction.Right:
                            v.pos = new Vector2D(v.pos.x + room.data.width, v.pos.y + (room.data.height / 4));
                            d.pos = v.pos;
                            cwidth = next.data.pos.x - v.pos.x;
                            cheight = room.data.height / 2;
                            dwidth = 20;
                            dheight = cheight;
                            break;
                        case Direction.Down:
                            v.pos = new Vector2D(v.pos.x + (room.data.width / 4), v.pos.y);
                            d.pos = v.pos;
                            cwidth = room.data.width / 2;
                            cheight = (next.data.pos.y + next.data.height) - v.pos.y;
                            dwidth = cwidth;
                            dheight = -20;
                            break;
                        case Direction.Left:
                            v.pos = new Vector2D(v.pos.x, v.pos.y + (room.data.height / 4));
                            d.pos = v.pos;
                            cwidth = (next.data.pos.x + next.data.width) - v.pos.x;
                            cheight = room.data.height / 2;
                            dwidth = -20;
                            dheight = cheight;
                            break;
                    }

                    newSector(v, cwidth,
                        cheight,
                        200,
                        room.data.ceil,
                        room.data.floor);

                    newSector(d, dwidth,
                        dheight,
                        200,
                        room.data.floor,
                        room.data.floor);
                }
            }
            
           


            
        }

        //Easy mode
        private void btnAnalysis_Click(object sender, EventArgs e)
        {
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
            makeOnePath(true, 2);

            correctMissingTex();
        }
    }
}
