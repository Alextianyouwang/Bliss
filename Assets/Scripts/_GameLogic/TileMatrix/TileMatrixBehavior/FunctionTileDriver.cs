using UnityEngine;
public class FunctionTileDriver : MonoBehaviour
{
    private TileMatrixStructureData t;
    private FunctionTilesStructureData b;
    private SaveButton sb;
    private DeleteButton db;
    private void OnEnable()
    {
        TileMatrixDriver.OnShareTileStructureData += InitializeTileButtons;
        TileMatrixDriver.OnCallFunctionTileUpdate += FunctionTileUpdate;
    }
    private void OnDisable()
    {
        TileMatrixDriver.OnShareTileStructureData -= InitializeTileButtons;
        TileMatrixDriver.OnCallFunctionTileUpdate -= FunctionTileUpdate;
    }

    private void InitializeTileButtons(TileMatrixStructureData _t) 
    {
        t = _t;
        LoadButtons();
        InitializeButtons();
    }

    private void LoadButtons() 
    {
        if (SceneDataMaster.sd == null)
        {
            print("SceneDataMaster Doesn't Exist.");
            return;
        }
        sb = SceneDataMaster.sd.saveButton_prefab.GetComponent<SaveButton>();
        db = SceneDataMaster.sd.deleteButton_prefab.GetComponent<DeleteButton>();
    }
    private void InitializeButtons() 
    {
        if (!sb || !db ||t == null)
            return;
        b = new FunctionTilesStructureData(t, sb, db);
    }
    private void FunctionTileUpdate() 
    {
        b.UpdateButtonPosition(FunctionTile.DisplayState.save);

    }

}
