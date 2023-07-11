
using UnityEngine;

public class TileData
{
    public Vector3 initialXZPosition, groundXYZPosition, finalXYZPosition;
    public Vector3 smoothedFinalXYZPosition, refPos;
    public Vector3 screenPos;
    public Vector2 globalTileCoord, localTileCoord;
    public Vector3 tileBound;
    private RaycastHit botHit;
    private LayerMask groundMask;
    public Matrix4x4 transformMat;
    public bool isInDisplay;
    public bool isWindows;
    public float dampSpeed;
    public float screenPosDistanceToScreenCenter;

    public bool occupied = false;

    public enum DisplayState { tile, save, delete }
    public DisplayState displayState;

    public TileData(Vector3 _tileBound)
    {
        initialXZPosition = Vector3.zero;
        smoothedFinalXYZPosition = Vector3.zero;
        groundXYZPosition = Vector3.zero;
        screenPos = Vector3.zero;
        finalXYZPosition = Vector3.zero;
        globalTileCoord = Vector2.zero;
        localTileCoord = Vector2.zero;
        tileBound = _tileBound;
        groundMask = LayerMask.GetMask("Ground");
        botHit = new RaycastHit();
        transformMat = Matrix4x4.identity;
        isInDisplay = false;
        isWindows = false;
        occupied = false;
        dampSpeed = 0.2f;
        screenPosDistanceToScreenCenter = 0;
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

    public void ToggleOccupied(bool _occupy) 
    {
        occupied = _occupy;
    }
}

