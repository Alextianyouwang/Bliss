using UnityEngine;

public class FunctionTilesStructureData
{
    private FunctionTile[] buttonArray = new FunctionTile[4];
    private SaveButton saveButton;
    private DeleteButton deleteButton;
    private TileMatrixStructureData t;
   // public bool displayAndUpdateButton  = true;

    public FunctionTilesStructureData (TileMatrixStructureData _t, SaveButton _saveButton, DeleteButton _deleteButton) 
    {
        t = _t;
        saveButton = _saveButton;
        deleteButton = _deleteButton;

        for (int i = 0; i < 4; i++)
        {
            buttonArray[i] = new FunctionTile(Object.Instantiate(saveButton), Object.Instantiate(deleteButton));
        }
    }

    public void ToggleSaveHasBeenClicked(bool b)
    {
        for (int i = 0; i < 4; i++)
        {
            buttonArray[i].save.hasBeenClicked = b;
        }
    }
    public void UpdateButtonPosition(FunctionTile.DisplayState state)
    {
        int counter = 0;
        for (int i = 0; i < 4; i++)
        {
            if (t.windowTiles[i] == null)
            {
                counter += 1;
            }

        }
        if (counter == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (state == FunctionTile.DisplayState.off)
                {
                    buttonArray[i].SetDisplay(FunctionTile.DisplayState.off);
                    t.windowTiles[i].ToggleOccupied(false);
                   
                }
                else
                {
                    t.windowTiles[i].ToggleOccupied(true);
                    buttonArray[i].SetPosition(t.windowTiles[i].smoothedFinalXYZPosition);
                    if (buttonArray[i].displayState != state)
                        buttonArray[i].SetDisplay(state);
                }

            }
        }
    }
   

    
}
