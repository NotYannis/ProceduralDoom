using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeImp.DoomBuilder;
using CodeImp.DoomBuilder.Geometry;

namespace GenerativeDoom
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left,
    }

    /// <summary>
    ///      Represent a graph. A graph is a list of vertices.
    ///      Each vertex contains a list of its neighbours.
    /// </summary>
    public class Graph
    {
        List<Vertex> vertices;
        public int size;
        public int squareEdgeSize;

        static Random _r;


        /// <summary>
        ///     Creates a graph with a list of vertices and a matrix of their edges.
        ///     It will be a squared matrix, with double sided vertices
        ///</summary>
        ///
        /// <param name="numberOfVert">The number of vertices you want to have in your graph. Need to be square number
        ///     since all graphs created are squares.</param>
        public Graph(int numberOfVert)
        {
            _r = new Random();
            size = 0;
            vertices = new List<Vertex>();

            for (int i = 0; i < numberOfVert; ++i)
            {
                AddVertice(i);
            }

            squareEdgeSize = (int)Math.Sqrt((double)size);

            //Adding neighbours
            for (int i = 0; i < numberOfVert - 1; ++i)
            {
                AddEdge(i, i + 1);
                AddEdge(i + 1, i);

                AddEdge(i, i - 1);
                AddEdge(i - 1, i);

                AddEdge(i, i + squareEdgeSize);
                AddEdge(i + squareEdgeSize, i);

                AddEdge(i, i - squareEdgeSize);
                AddEdge(i - squareEdgeSize, i);
            }

        }

        /// <summary>Add an edge to the graph</summary>
        ///
        /// <param name="x">The vertex wich the connection comes from</param>
        /// <param name="y">The vertex wich the connection goes to</param>
        public void AddEdge(int x, int y)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                vertices[x].AddNeighbour(vertices[y], GetDirection(x, y));
            }
        }

        /// <summary>Remove an edge to the graph</summary>
        ///
        /// <param name="x">The vertex wich the connection comes from</param>
        /// <param name="y">The vertex wich the connection goes to</param>
        public void RemoveEdge(int x, int y)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                vertices[x].RemoveNeighbour(GetDirection(x, y));
            }
        }

        public Vertex AddVertice(int index)
        {
            vertices.Add(new Vertex(index));
            size++;
            return vertices[size - 1];
        }

        public Vertex AddVertice(Vertex v)
        {
            vertices.Add(v);
            size++;
            return vertices[size - 1];
        }

        public void RemoveVertice(int index)
        {
            vertices.Remove(vertices[index]);
            size--;
        }

        public void RemoveAllVertex()
        {
            for (int i = size - 1; i > 0; --i)
            {
                if (vertices[i].data.type == "empty")
                {
                    vertices.RemoveAt(i);
                }
            }
        }

        public Vertex GetVertexNeighbour(int vertexIndex, Direction dir)
        {
            return vertices[vertexIndex].GetNeighbour(dir);
        }

        /// <summary>
        /// Give a random neighbour from the list of vertex contained in the vertice
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns>The direction of the neighbour</returns>
        public Direction GetRandomVertexNeighbour(int vertexIndex)
        {
            int randNeighbour = _r.Next(0, vertices[vertexIndex].NumberOfNeighbours);

            return vertices[vertexIndex].GetNeighbour(randNeighbour);
        }

        ///<summary>
        ///Return the index of a vertex on the edge of the graph
        ///</summary>
        public Vertex GetVertexOnEdge()
        {
            Vertex vertex = vertices[_r.Next(0, size - 1)];

            while (vertex.NumberOfNeighbours > 3)
            {
                vertex = vertices[_r.Next(0, vertices.Count)];
            }

            return vertex;
        }

        /// <summary>
        /// Get a vertice on the graph
        /// </summary>
        /// <param name="i">The index of the vertice</param>
        /// <returns>The vertice at the given index</returns>
        public Vertex GetVertex(int index)
        {
            return vertices[index];
        }

        public Direction GetDirection(int x, int y)
        {
            if (x == y + 1)
            {
                return Direction.Right;
            }
            else if (x == y - 1)
            {
                return Direction.Left;
            }
            else if (x == y + squareEdgeSize)
            {
                return Direction.Down;
            }
            else
            {
                return Direction.Up;
            }
        }

        public int GetNumVertices
        {
            get
            {
                return vertices.Count;
            }
        }
    }

    /// <summary>Represent a Vertex on a graph</summary>
    public class Vertex
    {
        public struct VertexData
        {
            public string type;
            public Vector2D pos;
            public float width;
            public float height;
            public int ceil;
            public int floor;
        }

        public int index;
        private Dictionary<Direction, Vertex> neighbours; //List of neighbours
        public VertexData data; //Type of the vertex

        public Vertex(int _index)
        {
            index = _index;
            data.type = "empty";
            SetData(new Vector2D(0.0f, 0.0f), 0.0f, 0.0f, 0, 0);
            neighbours = new Dictionary<Direction, Vertex>();
        }

        /// <summary>
        /// Add a neighbour to the list
        /// </summary>
        /// <param name="neighbour">The vertex to add</param>
        public void AddNeighbour(Vertex neighbour, Direction dir)
        {
            if (!HasNeighbour(dir))
            {
                neighbours.Add(dir, neighbour);
            }
        }

        /// <summary>
        /// Removes a neighbour to the list
        /// </summary>
        /// <param name="neighbour">The vertex to remove</param>
        public void RemoveNeighbour(Direction dir)
        {
            if (HasNeighbour(dir))
            {
                neighbours.Remove(dir);
            }
        }

        /// <summary>
        /// Checks if the vertex has the neighbour set on param
        /// </summary>
        /// <param name="neighbour">The neighbour to check</param>
        /// <returns>True if the has the neighbour</returns>
        public bool HasNeighbour(Direction dir)
        {
            return neighbours.ContainsKey(dir);
        }

        /// <summary>
        /// Returns a neighbour at the index set on param
        /// </summary>
        /// <param name="neighbourIndex">The index of the list to check</param>
        /// <returns>The vertex found in the list of neighbour</returns>
        public Direction GetNeighbour(int neighbourIndex)
        {
            if (neighbourIndex < NumberOfNeighbours)
            {
                return neighbours.ElementAt(neighbourIndex).Key;
            }
            else
            {
                return Direction.Up;
            }
        }

        /// <summary>
        /// Returns a neighbour at the index set on param
        /// </summary>
        /// <param name="neighbourIndex">The index of the list to check</param>
        /// <returns>The vertex found in the list of neighbour</returns>
        public Vertex GetNeighbour(Direction dir)
        {
            Vertex v = new Vertex(-1);
            neighbours.TryGetValue(dir, out v);
            return v;
        }

        public void SetData(Vector2D p, float w, float h, int c, int f)
        {
            data.pos = p;
            data.width = w;
            data.height = h;
            data.ceil = c;
            data.floor = f;
        }

        public int NumberOfNeighbours
        {
            get
            {
                return neighbours.Count;
            }
        }
    }
}
