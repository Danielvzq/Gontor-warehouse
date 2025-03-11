using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] public Coordenadas gridSize;
    [field: SerializeField] public int TileSize { get; private set; }
    [SerializeField] private GameObject tilePrefab;

    private Dictionary<Coordenadas, Etiquetas> gridTiles = new Dictionary<Coordenadas, Etiquetas>();

    private List<Coordenadas> initialPositions = new List<Coordenadas>();
    public List<Coordenadas> puntos_entregas = new List<Coordenadas>();
    private List<Coordenadas> obstacles = new List<Coordenadas>();
    public Coordenadas puntoRecogida = null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadExistingTiles();
    }

    [ContextMenu("Clear Grid")]
    private void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        gridTiles.Clear();
        initialPositions.Clear();
        puntos_entregas.Clear();
        obstacles.Clear();
        puntoRecogida = null;
    }

    [ContextMenu("Spawn Grid")]
    private void LoadExistingTiles()
    {
        Etiquetas[] existingTiles = FindObjectsOfType<Etiquetas>();

        if (existingTiles.Length > 0)
        {
            Debug.Log("Se encontraron tiles existentes, usándolos en lugar de regenerar.");
            gridTiles.Clear();

            foreach (Etiquetas tile in existingTiles)
            {
                Coordenadas coord = tile.coordenadas;
                gridTiles[coord] = tile;
                UpdateTileType(tile);
            }
            return;
        }

        Debug.Log("No se encontraron tiles, generando un nuevo grid.");
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(x * TileSize, 0, y * TileSize);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = $"Tile {x} {y}";

                Coordenadas coord = new Coordenadas(x, y);
                Etiquetas etiquetas = tile.GetComponent<Etiquetas>();
                etiquetas.SetCoordinates(x, y);

                gridTiles[coord] = etiquetas;
                UpdateTileType(etiquetas);
            }
        }
    }

    public void UpdateTileType(Etiquetas tile)
    {
        Coordenadas coord = tile.coordenadas;

        initialPositions.Remove(coord);
        puntos_entregas.Remove(coord);
        obstacles.Remove(coord);
        if (puntoRecogida != null && puntoRecogida.Equals(coord))
        {
            puntoRecogida = null;
        }

        switch (tile.Tipo)
        {
            case TileType.InitialPosition:
                initialPositions.Add(coord);
                break;
            case TileType.TargetPosition:
                puntos_entregas.Add(coord);
                break;
            case TileType.Obstacle:
                obstacles.Add(coord);
                break;
            case TileType.PuntoRecogida:
                puntoRecogida = coord;
                break;
        }
    }

    public Vector3 GetWorldPosition(Coordenadas coordenadas)
    {
        float tileSize = TileSize;
        float worldX = coordenadas.x * tileSize + transform.position.x;
        float worldZ = coordenadas.y * tileSize + transform.position.z;

        return new Vector3(worldX, 0.15f, worldZ);
    }

    public Dictionary<string, object> GetGridData()
    {
        return new Dictionary<string, object>
        {
            { "width", gridSize.x },
            { "length", gridSize.y },
            { "num_agents", initialPositions.Count },
            { "initial_positions", initialPositions.Select(c => new int[] { c.x, c.y }).ToArray() },
            { "obstacles", obstacles.Select(c => new int[] { c.x, c.y }).ToArray() },
            { "puntos_entregas", puntos_entregas.Select(c => new int[] { c.x, c.y }).ToArray() },
            { "punto_recogida", puntoRecogida != null ? new int[] { puntoRecogida.x, puntoRecogida.y } : new int[] { 0, 0 } }, // 🔹 Se envía el punto de recogida
            { "generate_txt", true }
        };
    }
}
