using UnityEngine;

public class TileButtons 
{
    private ButtonTile[] buttonArray = new ButtonTile[4];
    private SaveButton saveButton;
    private DeleteButton deleteButton;
    private TileDrawInstance t;

    public TileButtons(TileDrawInstance _t, SaveButton _saveButton, DeleteButton _deleteButton) 
    {
        t = _t;
        saveButton = _saveButton;
        deleteButton = _deleteButton;

        for (int i = 0; i < 4; i++)
        {
            buttonArray[i] = new ButtonTile(Object.Instantiate(saveButton), Object.Instantiate(deleteButton));
        }
    }

    public void ToggleSaveHasBeenClicked(bool b)
    {
        for (int i = 0; i < 4; i++)
        {
            buttonArray[i].save.hasBeenClicked = b;
        }
    }
    public void UpdateButtonPosition(ButtonTile.DisplayState state)
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
                if (t.allowWindowsSetPrefabToButtons)
                {
                    buttonArray[i].SetPosition(t.windowTiles[i].smoothedFinalXYZPosition);
                    if (buttonArray[i].displayState != state)
                        buttonArray[i].SetDisplay(state);
                }
                else
                {
                    buttonArray[i].SetDisplay(ButtonTile.DisplayState.off);
                }

            }
        }
    }

    public class ButtonTile
    {
        public SaveButton save;
        public DeleteButton delete;
        private GameObject buttonComposite;
        private Vector3 bounds;

        public enum DisplayState { off, save, delete }
        public DisplayState displayState;

        public ButtonTile(SaveButton _save, DeleteButton _delete)
        {
            save = _save;
            delete = _delete;
            buttonComposite = new GameObject("ButtonComposite");
            save.transform.parent = buttonComposite.transform;
            save.transform.localPosition = Vector3.zero;
            save.gameObject.SetActive(false);
            delete.transform.parent = buttonComposite.transform;
            delete.transform.localPosition = Vector3.zero;
            delete.gameObject.SetActive(false);
            bounds = save.GetComponent<MeshRenderer>().bounds.size;
        }

        public void SetPosition(Vector3 pos)
        {
            buttonComposite.transform.position = pos - Vector3.up * (bounds.y / 2 - 0.5f);
        }
        public void SetDisplay(DisplayState state)
        {
            switch (state)
            {
                case DisplayState.save:
                    save.gameObject.SetActive(true);
                    delete.gameObject.SetActive(false);
                    break;
                case DisplayState.delete:
                    save.gameObject.SetActive(false);
                    delete.gameObject.SetActive(true);

                    break;
                case DisplayState.off:
                    save.gameObject.SetActive(false);
                    delete.gameObject.SetActive(false);
                    break;
            }
            displayState = state;

        }

    }
}
