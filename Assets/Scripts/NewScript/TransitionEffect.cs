using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TransitionEffect : MonoBehaviour
{
    public GameObject tile;
    public Queue<Tile> tilePool = new Queue<Tile>();
    public Tile[,] tileMatrixRefPool;
    private Dictionary<Vector2, Tile> tileDict = new Dictionary<Vector2, Tile>();
    private Vector3 originalTileBound,varyingTileBound;
    private Vector3 originalScale;

    public int maximumTileDiemnsion = 7;
    public float originalRadius = 15;
    private float varyingRadius, currentRadius;

    private float formationSideLength;
    private Vector3 formationOffset,startPosition,playerGroundPosition,centerPosition;
    

    public LayerMask groundMask;

    float scaleAnimation = 0;
    RaycastHit playerGroundHit;
    private Coroutine fileStagingCo;
    public enum TileStates {NormalFollow, Staging }
    public TileStates state;

    private bool isInClippy = false;

    private void OnEnable()
    {
        FirstPersonController.OnPitchChange += ChangeRadius;
        WorldTransition.OnStageFile += StartStagingFile;
        WorldTransition.OnStageFileEnd += EndStagingFile;
        WorldTransition.OnClippyToggle += ReceiveToggleClippy;
    }
    private void OnDisable()
    {
        FirstPersonController.OnPitchChange -= ChangeRadius;
        WorldTransition.OnStageFile -= StartStagingFile;
        WorldTransition.OnStageFileEnd -= EndStagingFile;
        WorldTransition.OnClippyToggle -= ReceiveToggleClippy;



    }
    #region TileManagement
    void Initialize() 
    {
        tileMatrixRefPool = new Tile[maximumTileDiemnsion, maximumTileDiemnsion];
        originalScale = tile.transform.localScale;
        startPosition = transform.position;
        originalTileBound = tile.GetComponent<Renderer>().bounds.size;
        formationSideLength = maximumTileDiemnsion * originalTileBound.x;
        formationOffset = new Vector3(formationSideLength / 2 - originalTileBound.x / 2, 0, formationSideLength / 2 - originalTileBound.y / 2);
        for (int i = 0; i < maximumTileDiemnsion; i++)
        {
            for (int j = 0; j < maximumTileDiemnsion; j++)
            {
                tileMatrixRefPool[i,j] = new Tile(tile,groundMask);
                tileMatrixRefPool[i, j].InstantiateTile(Vector3.zero, null);
                tileMatrixRefPool[i, j].TileSetActive(false);
                tilePool.Enqueue(tileMatrixRefPool[i, j]);             
            }
        }
    }
    Tile GetNextTile(Vector3 position)
    {
        Tile newTile = tilePool.Dequeue();
        newTile.SetTilePosition(position, true);
        newTile.TileSetActive(true);
        return newTile;
    }
    void RecycleTile(Tile toRecycle)
    {
        toRecycle.TileSetActive(false);
        toRecycle.finalPosition = Vector3.zero;
        toRecycle.tileCoord = Vector2.zero;
        toRecycle.tileObject_instance.transform.localScale = toRecycle.originalScale;
        tilePool.Enqueue(toRecycle);
    }

    public class Tile
    {
        public GameObject tileObject;
        public GameObject tileObject_instance;
        public Vector3 finalPosition;
        public Vector2 tileCoord;
        private RaycastHit botHit;
        private LayerMask groundMask;
        public Vector3 originalScale;
        public Vector3 tileRefSpeed,targetPos = Vector3.zero;

        public Tile(GameObject _tileObject, LayerMask _groundMask)
        {
            tileObject = _tileObject;
            groundMask = _groundMask;
        }

        public void InstantiateTile(Vector3 position, Transform parent)
        {
            tileObject_instance = Instantiate(tileObject);
            tileObject_instance.transform.position = position;
            tileObject_instance.transform.parent = parent;
            originalScale = tileObject_instance.transform.localScale;
        }
        public void SetTilePosition(Vector3 position, bool setFinalPosition)
        {
            tileObject_instance.transform.position = position;
            if (setFinalPosition)
            {
                finalPosition = position;
                targetPos = position;
            }

        }
        public void SetTileLocalScale(Vector3 localScale)
        {
            tileObject_instance.transform.localScale = localScale;
        }
        public void StickTileToGround()
        {
            SetTilePosition(finalPosition + Vector3.up * 80, true);
            Ray botRay = new Ray(finalPosition, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, 100f, groundMask))
            {
                finalPosition.y = botHit.point.y;
                SetTilePosition(finalPosition, true);
            }
        }
        public Vector3 GetGroundPosition()
        {
            Vector3 initlalPos = finalPosition;
            Vector3 groundPos = finalPosition;
            SetTilePosition(groundPos + Vector3.up * 80, false);
            Ray botRay = new Ray(groundPos, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, 100f, groundMask))
            {
                groundPos = botHit.point;
            }
            SetTilePosition(initlalPos, false);
            return groundPos;
        }
        public void TileSetActive(bool active)
        {
            tileObject_instance.SetActive(active);
        }
        public void DestroyTile()
        {
            Destroy(tileObject_instance);
        }

    }

    void UpdateTiles(Vector3 groundPos, float radius)
    {
        for (int i = 0; i < tileDict.Values.Count; i++)
        {
            var t = tileDict.ElementAt(i);
            if (Vector3.Distance(t.Value.finalPosition, groundPos) >= radius)
            {
                tileDict.Remove(t.Key);
                RecycleTile(t.Value);
            }
        }
        for (int i = 0; i < maximumTileDiemnsion; i++)
        {
            for (int j = 0; j < maximumTileDiemnsion; j++)
            {
                Vector3 tileVirtualPosition = groundPos + new Vector3(i * originalTileBound.x, 0, j * originalTileBound.z) - formationOffset;
                Vector2 tileCoord = new Vector2(Mathf.Floor((tileVirtualPosition.x - startPosition.x) / originalTileBound.x), Mathf.Floor((tileVirtualPosition.z - startPosition.z) / originalTileBound.z));
                Vector3 finalPosition = new Vector3(tileCoord.x * originalTileBound.x, 0, tileCoord.y * originalTileBound.z) + new Vector3(startPosition.x, groundPos.y, startPosition.z);
                if (!tileDict.ContainsKey(tileCoord) && Vector3.Distance(finalPosition, groundPos) < radius && tilePool.Count > 0)
                {
                    Tile localTile = GetNextTile(finalPosition);
                    localTile.tileCoord = tileCoord;
                    tileDict.Add(tileCoord, localTile);
                    //localTile.StickTileToGround();
                }
            }
        }
    }

    void UpdateTilesStatusPerFrame(float innerRadius, float outerRadius, float multiplier, float noiseWeight, float dampSpeed)
    {
        for (int i = 0; i < maximumTileDiemnsion; i++)
        {
            for (int j = 0; j < maximumTileDiemnsion; j++)
            {

                Tile localTile = tileMatrixRefPool[i, j];
                if (localTile.tileObject_instance.activeSelf)
                {
                    float distanceToCenter = Vector3.Distance(localTile.finalPosition, centerPosition);
                    float highRiseInfluence = Mathf.InverseLerp(innerRadius, outerRadius, distanceToCenter);
                    float noise = Mathf.PerlinNoise(localTile.finalPosition.x / 10 + Time.time / 3, localTile.finalPosition.z / 10 + Time.time / 3);
                    Vector3 groundPosition = localTile.GetGroundPosition();
                    //Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y + highRiseInfluence * multiplier + noise * noiseWeight, groundPosition.z);
                    Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y , groundPosition.z);

                    //localTile.finalPosition = Vector3.SmoothDamp(localTile.finalPosition,newPos, ref localTile.tileRefSpeed,dampSpeed);
                    //localTile.tileObject_instance.transform.position = Vector3.SmoothDamp(localTile.tileObject_instance.transform.position, newPos, ref localTile.tileRefSpeed, dampSpeed);

                    localTile.SetTilePosition(newPos, false);
                }
            }
        }
    }
    #endregion 

    void Start()
    {
        Initialize();
    }
    void Update()
    {
        if (!isInClippy) 
        {
            switch (state) 
            {
            case TileStates.NormalFollow:
                UpdatePlayerGroundRay();
                UpdateTiles(playerGroundPosition,varyingRadius);
                UpdateTilesStatusPerFrame(0,originalRadius,varyingRadius/2,0.5f,0.3f);
                centerPosition = playerGroundPosition;
                currentRadius = varyingRadius;
                break;
            case TileStates.Staging:
                break;
            }
        }
    }
    #region EventSubscribtion
    void ChangeRadius(float pitch) 
    {
        float percentage = Mathf.InverseLerp(30, 80, pitch);
        varyingRadius = Mathf.Lerp(0, originalRadius, percentage);
    }

    void ReceiveToggleClippy(bool inClippy) 
    {
        isInClippy = inClippy;
    }
    public void StartStagingFile(Vector3 target) 
    {
        if (!isInClippy)
            fileStagingCo = StartCoroutine(FileStagingAnimation(target));
    }
    public void EndStagingFile() 
    {
        if (fileStagingCo != null)
            StopCoroutine(fileStagingCo);
        fileStagingCo = null;
        state = TileStates.NormalFollow;
    }
    #endregion
    IEnumerator FileStagingAnimation(Vector3 target)
    {
        state = TileStates.Staging;
        float percent = 0;
        float initialRaidius = currentRadius;
        float targetRadius = 5;
        Vector3 currentPosition = centerPosition;
        while (percent < 1) 
        {
            percent += Time.deltaTime;
            centerPosition = Vector3.Lerp(currentPosition, target, percent);
            currentRadius = Mathf.Lerp(initialRaidius, targetRadius, percent);
            Vector3 path = Vector3.Lerp(currentPosition, target, percent);
            float newRadius = Mathf.Lerp(initialRaidius, targetRadius, percent);
            UpdateTiles(path, newRadius);
            UpdateTilesStatusPerFrame(0, targetRadius, 0, 0, 0.3f);
            yield return null;
        }
        float innerRadius = targetRadius * 0.5f;
        float highRiseMultipler = 2f;
        while (fileStagingCo!= null) 
        {
            
            UpdateTilesStatusPerFrame(innerRadius, targetRadius, highRiseMultipler, 2,0.3f);
            yield return null;
        }
    }
    void UpdatePlayerGroundRay() 
    {
        Ray playerDownRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(playerDownRay, out playerGroundHit, 100f, groundMask))
        {
            playerGroundPosition = playerGroundHit.point;
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, originalRadius);
    }
}
