using UnityEngine;

public class TileMatrixEvent : MonoBehaviour
{
    private TileMatrixStats s;
    private void OnEnable()
    {
        TileMatrixDriver.OnCallStatsUpdate += UpdateStats;
        TileMatrixDriver.OnShareTileStats += ReceiveTileMatrixStats;
    }
    private void OnDisable()
    {
        TileMatrixDriver.OnCallStatsUpdate -= UpdateStats;
        TileMatrixDriver.OnShareTileStats -= ReceiveTileMatrixStats;


    }

    void ReceiveTileMatrixStats(TileMatrixStats _s) 
    {
        s = _s; 
    }

    TileMatrixStats UpdateStats() 
    {
        s.center = transform.position;
        return s;
    }
}
