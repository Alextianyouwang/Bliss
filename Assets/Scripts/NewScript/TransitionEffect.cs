using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class TransitionEffect : MonoBehaviour
{
    public GameObject tile;
    public GameObject saveButton;
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
        int itemBelowMiddle = (int)Mathf.Floor(maximumTileDiemnsion / 2);
        int itemAboveMiddle = (int)Mathf.Ceil(maximumTileDiemnsion / 2);
        for (int i = 0; i < maximumTileDiemnsion; i++)
        {
            for (int j = 0; j < maximumTileDiemnsion; j++)
            {
                tileMatrixRefPool[i, j] = new Tile(groundMask);
                tileMatrixRefPool[i, j].InstantiateTile(tile, Vector3.zero, null);
                tileMatrixRefPool[i, j].TileSetActive(false);
                tilePool.Enqueue(tileMatrixRefPool[i, j]);
                //tileMatrixRefPool[i, j].localIndex = new Vector2(i, j);
                //tileMatrixRefPool[i, j].SetDebugText( "( " +i.ToString() + "," + j.ToString() + " )");
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
        toRecycle.formationFinalPosition = Vector3.zero;
        toRecycle.localTileCoord = Vector2.zero;
        toRecycle.tileObject_instance.transform.localScale = toRecycle.originalScale;
        tilePool.Enqueue(toRecycle);
    }

    void SwapTileFormationFinalPosition() 
    {
    
    }

    public class Tile
    {
        
        public GameObject tileObject_instance;
        public Text debugText;
        public Vector3 formationFinalPosition;
        public Vector2 localTileCoord;
        private RaycastHit botHit;
        private LayerMask groundMask;
        public Vector3 originalScale;
        public Vector3 tileRefSpeed,tileFinalPosition = Vector3.zero;
        public Vector2 localIndex;

        public Tile( LayerMask _groundMask)
        {

            groundMask = _groundMask;
        }

        public void InstantiateTile(GameObject reference, Vector3 position, Transform parent)
        {
            tileObject_instance = Instantiate(reference);
            tileObject_instance.transform.position = position;
            tileObject_instance.transform.parent = parent;
            originalScale = tileObject_instance.transform.localScale;
            debugText = reference.GetComponentInChildren<Text>();
        }
        public void SetDebugText( string content) 
        {
            //print(content);

            if(tileObject_instance.GetComponentInChildren<Text>()!=null)
                tileObject_instance.GetComponentInChildren<Text>().text = content;
            //print(tileObject_instance.GetComponent<Text>().text);
        }
        public void SetTilePosition(Vector3 position, bool setFinalPosition)
        {
            tileObject_instance.transform.position = position;
            tileFinalPosition = position;
            if (setFinalPosition)
            {
                formationFinalPosition = position;  
            }
        }
        public void SetTileLocalScale(Vector3 localScale)
        {
            tileObject_instance.transform.localScale = localScale;
        }
        public void StickTileToGround()
        {
            SetTilePosition(formationFinalPosition + Vector3.up * 80, true);
            Ray botRay = new Ray(formationFinalPosition, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, 100f, groundMask))
            {
                formationFinalPosition.y = botHit.point.y;
                SetTilePosition(formationFinalPosition, true);
            }
        }
        public Vector3 GetGroundPosition()
        {
            Vector3 groundPos = formationFinalPosition;
            Ray botRay = new Ray(groundPos + Vector3.up * 80, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, 100f, groundMask))
            {
                groundPos = botHit.point;
            }
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
            if (Vector3.Distance(t.Value.formationFinalPosition, groundPos) >= radius)
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
                Vector2 localTileCoord = new Vector2(Mathf.Floor((tileVirtualPosition.x - transform.position.x) / originalTileBound.x), Mathf.Floor((tileVirtualPosition.z - transform.position.z) / originalTileBound.z));
                Vector3 finalPosition = new Vector3(tileCoord.x * originalTileBound.x, 0, tileCoord.y * originalTileBound.z) + new Vector3(startPosition.x, groundPos.y, startPosition.z);
                
                if (!tileDict.ContainsKey(tileCoord) && Vector3.Distance(finalPosition, groundPos) < radius && tilePool.Count > 0)
                {
                    Tile localTile = GetNextTile(finalPosition);
                    localTile.localTileCoord = localTileCoord;
                    
                    tileDict.Add(tileCoord, localTile);

                   
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

                    float distanceToCenter = Vector3.Distance(localTile.formationFinalPosition, centerPosition);
                    float highRiseInfluence = Mathf.InverseLerp(innerRadius, outerRadius, distanceToCenter);
                    float noise = Mathf.PerlinNoise(localTile.formationFinalPosition.x / 10 + Time.time / 3, localTile.formationFinalPosition.z / 10 + Time.time / 3);
                    Vector3 groundPosition = localTile.GetGroundPosition();
                    Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y + highRiseInfluence * multiplier + noise * noiseWeight, groundPosition.z);
                    localTile.tileFinalPosition = Vector3.SmoothDamp(localTile.tileFinalPosition,newPos, ref localTile.tileRefSpeed,dampSpeed);

                    localTile.SetTilePosition(localTile.tileFinalPosition, false);

                    Vector2 localTileCoord = new Vector2(
                        Mathf.Floor((localTile.formationFinalPosition.x - transform.position.x + originalTileBound.x/2) / originalTileBound.x), 
                        Mathf.Floor((localTile.formationFinalPosition.z - transform.position.z + originalTileBound.z/2) / originalTileBound.z));
                    localTile.localTileCoord = localTileCoord;
                    localTile.SetDebugText("( " + localTile.localTileCoord.x.ToString() + "," + localTile.localTileCoord.y.ToString() + " )");

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
                UpdateTilesStatusPerFrame(0,originalRadius,varyingRadius/2,0.5f,0.7f);
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
        float highRiseMultipler = 0.5f;
        while (fileStagingCo!= null) 
        {
            UpdateTilesStatusPerFrame(innerRadius, targetRadius, highRiseMultipler, 0.5f,0.3f);
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
