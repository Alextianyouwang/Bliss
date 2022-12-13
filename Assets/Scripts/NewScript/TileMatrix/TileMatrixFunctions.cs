using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class TileMatrixFunctions
{
    // Data Storages
    private GameObject tile,saveButton,deleteButton;
    private Queue<TileBase> tilePool = new Queue<TileBase>();
    private TileBase[,] tileMatrixRefPool;
    private Dictionary<Vector2, TileBase> tileDict = new Dictionary<Vector2, TileBase>();
    public Dictionary<Vector2, TileBase> tileOrderedDict = new Dictionary<Vector2, TileBase>();
    public TileBase[] windowTiles = new TileBase[4];

    // Global References
    private Transform player;
    private Vector3 originalTileBound;
    private Vector3 formationOffset, startPosition;
    private float formationSideLength;
    private int defaultTileDimension;

    // Public Parameters
    public bool activateWindowsIndependance = false;
    public float changingWindowsYPos;

    public TileMatrixFunctions(Transform _player,GameObject _tile,GameObject _saveButton,GameObject _deleteButton,int _maximumTile) 
    {
        player = _player;
        tile = _tile;
        saveButton = _saveButton;
        deleteButton = _deleteButton;
        defaultTileDimension = _maximumTile;
    }

    public void Initialize()
    {
        tileMatrixRefPool = new TileBase[defaultTileDimension, defaultTileDimension];
        startPosition = player.position;
        originalTileBound = tile.GetComponent<Renderer>().bounds.size;
        formationSideLength = defaultTileDimension * originalTileBound.x;
        formationOffset = new Vector3(formationSideLength / 2 - originalTileBound.x / 2, 0, formationSideLength / 2 - originalTileBound.z / 2);
        for (int i = 0; i < defaultTileDimension; i++)
        {
            for (int j = 0; j < defaultTileDimension; j++)
            {
                tileMatrixRefPool[i, j] = new TileBase();
                tileMatrixRefPool[i, j].InstantiateTile(Object.Instantiate(tile), Object.Instantiate(saveButton),Object.Instantiate(deleteButton), Vector3.zero, null);
                tileMatrixRefPool[i, j].SetDisplay(TileBase.DisplayState.tile);
                tileMatrixRefPool[i, j].TileSetActive(false);
                tilePool.Enqueue(tileMatrixRefPool[i, j]);
            }
        }
    }
    public TileBase GetNextTile(Vector3 position)
    {
        TileBase newTile = tilePool.Dequeue();
        newTile.SetTilePosition(position, true);
        newTile.TileSetActive(true);
        return newTile;
    }
    public void RecycleTile(TileBase toRecycle)
    {
        toRecycle.TileSetActive(false);
        toRecycle.formationFinalPosition = Vector3.zero;
        toRecycle.localTileCoord = Vector2.zero;
        tilePool.Enqueue(toRecycle);
    }
    public void UpdateTileSetActive(Vector3 groundPos, float radius)
    {
        for (int i = 0; i < tileDict.Values.Count; i++)
        {
            var t = tileDict.ElementAt(i);
            if (Vector3.Distance(t.Value.formationFinalPosition, groundPos) >= radius)
            {
                tileDict.Remove(t.Key);
                RecycleTile(t.Value);
            }
        }
        for (int i = 0; i < defaultTileDimension; i++)
        {
            for (int j = 0; j < defaultTileDimension; j++)
            {
                Vector3 tileVirtualPosition = groundPos + new Vector3(i * originalTileBound.x, 0, j * originalTileBound.z) - formationOffset;
                Vector2 tileCoord = new Vector2(Mathf.Floor((tileVirtualPosition.x - startPosition.x) / originalTileBound.x), Mathf.Floor((tileVirtualPosition.z - startPosition.z) / originalTileBound.z));
                Vector3 finalPosition = new Vector3(tileCoord.x * originalTileBound.x, 0, tileCoord.y * originalTileBound.z) + new Vector3(startPosition.x, groundPos.y, startPosition.z);
                if (!tileDict.ContainsKey(tileCoord) && Vector3.Distance(finalPosition, groundPos) < radius && tilePool.Count > 0)
                {
                    TileBase localTile = GetNextTile(finalPosition);
                    tileDict.Add(tileCoord, localTile);
                    localTile.StickTileToGround();
                }
            }
        }
    }
    public void UpdateTileOrderedCoordinate(Vector3 centerPosition)
    {
        tileOrderedDict.Clear();
        for (int i = 0; i < defaultTileDimension; i++)
        {
            for (int j = 0; j < defaultTileDimension; j++)
            {
                TileBase localTile = tileMatrixRefPool[i, j];
                if (localTile.display_instance.activeSelf)
                {
                    Vector2 localTileCoord = new Vector2(
                 (int)Mathf.Floor((localTile.formationFinalPosition.x - centerPosition.x + originalTileBound.x / 2) / originalTileBound.x),
                 (int)Mathf.Floor((localTile.formationFinalPosition.z - centerPosition.z + originalTileBound.z / 2) / originalTileBound.z));
                    localTile.localTileCoord = localTileCoord;
                    localTile.SetDebugText("( " + localTile.localTileCoord.x.ToString() + "," + localTile.localTileCoord.y.ToString() + " )");

                    if (!tileOrderedDict.ContainsKey(localTileCoord))
                        tileOrderedDict.Add(localTileCoord, localTile);
                }
            }
        }
    }
    public void ResetWindowsTilePrefab()
    {
        for (int i = 0; i< windowTiles.Length;i++) 
        {
            if (windowTiles[i] != null)
                windowTiles[i].SetDisplay(TileBase.DisplayState.tile);
        }
    }

    public void UpdateTileDampSpeedTogether(float dampSpeed)
    {
        for(int i = 0; i< tileOrderedDict.Count; i++) 
        {
            tileOrderedDict.ElementAt(i).Value.dampSpeed = dampSpeed;
        }
    }

    public void UpdateWindowTile(Vector3 comparePos)
    {
        if (tileOrderedDict.Count == 0)
            return;
        TileBase center, up, bot, left, right, upRight, upLeft, botRight, botLeft;
        tileOrderedDict.TryGetValue(Vector2.zero, out center);
        tileOrderedDict.TryGetValue(new Vector2(0, 1), out up);
        tileOrderedDict.TryGetValue(new Vector2(0, -1), out bot);
        tileOrderedDict.TryGetValue(new Vector2(-1, 0), out left);
        tileOrderedDict.TryGetValue(new Vector2(1, 0), out right);
        tileOrderedDict.TryGetValue(new Vector2(1, 1), out upRight);
        tileOrderedDict.TryGetValue(new Vector2(-1, 1), out upLeft);
        tileOrderedDict.TryGetValue(new Vector2(1, -1), out botRight);
        tileOrderedDict.TryGetValue(new Vector2(-1, -1), out botLeft);
        windowTiles[0] = center;

        if (center == null)
            return;

        if (comparePos.x > center.formationFinalPosition.x && comparePos.z > center.formationFinalPosition.z)
        {
            windowTiles[1] = up;
            windowTiles[2] = right;
            windowTiles[3] = upRight;
        }
        else if (comparePos.x > center.formationFinalPosition.x && comparePos.z < center.formationFinalPosition.z)
        {
            windowTiles[1] = bot;
            windowTiles[2] = right;
            windowTiles[3] = botRight;
        }
        else if (comparePos.x < center.formationFinalPosition.x && comparePos.z > center.formationFinalPosition.z)
        {
            windowTiles[1] = left;
            windowTiles[2] = up;
            windowTiles[3] = upLeft;
        }
        else if (comparePos.x < center.formationFinalPosition.x && comparePos.z < center.formationFinalPosition.z)
        {
            windowTiles[1] = bot;
            windowTiles[2] = left;
            windowTiles[3] = botLeft;
        }
    }

  
    public void UpdateTilesStatusPerFrame(float innerRadius, float outerRadius, float multiplier, float yPos, float noiseWeight, Vector3 centerPosition)
    {
        for (int i = 0; i< tileOrderedDict.Count;i++)
        {
            TileBase localTile = tileOrderedDict.ElementAt(i).Value;
            localTile.isWindows = windowTiles.Contains(localTile);

            float distanceToCenter = Vector2.Distance(new Vector2(localTile.formationFinalPosition.x, localTile.formationFinalPosition.z), new Vector2(centerPosition.x, centerPosition.z));
            float highRiseInfluence = Mathf.InverseLerp(innerRadius, outerRadius, distanceToCenter);
            float noise = Mathf.PerlinNoise(localTile.formationFinalPosition.x / 10 + Time.time / 3, localTile.formationFinalPosition.z / 10 + Time.time / 3);
            Vector3 groundPosition = localTile.GetGroundPosition();
            Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y + highRiseInfluence * multiplier + noise * noiseWeight, groundPosition.z) + Vector3.up *yPos;

            if (activateWindowsIndependance && localTile.isWindows)
            {
                newPos = new Vector3(groundPosition.x, changingWindowsYPos, groundPosition.z);
            }
            localTile.targetPosition = newPos;
            localTile.SetTileSmoothDampPos(newPos);
        }
    }

    public void UpdateTielDampSpeedForLanding()
    {
        for (int i = 0; i < tileOrderedDict.Count; i++)
        {
            TileBase localTile = tileOrderedDict.ElementAt(i).Value;
            float distanceToCenter = Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(localTile.formationFinalPosition.x, localTile.formationFinalPosition.z));
            float dampMask = Mathf.InverseLerp(0, 7, distanceToCenter);
            localTile.dampSpeed = Mathf.Lerp(0.5f, 0.15f, dampMask);
        }
    }

    public void TeleportMatrixAlongY(float yOffset)
    {
        for (int i = 0; i < tileOrderedDict.Count; i++)
        {
            TileBase localTile = tileOrderedDict.ElementAt(i).Value;
            Vector3 currentFormation = localTile.formationFinalPosition;
            localTile.SetTilePosition(new Vector3(currentFormation.x, yOffset, currentFormation.z), false);
        }
    }

   
    public void UpdateWindowsTilePrefab(bool isInClippy)
    {
        for (int i = 0; i < windowTiles.Length; i++)
        {
            TileBase t = windowTiles[i];
            if (t != null)
            {
                t.SetDebugText("Window");
                if (isInClippy)
                {
                    t.SetDisplay(TileBase.DisplayState.delete);
                }
                else 
                {
                    t.SetDisplay(TileBase.DisplayState.save);

                }
            }
        }
    }

 
}
