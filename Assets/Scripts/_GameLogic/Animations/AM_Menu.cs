using UnityEngine;
using System;

public class AM_Menu : AnchorAnimation
{
    [SerializeField]private AnimationCurve curve;

    public static event Action OnZoomFinished;
    private void OnEnable()
    {
        SceneManager.OnZoomingToScreenRequested += InitiateZoomToScreen;
    }

    private void OnDisable()
    {
        SceneManager.OnZoomingToScreenRequested -= InitiateZoomToScreen;
    }
    void InitiateZoomToScreen(Vector3 targetPosition, Quaternion lookDirection)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        PlayerAnchorTask(targetPosition, Vector3.zero, lookDirection, 0.3f, 0.3f, 0.05f, player, curve, false, false, true, true, false, TurnOnCanvasDisplay, null);
    }

    void TurnOnCanvasDisplay() 
    {
        OnZoomFinished?.Invoke();
    }
}
