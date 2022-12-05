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
using Unity.VisualScripting;
using System;

public class ModularMatrix : MonoBehaviour
{
    public GameObject tile;
    public GameObject saveButton;
    public Queue<Tile> tilePool = new Queue<Tile>();
    public Tile[,] tileMatrixRefPool,tileMatrixRefPoolOrdered;
    private Dictionary<Vector2, Tile> tileDict = new Dictionary<Vector2, Tile>();
    private Dictionary<Vector2, Tile> tileOrderedDict = new Dictionary<Vector2, Tile>();
    private Tile[] windowTiles = new Tile[4];
    private Vector3 originalTileBound,varyingTileBound;
    private Vector3 originalScale;
    private Vector3 globalPositionOffset;
    private float highRiseMultiplierBoost,groundLevelMultiplier,noiseWeight = 0.5f,dampSpeed = 0.18f;

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
    private bool isInDiveFormation = false;
    public static Action<Vector3, Quaternion> OnInitiateTeleportFromMatrix;

    

    private void OnEnable()
    {
        FirstPersonController.OnPitchChange += ChangeRadius;
        WorldTransition.OnStageFile += StartStagingFile;
        WorldTransition.OnPlayerExitAnchor += EndStagingFile;
        WorldTransition.OnPlayerExitAnchor += SetIsInDiveForamtionFalse;
        WorldTransition.OnClippyToggle += ReceiveToggleClippy;
        FirstPersonController.OnIncreaseAnimationTime += ReceiveGlobalPositionOffset;
        FirstPersonController.OnExitThreshold += ResetGlobalPositionOffset;
        FirstPersonController.OnTeleporting += PrepareDiveAnimationPositionAndRotation;

    }
    private void OnDisable()
    {
        FirstPersonController.OnPitchChange -= ChangeRadius;
        WorldTransition.OnStageFile -= StartStagingFile;
        WorldTransition.OnPlayerExitAnchor -= EndStagingFile;
        WorldTransition.OnPlayerExitAnchor -= SetIsInDiveForamtionFalse;
        WorldTransition.OnClippyToggle -= ReceiveToggleClippy;
        FirstPersonController.OnIncreaseAnimationTime -= ReceiveGlobalPositionOffset;
        FirstPersonController.OnExitThreshold -= ResetGlobalPositionOffset;
        FirstPersonController.OnTeleporting -= PrepareDiveAnimationPositionAndRotation;


    }
    #region TileManagement
    void Initialize() 
    {
        tileMatrixRefPool = new Tile[maximumTileDiemnsion, maximumTileDiemnsion];
        tileMatrixRefPoolOrdered = new Tile[maximumTileDiemnsion, maximumTileDiemnsion];
        originalScale = tile.transform.localScale;
        startPosition = transform.position;
        originalTileBound = tile.GetComponent<Renderer>().bounds.size;
        formationSideLength = maximumTileDiemnsion * originalTileBound.x;
        formationOffset = new Vector3(formationSideLength / 2 - originalTileBound.x / 2, 0, formationSideLength / 2 - originalTileBound.z / 2);
        int itemBelowMiddle = (int)Mathf.Floor(maximumTileDiemnsion / 2);
        int itemAboveMiddle = (int)Mathf.Ceil(maximumTileDiemnsion / 2);
        for (int i = 0; i < maximumTileDiemnsion; i++)
        {
            for (int j = 0; j < maximumTileDiemnsion; j++)
            {
                tileMatrixRefPool[i, j] = new Tile(groundMask,-originalTileBound.y/2);
                tileMatrixRefPool[i, j].InstantiateTile(tile, Vector3.zero, null);
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
        toRecycle.formationFinalPosition = Vector3.zero;
        toRecycle.localTileCoord = Vector2.zero;
        toRecycle.tileObject_instance.transform.localScale = toRecycle.originalScale;
        tilePool.Enqueue(toRecycle);
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
        public Vector3 tileRefSpeed,tileFinalPosition = Vector3.zero,groundPosition =Vector3.zero;
        public Vector2 localIndex;
        private float yOffset;
        public bool isWindows = false;

        public Tile( LayerMask _groundMask,float _yOffset)
        {
            yOffset = _yOffset;
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
            if(tileObject_instance.GetComponentInChildren<Text>()!=null)
                tileObject_instance.GetComponentInChildren<Text>().text = content;
        }
        public void SetTilePosition(Vector3 position, bool setFinalPosition)
        {
            tileObject_instance.transform.position = position + Vector3.up * yOffset ;
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
            SetTilePosition(formationFinalPosition + Vector3.up * 800f, true);
            Ray botRay = new Ray(formationFinalPosition, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, 1000f, groundMask))
            {
                formationFinalPosition.y = botHit.point.y;
                SetTilePosition(formationFinalPosition, true);
            }
        }
        public Vector3 GetGroundPosition()
        {
            Vector3 groundPos = formationFinalPosition;
            Ray botRay = new Ray(groundPos + Vector3.up * 800, Vector3.down);
            if (Physics.Raycast(botRay, out botHit, 1000f, groundMask))
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
                Vector3 finalPosition = new Vector3(tileCoord.x * originalTileBound.x, 0, tileCoord.y * originalTileBound.z) + new Vector3(startPosition.x, groundPos.y, startPosition.z);             
                if (!tileDict.ContainsKey(tileCoord) && Vector3.Distance(finalPosition, groundPos) < radius && tilePool.Count > 0)
                {
                    Tile localTile = GetNextTile(finalPosition);                   
                    tileDict.Add(tileCoord, localTile);                 
                }         
            }
        }
    }

    void UpdateTileOrderedCoordinate(Vector3 centerPosition) 
    {
        tileOrderedDict.Clear();
        for (int i = 0; i < maximumTileDiemnsion; i++)
        {
            for (int j = 0; j < maximumTileDiemnsion; j++)
            {
                Tile localTile = tileMatrixRefPool[i, j];

                if (localTile.tileObject_instance.activeSelf) 
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
    void UpdateTilesStatusPerFrame(float innerRadius, float outerRadius, float multiplier, float noiseWeight, float dampSpeed, Vector3 centerPosition)
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
                    Vector3 newPos = new Vector3(groundPosition.x, groundPosition.y + highRiseInfluence * multiplier + noise * noiseWeight, groundPosition.z)+globalPositionOffset;
                    localTile.groundPosition = newPos;
                    localTile.tileFinalPosition = Vector3.SmoothDamp(localTile.tileFinalPosition,newPos, ref localTile.tileRefSpeed,dampSpeed);
                    localTile.SetTilePosition(localTile.tileFinalPosition, false);     
                }
                
            }
        }
       
    }

    void UpdateWindowTile(Vector3 comparePos)
    {
        if (tileOrderedDict.Count == 0)
            return;
        Tile center,up,bot,left,right,upRight,upLeft,botRight,botLeft;
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

        foreach (Tile t in windowTiles)
        {
            if (t != null) 
            {
                t.SetDebugText("Window");
                t.isWindows = true;

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
                UpdateTileOrderedCoordinate(transform.position);
                UpdateWindowTile(transform.position);
                UpdateTilesStatusPerFrame(0,originalRadius,varyingRadius/2 + highRiseMultiplierBoost,noiseWeight,dampSpeed,playerGroundPosition);

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
        float percentage = Mathf.InverseLerp(30, 75, pitch);
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
    void ReceiveGlobalPositionOffset(float y) 
    {
        globalPositionOffset.y = -y * 4;
        highRiseMultiplierBoost = y * 4;
        dampSpeed = Mathf.Lerp(0.18f, .7f, y);
    }
    void DiveFormation(float number) 
    {
            globalPositionOffset.y = -number;
            highRiseMultiplierBoost = number * 1.2f;
    }
    void ResetGlobalPositionOffset(float number) 
    {
        if (!isInDiveFormation) 
        {
            globalPositionOffset.y = 0;
            highRiseMultiplierBoost = 0;
            dampSpeed = 0.18f;

        }

    }

    void SetIsInDiveForamtionFalse() 
    {
        isInDiveFormation = false;
        dampSpeed = 0.18f;
    }
    void PrepareDiveAnimationPositionAndRotation(FirstPersonController player) 
    {
        Vector3 divePosition = Vector3.zero;
        for (int i= 0; i < windowTiles.Length; i++) 
        {
            divePosition += windowTiles[i].groundPosition;
        }
        divePosition /= windowTiles.Length;
        divePosition.y -=15f;

        float yRot = player.transform.eulerAngles.y;
        Vector3 finalEuler = Vector3.zero;
        if (yRot >= -45f && yRot < 45f)
        {
            finalEuler = Vector3.forward;
        }
        else if (yRot >= 45f && yRot < 135f)
        {
            finalEuler = Vector3.right;
        }
        else if (yRot >= 135f && yRot < 225f)
        {
            finalEuler = Vector3.back;
        }
        else if (yRot >= 225f && yRot < 315f) 
        {
            finalEuler = Vector3.left;
        }
        print(divePosition);
        Quaternion finalRot = Quaternion.LookRotation(Vector3.down, finalEuler);
        if (!isInClippy) 
        {
            OnInitiateTeleportFromMatrix?.Invoke(divePosition, finalRot); 
            isInDiveFormation = true;
            DiveFormation(32);
            
        }
            
    }

    #endregion
    IEnumerator FileStagingAnimation(Vector3 target)
    {
        state = TileStates.Staging;
        float percent = 0;
        float initialRaidius = currentRadius;
        float targetRadius = 5;
        Vector3 currentPosition = centerPosition;
        Vector3 path = Vector3.zero;
        while (percent < 1) 
        {
            percent += Time.deltaTime;
            centerPosition = Vector3.Lerp(currentPosition, target, percent);
            currentRadius = Mathf.Lerp(initialRaidius, targetRadius, percent);
            path = Vector3.Lerp(currentPosition, target, percent);
            float newRadius = Mathf.Lerp(initialRaidius, targetRadius, percent);
            UpdateTiles(path, newRadius);
            UpdateTileOrderedCoordinate(path);
            UpdateWindowTile(path);
            UpdateTilesStatusPerFrame(0, newRadius, 0.3f, noiseWeight, dampSpeed,path);
            yield return null;
        }
        float innerRadius = targetRadius * 0.5f;
        float highRiseMultipler = 1f;
        while (fileStagingCo!= null) 
        {
            UpdateTileOrderedCoordinate(path);
            UpdateWindowTile(path);
            UpdateTilesStatusPerFrame(innerRadius, targetRadius, highRiseMultipler, 1f,0.5f,path);
      

            yield return null;
        }
    }

    
    void UpdatePlayerGroundRay() 
    {
        Ray playerDownRay = new Ray(transform.position + Vector3.up * 800f, Vector3.down);
        if (Physics.Raycast(playerDownRay, out playerGroundHit, 2000f, groundMask))
        {
            playerGroundPosition = playerGroundHit.point;
        }

    }
   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, originalRadius);
        foreach (Tile t in tileOrderedDict.Values) 
        {
            Gizmos.DrawSphere(t.groundPosition, 0.5f);
        }
    }
}
