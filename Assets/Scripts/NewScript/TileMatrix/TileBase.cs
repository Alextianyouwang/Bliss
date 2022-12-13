using UnityEngine;
using UnityEngine.UI;

public class TileBase
{
    public GameObject display_instance;
    public GameObject tileObject_instance, saveButton_instance, deleteButton_instance;
    private float tileObjectYOffset, saveButtonYOffset,deleteButtonYOffset;

    public Text debugText;
    public Vector3 formationFinalPosition;
    public Vector2 localTileCoord;
    private RaycastHit botHit;
    private LayerMask groundMask = LayerMask.GetMask("Ground");

    public Vector3 tileRefSpeed, tileFinalPosition = Vector3.zero, targetPosition = Vector3.zero;
    public bool isWindows = false;
    public float dampSpeed;

    public enum DisplayState {tile, save, delete }
    public DisplayState displayState;

    public void InstantiateTile(GameObject mainReference, GameObject saveButtonReference, GameObject deleteButtonReference, Vector3 position, Transform parent)
    {
        tileObject_instance = mainReference;
        tileObjectYOffset = -tileObject_instance.GetComponent<Renderer>().bounds.size.y / 2 + 0.2f;

        saveButton_instance = saveButtonReference;
        saveButtonYOffset = -saveButton_instance.GetComponent<Renderer>().bounds.size.y / 2 + 0.2f;

        deleteButton_instance = deleteButtonReference;
        deleteButtonYOffset = -deleteButton_instance.GetComponent<Renderer>().bounds.size.y / 2 + 0.2f;



        display_instance = new GameObject();
        display_instance.name = "TileComposit";
        tileObject_instance.transform.parent = display_instance.transform;
        tileObject_instance.transform.localPosition = Vector3.zero + Vector3.up * tileObjectYOffset;
        saveButton_instance.transform.parent = display_instance.transform;
        saveButton_instance.transform.localPosition = Vector3.zero + Vector3.up * saveButtonYOffset;
        deleteButton_instance.transform.parent = display_instance.transform;
        deleteButton_instance.transform.localPosition = Vector3.zero + Vector3.up * deleteButtonYOffset;

        debugText = mainReference.GetComponentInChildren<Text>();
    }

    public void SetDisplay(DisplayState state) 
    {
        switch (state) 
        {
            case DisplayState.tile:
                saveButton_instance.SetActive(false);
                tileObject_instance.SetActive(true);
                deleteButton_instance.SetActive(false);
                break;
            case DisplayState.delete:
                saveButton_instance.SetActive(false);
                tileObject_instance.SetActive(false);
                deleteButton_instance.SetActive(true);
                break;
            case DisplayState.save:
                saveButton_instance.SetActive(true);
                tileObject_instance.SetActive(false);
                deleteButton_instance.SetActive(false);
                break;
        }
    }

    public void SetDebugText(string content)
    {
        if (display_instance.GetComponentInChildren<Text>() != null)
            display_instance.GetComponentInChildren<Text>().text = content;
    }
    public void SetTilePosition(Vector3 position, bool setFinalPosition)
    {
        display_instance.transform.position = position;
        tileFinalPosition = position;
        if (setFinalPosition)
        {
            formationFinalPosition = position;
        }
    }
    public void SetTileSmoothDampPos(Vector3 target)
    {
        tileFinalPosition = Vector3.SmoothDamp(tileFinalPosition, target, ref tileRefSpeed, dampSpeed, 10000f);
        SetTilePosition(tileFinalPosition, false);
    }

    public void StickTileToGround()
    {
        Ray botRay = new Ray(formationFinalPosition, Vector3.down);
        if (Physics.Raycast(botRay, out botHit, 1000f, groundMask))
        {
            SetTilePosition(botHit.point, false);
        }
    }
    public Vector3 GetGroundPosition()
    {
        Vector3 groundPos = formationFinalPosition;
        Ray botRay = new Ray(groundPos + Vector3.up * 10000f, Vector3.down);
        if (Physics.Raycast(botRay, out botHit, float.MaxValue, groundMask))
        {
            groundPos = botHit.point;
        }
        return groundPos;
    }
    public void TileSetActive(bool active)
    {
        display_instance.SetActive(active);
    }

}
