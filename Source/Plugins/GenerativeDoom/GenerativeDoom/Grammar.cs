using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeImp.DoomBuilder;
using CodeImp.DoomBuilder.Geometry;


namespace GenerativeDoom
{
    public class Grammar
    {
        Stack<string> Generation;
        public Queue<string> finalMission;

        RandomNormal rnd;

        static Random _r = new Random();
        static Random _r2 = new Random();

        float enemyProb = 0.7f;


        public Grammar(int difficulty)
        {
            Generation = new Stack<string>();
            rnd = new RandomNormal();
            finalMission = new Queue<string>();
            switch (difficulty)
            {
                case 0: enemyProb = 0.4f;
                    break;
                case 1: enemyProb = 0.6f;
                    break;
                case 2: enemyProb = 0.8f;
                    break;
            }
        }

        public Graph Build()
        {
            Graph graph = new Graph(36);
            BuildMission();
            graph = BuildGraph(36);
            return BuildSpace(graph);
        }

        /// <summary>
        /// Build a list of string that represent a simple scenario of a dungeon
        /// </summary>
        public void BuildMission()
        {
            Generation.Push("Dungeon");
            string current;

            while (Generation.Count > 0)
            {
                current = Generation.Pop();

                switch (current)
                {
                    case "Dungeon":
                        Generation.Push("boss");
                        Generation.Push("weapon");
                        Generation.Push("Rooms");
                        Generation.Push("Rooms");
                        Generation.Push("Rooms");
                        Generation.Push("entry");
                        break;
                    case "Rooms":
                    case "Enemy":
                        GetNonTermProb(current);
                        break;
                    case "boss":
                    case "entry":
                    case "lock":
                    case "key":
                    case "room":
                    case "enemy":
                    case "bonus":
                    case "weapon":
                        finalMission.Enqueue(current);
                        Console.Write(current + ", ");
                        break;
                }
            }
            Console.Write("\n");
        }

        /// <summary>
        /// Consume a non terminal word, and build a serie of non-terminals or terminals,
        /// using probabilities.
        /// </summary>
        /// <param name="word">The word consumed</param>
        public void GetNonTermProb(string word)
        {
            double prob = _r.NextDouble();

            switch (word)
            {
                case "Rooms":
                    if(prob < 0.4)
                    {
                        Generation.Push("Enemy");
                        Generation.Push("Rooms");
                    }
                    else
                    {
                        Generation.Push("Enemy");
                    }
                    break;
                case "Enemy":
                    if(prob < enemyProb)
                    {
                        Generation.Push("enemy");
                    }
                    else
                    {
                        prob = _r.NextDouble();
                        if(prob < 0.3){
                            Generation.Push("bonus");
                            Generation.Push("enemy");
                        }
                        else
                        {
                            Generation.Push("enemy");
                            Generation.Push("weapon");
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Build a graph from the dungeon's scenario
        /// </summary>
        /// <returns>Return the graph</returns>
        public Graph BuildGraph(int size)
        {
            Graph graph = new Graph(size);
            Vertex currentVertex = new Vertex(-1);

            List<Vertex> populatedVertex;
            populatedVertex = new List<Vertex>();

            while (finalMission.Count > 0)
            {
                string current = finalMission.Dequeue();
                switch (current)
                {
                    case "entry":
                        currentVertex = graph.GetVertexOnEdge();
                        currentVertex.SetData(new Vector2D(0, 0), 512.0f, 512.0f, 128, 0);
                        break;
                    case "lock":
                    case "key":
                    case "room":
                    case "imp":
                    case "zombie":
                    case "enemy":
                    case "bonus":
                    case "boss":
                    case "weapon":
                        //Search for an empty vertex
                        Direction nextDir = graph.GetRandomVertexNeighbour(currentVertex.index);
                        Vertex nextVer = graph.GetVertexNeighbour(currentVertex.index, nextDir);
                        while (graph.GetVertex(nextVer.index).data.type != "empty")
                        {
                            nextDir = graph.GetRandomVertexNeighbour(currentVertex.index);
                            nextVer = graph.GetVertexNeighbour(currentVertex.index, nextDir);
                        }
                        
                        //Remove every neighbours of the current vertex, except the new one
                        for (int i = graph.GetVertex(currentVertex.index).NumberOfNeighbours - 1; i >= 0; --i)
                        {
                            Direction dir = graph.GetVertex(currentVertex.index).GetNeighbour(i);
                            if (dir != nextDir)
                            {
                                graph.GetVertex(currentVertex.index).RemoveNeighbour(dir);
                            }
                        }
                        currentVertex = nextVer;
                        break;
                }
                graph.GetVertex(currentVertex.index).data.type = current;
                populatedVertex.Add(currentVertex);
            }

            int graphSize = graph.size;

            return graph;
        }

        public Graph BuildSpace(Graph g)
        {
            Vertex current = new Vertex(-1);
            Graph final = new Graph(0);

            Vector2D currentPos = new Vector2D(0.0f, 0.0f);
            float width = 512.0f;
            float height = 512.0f;
            int ceil = 128;
            int floor = 0;

            int widthVariation;
            int heightVariation;

            for (int i = 0; i < g.GetNumVertices; ++i)
            {
                if(g.GetVertex(i).data.type == "entry")
                {
                    current = g.GetVertex(i);
                    break;
                }
            }

            final.AddVertice(current);

            while (current.data.type != "boss")
            {
                //for(int i = 0; i < current.NumberOfNeighbours; ++i)
                //{
                Vertex next = new Vertex(-1);
                widthVariation = (int)new RandomNormal().Generate(0, 50);
                heightVariation = (int)new RandomNormal().Generate(0, 50);

                switch (current.GetNeighbour(0))
                    {
                        case Direction.Up:
                            currentPos.y += heightVariation + height * 2;
                            next = current.GetNeighbour(Direction.Up);
                            break;
                        case Direction.Right:
                            currentPos.x += widthVariation + width * 2;
                            next = current.GetNeighbour(Direction.Right);
                            break;
                        case Direction.Down:
                            currentPos.y -= heightVariation + height * 2;
                            next = current.GetNeighbour(Direction.Down);
                            break;
                        case Direction.Left:
                            currentPos.x -= widthVariation + width * 2;
                            next = current.GetNeighbour(Direction.Left);
                            break;
                    }

                    current = next;
                    current.SetData(currentPos, width + widthVariation, height + heightVariation, ceil, floor);
               // }
                final.AddVertice(current);
            }

            //final.AddVertice(current);

            return final;
        }
    }
}
