using UnityEngine;

public class FunctionTile
{
    public SaveButton save;
    public DeleteButton delete;
    private GameObject buttonComposite;
    private Vector3 bounds;

    public enum DisplayState { off, save, delete }
    public DisplayState displayState;

    public FunctionTile(SaveButton _save, DeleteButton _delete)
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