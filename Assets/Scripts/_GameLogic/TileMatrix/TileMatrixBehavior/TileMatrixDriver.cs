using UnityEngine;
using System;
using System.Linq;

public class TileMatrixDriver : MonoBehaviour,INeedResources
{
    private TileMatrixStructureData t;
    private TileMatrixStats s;

    [SerializeField] private bool isEnabled = true;
    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject masterObject;
    [SerializeField] private int defaultTileDimension = 7;
    [SerializeField] private float defaultRadius = 15;
    [SerializeField] private LayerMask tileMatrixLayer;

    public static Action<TileMatrixStructureData> OnShareTileStructureData;
    public static Action<TileMatrixStats> OnShareTileStats;
    public static Action OnCallFunctionTileUpdate;
    public static Func<TileMatrixStats> OnCallStatsUpdate;


    private void Start()
    {
        InitializeStats();
        InitializeTile();
    }
    public void LoadResources()
    {
        if (tile != null)
            return;
        if (SceneDataMaster.sd == null)
        {
            print("SceneDataMaster Doesn't Exist. Using Manually Assigned Tile Object.");
            return;
        }
        LoadTile(SceneDataMaster.sd.tile_prefab);
    }

    public void LoadTile(GameObject _tile) 
    {
        tile = _tile;
    }

    private void InitializeStats() 
    {
        s = new TileMatrixStats(
            masterObject.transform.position,
            defaultRadius,
            0.2f, 5.0f, 1.5f, 0, 0.5f, 0.2f
            ) ;
        OnShareTileStats?.Invoke(s);
    }
    private void InitializeTile()
    {
        if (tile == null) 
        {
            print("Tile Doesn't Exist.");
            return;
        }
        t = new TileMatrixStructureData(tile, defaultTileDimension);
        t.Initialize();
        OnShareTileStructureData?.Invoke(t);
    }

    private void Update()
    {
        if (!masterObject)
            return;
        if (!isEnabled)
            return;
        if (t == null)
            return;
        TileMatrixStats tempStats = OnCallStatsUpdate?.Invoke();
        if (tempStats == null)
            tempStats = s;

        t.UpdateTileSetActive(s.center,s.radius);
        t.UpdateTileOrderedCoordinate(s.center);
        t.UpdateWindowTile(s.center);
        t.UpdateTilesStatusPerFrame(s.innerRadius, s.outerRadius, s.coneShapeMultiplier, s.yOffset, s.noiseWeight,s.dampSpeed, s.center);
        OnCallFunctionTileUpdate?.Invoke();

        DrawTileInstanceCurrentFrame();
    }
    public void DrawTileInstanceCurrentFrame()
    {
        Matrix4x4[] tileMatrix = t. tileOrderedDict.Values.Where(x => !x.occupied).Select(x => x.transformMat).ToArray();
        Graphics.DrawMeshInstanced(
            t.tileMesh,
            0,
            t.tileMat,
            tileMatrix,
            tileMatrix.Length,
            new MaterialPropertyBlock(),
            UnityEngine.Rendering.ShadowCastingMode.On,
            true,
            (int)Mathf.Log(tileMatrixLayer.value, 2));
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, defaultRadius);
        if (t != null && t.tileOrderedDict != null)
            foreach (TileData t in t.tileOrderedDict.Values)
            {
                Gizmos.DrawSphere(t.finalXYZPosition, 0.1f);
            }
    }
}
