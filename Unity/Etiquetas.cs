using UnityEngine;
using TMPro;

public class Etiquetas : MonoBehaviour 
{
    [SerializeField]
    private TextMeshPro label;

    public Coordenadas coordenadas;
    
    [SerializeField]  
    private TileType tileType = TileType.Normal;

    private Renderer tileRenderer;

    public TileType Tipo
    {
        get { return tileType; }
        set { SetTileType(value); }
    }

    private void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        SetTileType(tileType);  
        UpdateCordsLabel();
    }

    public void SetCoordinates(int x, int y)
    {
        coordenadas = new Coordenadas(x, y);
        UpdateCordsLabel();
    }

    public void SetTileType(TileType type)
    {
        tileType = type;
        UpdateCordsLabel();
        UpdateTileAppearance();
        UpdateGameObjectTag();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileType(this);
        }
    }

    private void UpdateCordsLabel()
    {
        label.text = $"{coordenadas.x}, {coordenadas.y}\n{tileType}";
    }

    private void UpdateTileAppearance()
    {
        if (tileRenderer == null) return;

        switch (tileType)
        {
            case TileType.Obstacle:
                tileRenderer.material.color = Color.black;
                break;
            case TileType.InitialPosition:
                tileRenderer.material.color = Color.green;
                break;
            case TileType.TargetPosition:
                tileRenderer.material.color = Color.red;
                break;
            case TileType.PuntosRecogida:
                tileRenderer.material.color = Color.yellow;
                break;
            case TileType.Normal:
                tileRenderer.material.color = Color.white;
                break;
        }
    }

    private void UpdateGameObjectTag()
    {
        switch (tileType)
        {
            case TileType.Obstacle:
                gameObject.tag = "Obstacle";
                break;
            case TileType.InitialPosition:
                gameObject.tag = "InitialPosition";
                break;
            case TileType.TargetPosition:
                gameObject.tag = "TargetPosition";
                break;
            case TileType.PuntosRecogida:  // 🔹 Ahora es puntosRecogida
                gameObject.tag = "PuntosRecogida";
                break;
            case TileType.Normal:
                gameObject.tag = "Untagged";
                break;
        }
    }
}

// 🔹 Enum actualizado con puntosRecogida
public enum TileType
{
    Normal,
    Obstacle,
    InitialPosition,
    TargetPosition,
    PuntosRecogida  // 🔹 Ahora el nombre está en español
}
