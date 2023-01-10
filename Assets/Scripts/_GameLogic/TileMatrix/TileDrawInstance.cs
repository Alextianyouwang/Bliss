using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TileDrawInstance
{
    public GameObject tile;

    private Mesh tileMesh;
    private Material tileMat;
    private Vector3 originalTileBound, formationOffset, startPosition;
    public Dictionary<Vector2, TileData> tileDict = new Dictionary<Vector2, TileData>();
    public Dictionary<Vector2, TileData> tileOrderedDict = new Dictionary<Vector2, TileData>();
    private Queue<TileData> tileQueue = new Queue<TileData>();
    private TileData[,] tilePool;
    public TileData[] windowTiles = new TileData[4];

    private int dimension;
    public bool activateWindowsIndependance = false;
    public bool displayAndUpdateButton = true;
    public float changingWindowsYPos;
    public float varyingNoiseTime;


    public TileDrawInstance(GameObject _tile, int _maximumTile)
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
            if (!localTile.isInDisplay)
                continue;
            localTile.isWindows = windowTiles.Contains(localTile);

            float distanceToCenter = Vector2.Distance(new Vector2(localTile.initialXZPosition.x, localTile.initialXZPosition.z), new Vector2(centerPosition.x, centerPosition.z));
            float highRiseInfluence = Mathf.InverseLerp(innerRadius, outerRadius, distanceToCenter);
            float noise = Mathf.PerlinNoise(localTile.initialXZPosition.x / 10 + varyingNoiseTime, localTile.initialXZPosition.z / 10 + varyingNoiseTime);
            Vector3 groundPosition = localTile.GetGroundPosition();
            localTile.SetgroundPosition(groundPosition);
            Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y + highRiseInfluence * multiplier + noise * noiseWeight, groundPosition.z) + Vector3.up * yPos;

            if (activateWindowsIndependance && localTile.isWindows)
            {
                newPos = new Vector3(groundPosition.x, changingWindowsYPos, groundPosition.z);
            }
            localTile.SetTileFinalPosition(newPos);
            localTile.UpdateTileSmoothDampPos();
            localTile.GetTransformMatFromPos(localTile.OffsetTileAlignWithGround(localTile.smoothedFinalXYZPosition, 0.5f), Vector3.one * originalTileBound.x * 0.01f);

        }
    }
    public void UpdateTileDampSpeedTogether(float dampSpeed)
    {
        for (int i = 0; i < tileOrderedDict.Count; i++)
        {
            tileOrderedDict.ElementAt(i).Value.dampSpeed = dampSpeed;
        }
    }

    public void UpdateTileDampSpeedForLanding()
    {
        for (int i = 0; i < tileOrderedDict.Count; i++)
        {
            TileData localTile = tileOrderedDict.ElementAt(i).Value;
            if (!localTile.isInDisplay)
                continue;
            float distanceToCenter = Vector2.Distance(new Vector2(FirstPersonController.playerGroundPosition.x, FirstPersonController.playerGroundPosition.z), new Vector2(localTile.initialXZPosition.x, localTile.initialXZPosition.z));
            float dampMask = Mathf.InverseLerp(0, 7, distanceToCenter);
            localTile.dampSpeed = Mathf.Lerp(0.5f, 0.15f, dampMask);
        }
    }

    public void TeleportMatrixAlongY(float yOffset)
    {
        for (int i = 0; i < tileOrderedDict.Count; i++)
        {
            TileData localTile = tileOrderedDict.ElementAt(i).Value;
            Vector3 currentFormation = localTile.initialXZPosition;
            localTile.OverwriteTileSmoothDampPos(new Vector3(currentFormation.x, yOffset, currentFormation.z));
            localTile.GetTransformMatFromPos(localTile.finalXYZPosition, Vector3.one * originalTileBound.x * 0.01f);
        }
    }


    public void DrawTileInstanceCurrentFrame(bool includeWindow)
    {
        Matrix4x4[] tileMatrix = !includeWindow ?
            tileOrderedDict.Values.Select(x => x.transformMat).ToArray() :
             tileOrderedDict.Values.Where(x => !x.isWindows).Select(x => x.transformMat).ToArray();
        Graphics.DrawMeshInstanced(
            tileMesh,
            0,
            tileMat,
            tileMatrix,
            tileMatrix.Length,
            new MaterialPropertyBlock(),
            ShadowCastingMode.On,
            true,
            14);
    }


    
    public class TileData
    {
        public Vector3 initialXZPosition, groundXYZPosition, finalXYZPosition;
        public Vector3 smoothedFinalXYZPosition, refPos;
        public Vector2 globalTileCoord, localTileCoord;
        public Vector3 tileBound;
        private RaycastHit botHit;
        private LayerMask groundMask;
        public Matrix4x4 transformMat;
        public bool isInDisplay;
        public bool isWindows;
        public float dampSpeed;

        public enum DisplayState { tile, save, delete }
        public DisplayState displayState;


        public TileData(Vector3 _tileBound)
        {
            initialXZPosition = Vector3.zero;
            smoothedFinalXYZPosition = Vector3.zero;
            groundXYZPosition = Vector3.zero;
            finalXYZPosition = Vector3.zero;
            globalTileCoord = Vector2.zero;
            localTileCoord = Vector2.zero;
            tileBound = _tileBound;
            groundMask = LayerMask.GetMask("Ground");
            botHit = new RaycastHit();
            transformMat = Matrix4x4.identity;
            isInDisplay = false;
            isWindows = false;
            dampSpeed = 0.2f;
        }
        public void SetTilePositionAndGlobalCoordinate(Vector3 _position, Vector3 _globalTileCoord)
        {
            initialXZPosition = _position;
            smoothedFinalXYZPosition = _position;
            globalTileCoord = _globalTileCoord;
        }
        public void UpdateTileSmoothDampPos()
        {
            smoothedFinalXYZPosition = Vector3.SmoothDamp(smoothedFinalXYZPosition, finalXYZPosition, ref refPos, dampSpeed, 10000f);
        }
        public void SetgroundPosition(Vector3 _newPos) 
        {
            groundXYZPosition = _newPos;
        }
        public void OverwriteTileSmoothDampPos(Vector3 _newPos)
        {
            smoothedFinalXYZPosition = _newPos;
        }

        public void SetTileFinalPosition(Vector3 _newPos)
        {
            finalXYZPosition = _newPos;
        }
        public void SetDisplay(DisplayState state)
        {
            displayState = state;
        }
        public Vector3 GetGroundPosition()
        {
            Vector3 groundPos = initialXZPosition;
            Ray botRay = new Ray(groundPos + Vector3.up * 10000f, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, float.MaxValue, groundMask))
            {
                groundPos = botHit.point;
            }
            return groundPos;
        }
        public Vector3 OffsetTileAlignWithGround(Vector3 pos, float groundOffset)
        {
            return pos + Vector3.down * (tileBound.y / 2 - groundOffset);
        }
        public Matrix4x4 GetTransformMatFromPos(Vector3 pos, Vector3 scale)
        {
            transformMat = Matrix4x4.TRS(pos, Quaternion.identity, scale);
            return transformMat;
        }

        public void ToggleIsInDisplay(bool _display)
        {
            isInDisplay = _display;
        }
        public void ToggleIsWindows(bool _window)
        {
            isWindows = _window;
        }
    }

}
