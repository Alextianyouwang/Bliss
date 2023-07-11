using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TileMatrixStructureData
{
    public GameObject tile;
    public Mesh tileMesh { get; private set; }
    public Material tileMat { get; private set; }
    private Vector3 originalTileBound, formationOffset, startPosition;
    private TileData[,] tilePool;
    private Queue<TileData> tileQueue = new Queue<TileData>();
    public Dictionary<Vector2, TileData> tileDict = new Dictionary<Vector2, TileData>();
    public Dictionary<Vector2, TileData> tileOrderedDict = new Dictionary<Vector2, TileData>();
    public TileData[] windowTiles { get; private set; } = new TileData[4];


    private int dimension;
    public float varyingNoiseTime;
    public class TileBehaviorParameters
    {
        public Vector3 center;
        public float radius;
        public float curveMultiplier;
        public float dampingAmount;
        public float noiseWeight;
    }
    public TileMatrixStructureData(GameObject _tile, int _maximumTile)
    {
        tile = _tile;
        dimension = _maximumTile;
    }

    public void Initialize()
    {
        startPosition = FirstPersonController.playerGroundPosition;
        tileMat = tile.GetComponent<MeshRenderer>().sharedMaterial;
        tileMesh = tile.GetComponent<MeshFilter>().sharedMesh;
        originalTileBound = tile.GetComponent<MeshRenderer>().bounds.size;
        float formationSideLength = dimension * originalTileBound.x;
        formationOffset = new Vector3(formationSideLength / 2 - originalTileBound.x / 2, 0, formationSideLength / 2 - originalTileBound.z / 2);

        tilePool = new TileData[dimension, dimension];
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                tilePool[i, j] = new TileData(originalTileBound);

                tileQueue.Enqueue(tilePool[i, j]);
            }
        }

    }

    public void UpdateTileSetActive(Vector3 groundPos, float radius)
    {
        for (int i = 0; i < tileDict.Values.Count; i++)
        {
            var t = tileDict.ElementAt(i);
            if (Vector3.Distance(t.Value.initialXZPosition, groundPos) >= radius)
            {
                tileDict.Remove(t.Key);
                t.Value.ToggleIsInDisplay(false);
                tileQueue.Enqueue(t.Value);

            }
        }
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                Vector3 tileVirtualPosition = groundPos + new Vector3(i * originalTileBound.x, 0, j * originalTileBound.z) - formationOffset;
                Vector2 tileCoord = new Vector2(Mathf.Floor((tileVirtualPosition.x - startPosition.x) / originalTileBound.x), Mathf.Floor((tileVirtualPosition.z - startPosition.z) / originalTileBound.z));
                Vector3 finalPosition = new Vector3(tileCoord.x * originalTileBound.x, 0, tileCoord.y * originalTileBound.z) + new Vector3(startPosition.x, groundPos.y, startPosition.z);
                if (!tileDict.ContainsKey(tileCoord) && Vector3.Distance(finalPosition, groundPos) < radius)
                {
                    TileData localTile = tileQueue.Dequeue();
                    localTile.SetTilePositionAndGlobalCoordinate(finalPosition, tileCoord);
                    localTile.OverwriteTileSmoothDampPos(localTile.GetGroundPosition());
                    localTile.ToggleIsInDisplay(true);
                    tileDict.Add(tileCoord, localTile);
                }

            }
        }
    }
    public void UpdateTileOrderedCoordinate(Vector3 centerPosition)
    {
        tileOrderedDict.Clear();
        for (int i = 0; i < tileDict.Values.Count; i++)
        {
            TileData localTile = tileDict.ElementAt(i).Value;
            if (!localTile.isInDisplay)
                continue;
            Vector2 localTileCoord = new Vector2(
             (int)Mathf.Floor((localTile.initialXZPosition.x - centerPosition.x + originalTileBound.x / 2) / originalTileBound.x),
             (int)Mathf.Floor((localTile.initialXZPosition.z - centerPosition.z + originalTileBound.z / 2) / originalTileBound.z));
            localTile.localTileCoord = localTileCoord;

            if (!tileOrderedDict.ContainsKey(localTileCoord))
                tileOrderedDict.Add(localTileCoord, localTile);

        }
    }

    public void UpdateWindowTile(Vector3 comparePos)
    {
        if (tileOrderedDict.Count == 0)
            return;
        TileData center, up, bot, left, right, upRight, upLeft, botRight, botLeft;
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

        if (comparePos.x > center.initialXZPosition.x && comparePos.z > center.initialXZPosition.z)
        {
            windowTiles[1] = up;
            windowTiles[2] = right;
            windowTiles[3] = upRight;
        }
        else if (comparePos.x > center.initialXZPosition.x && comparePos.z < center.initialXZPosition.z)
        {
            windowTiles[1] = bot;
            windowTiles[2] = right;
            windowTiles[3] = botRight;
        }
        else if (comparePos.x < center.initialXZPosition.x && comparePos.z > center.initialXZPosition.z)
        {
            windowTiles[1] = left;
            windowTiles[2] = up;
            windowTiles[3] = upLeft;
        }
        else if (comparePos.x < center.initialXZPosition.x && comparePos.z < center.initialXZPosition.z)
        {
            windowTiles[1] = bot;
            windowTiles[2] = left;
            windowTiles[3] = botLeft;
        }
    }



    public void UpdateTilesStatusPerFrame(float innerRadius, float outerRadius, float multiplier, float yPos, float noiseWeight, Vector3 centerPosition)
    {
        for (int i = 0; i < tileOrderedDict.Count; i++)
        {
            TileData localTile = tileOrderedDict.ElementAt(i).Value;
            localTile.ToggleOccupied(false);

            float distanceToCenter = Vector2.Distance(new Vector2(localTile.initialXZPosition.x, localTile.initialXZPosition.z), new Vector2(centerPosition.x, centerPosition.z));
            float highRiseInfluence = Mathf.InverseLerp(innerRadius, outerRadius, distanceToCenter);
            float noise = Mathf.PerlinNoise(localTile.initialXZPosition.x / 10 + varyingNoiseTime, localTile.initialXZPosition.z / 10 + varyingNoiseTime);
            Vector3 groundPosition = localTile.GetGroundPosition();
            localTile.SetgroundPosition(groundPosition);
            Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y + highRiseInfluence * multiplier + noise * noiseWeight, groundPosition.z) + Vector3.up * yPos;

            localTile.SetTileFinalPosition(newPos);
            localTile.UpdateTileSmoothDampPos();
            localTile.GetTransformMatFromPos(localTile.OffsetTileAlignWithGround(localTile.smoothedFinalXYZPosition, 0.5f), Vector3.one * originalTileBound.x * 0.01f);

        }
    }


   

}
