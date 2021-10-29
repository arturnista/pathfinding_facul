using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile
{

    public enum Status
    {
        None,
        Current,
        Active,
        Closed,
        Path
    }

	public Vector2Int Position { get; set; }
	public Vector3 WorldPosition => MazeGenerator.Instance.GetWorldPosition(Position);
	public bool IsWalkable { get; set; }
	public int G { get; set; }
	public int H { get; private set; }
	public int F => G + H;
	public Tile Parent { get; set; }
    private Status m_CurrentStatus;
    public Status CurrentStatus
    {
        get => m_CurrentStatus;
        set
        {
            ObjectTile.SetStatus(value);
            m_CurrentStatus = value;
        }
    }
    public MazeTile ObjectTile;

    public Tile(Vector2Int pos, bool isWalkable)
    {
        IsWalkable = isWalkable;
        Position = pos;
    }

	public void SetDistance(Vector2 target)
	{
		this.H = Mathf.RoundToInt(Mathf.Abs(target.x - Position.x) + Mathf.Abs(target.y - Position.y));
	}
}

public class MazeGenerator : MonoBehaviour
{

    public static MazeGenerator Instance { get; private set; }

    [Range(0f, 10f)]
    public float stepTime = 1f;
    public List<GameObject> tileset = new List<GameObject>();
    public MazeTile TilePrefab;

    [Header("Effects")]
    [SerializeField] private GameObject PathEffect;

    private List<Tile> m_ActiveTiles;
    private List<Tile> m_ClosedTiles;
    private List<Tile> m_CurrentNeighbors;
    private Tile m_Current;

    private int m_MazeDepth = 19;
    private int m_MazeWidth = 22;

    private int[,] m_Maze = new int[19, 22] {
{ 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
{ 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 },
{ 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1 },
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1 },
{ 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1 },
{ 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 1 },
{ 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
{ 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1 },
{ 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1 },
{ 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1 },
{ 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1 },
{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 }
};

    private float m_TileWidth = 3.0f;
    private float m_TileDepth = 3.0f;

    private float m_XI = -25.0f;
    private float m_ZI = 25.0f;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetWorldPosition(Vector2Int pos)
    {
        return new Vector3(
            m_XI + pos.y * m_TileWidth,
            0f,
            m_ZI - pos.x * m_TileDepth
        );
    }

    private void Start()
    {
        for (int i=0; i<m_MazeDepth; i++) //z
        {
            for (int j=0; j<m_MazeWidth; j++) //x
            {
                GameObject tilePrefab = tileset[m_Maze[i,j]];
                // Vector3 p = tilePrefab.transform.position;
                // p.x = xi + j * tileWidth;
                // p.z = zi - i * tileDepth;
                Vector3 p = GetWorldPosition(new Vector2Int(i, j));
                GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity, transform) as GameObject;
            }
        }
    }

    public IEnumerator FindPath(Vector2Int startPosition, Vector2Int finalPosition, Action<List<Vector3>> onFindPath)
    {
        Dictionary<Vector2Int, Tile> allTiles = new Dictionary<Vector2Int, Tile>();
        for (int x = 0; x < m_MazeDepth; x++) //z
        {
            for (int y = 0; y < m_MazeWidth; y++) //x
            {
                Vector2Int pos = new Vector2Int(x, y);
                var tile = new Tile(pos, m_Maze[x, y] == 0);
                allTiles.Add(pos, tile);
                if (tile.IsWalkable)
                {
                    var created = Instantiate(TilePrefab, tile.WorldPosition, Quaternion.identity, transform);
                    created.SetStatus(Tile.Status.None);
                    tile.ObjectTile = created;
                }
            }
        }

        var start = allTiles[startPosition];
        var finish = allTiles[finalPosition];
        start.SetDistance(finalPosition);

        m_ActiveTiles = new List<Tile>();
        m_ClosedTiles = new List<Tile>();

        m_ActiveTiles.Add(start);
        while (m_ActiveTiles.Count > 0)
        {
            m_Current = m_ActiveTiles.Where((x) => x.F == m_ActiveTiles.Min(y => y.F)).First();
            m_Current.CurrentStatus = Tile.Status.Current;

            if (m_Current == finish)
            {
                yield return CalcPathCoroutine(start, finish, onFindPath);
                break;
            }

            m_ActiveTiles.Remove(m_Current);

            m_CurrentNeighbors = GetNeighbors(m_Current, allTiles);
            foreach (var neighbor in m_CurrentNeighbors)
            {
                if (neighbor.Parent == null)
                {
                    neighbor.Parent = m_Current;
                }
                neighbor.G = m_Current.G + 1;
                neighbor.SetDistance(finalPosition);

                if (!m_ClosedTiles.Contains(neighbor))
                {
                    m_ActiveTiles.Add(neighbor);
                    neighbor.CurrentStatus = Tile.Status.Active;
                }
            }

            m_ClosedTiles.Add(m_Current);
            yield return new WaitForSeconds(stepTime);
            m_Current.CurrentStatus = Tile.Status.Closed;
        }
    }

    private IEnumerator CalcPathCoroutine(Tile start, Tile finish, Action<List<Vector3>> onFindPath)
    {
        List<Vector3> path = new List<Vector3>();
        var thisTile = finish;
        while (thisTile.Parent != null && thisTile != start)
        {
            path.Add(thisTile.WorldPosition);
            thisTile.CurrentStatus = Tile.Status.Path;
            thisTile = thisTile.Parent;

            Instantiate(PathEffect, thisTile.WorldPosition, Quaternion.identity);
            
            yield return new WaitForSeconds(stepTime);
        }
        path.Reverse();
        onFindPath?.Invoke(path);
    }

    private List<Tile> GetNeighbors(Tile current, Dictionary<Vector2Int, Tile> tiles)
    {
        List<Tile> neighbors = new List<Tile>();

        var curPos = new Vector2Int(current.Position.x, current.Position.y);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0) continue;
                if (x == 0 && y == 0) continue;

                var key = new Vector2Int(curPos.x + x, curPos.y + y);
                if (tiles.ContainsKey(key) && tiles[key].IsWalkable)
                {
                    neighbors.Add(tiles[key]);
                }
            }   
        }

        return neighbors;
    }
    
}
