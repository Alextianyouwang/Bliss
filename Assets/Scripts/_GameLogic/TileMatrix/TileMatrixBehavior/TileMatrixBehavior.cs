using UnityEngine;

public class TileMatrixBehavior : MonoBehaviour
{
    protected GameObject tile;
    protected SaveButton saveButton;
    protected DeleteButton deleteButton;
    protected TM_DrawInstance t;
    [SerializeField] protected int defaultTileDimension = 7;
    [SerializeField] protected float defaultRadius = 15;
    [SerializeField] protected bool isEnabled = true;

    private void Start()
    {
        LoadObject();
        InitializeTileButton();
    }
    private void LoadObject()
    {
        if (SceneSwitcher.sd == null)
        {
            print("SceneSwitcher Not Exist.");
            return;
        }
        tile = SceneSwitcher.sd.tile_prefab;
        saveButton = SceneSwitcher.sd.saveButton_prefab.GetComponent<SaveButton>();
        deleteButton = SceneSwitcher.sd.deleteButton_prefab.GetComponent<DeleteButton>();
    }

    private void InitializeTileButton()
    {
        if (tile == null)
            return;
        t = new TM_DrawInstance(tile, defaultTileDimension);
        t.Initialize();

    }

    private void Update()
    {
        if (t == null)
            return;
        t.UpdateTileSetActive(FirstPersonController.playerGroundPosition,defaultRadius);
        t.UpdateTileOrderedCoordinate(FirstPersonController.playerGroundPosition);
        t.UpdateTilesStatusPerFrame(0.2f, 5.0f, 1.5f, 0, 0.5f, FirstPersonController.playerGroundPosition);
        t.DrawTileInstanceCurrentFrame();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, defaultRadius);
        if (t != null && t.tileOrderedDict != null)
            foreach (TM_DrawInstance.TileData t in t.tileOrderedDict.Values)
            {
                Gizmos.DrawSphere(t.screenPos, 0.1f);
            }
    }
}
