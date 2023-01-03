using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct RotationEvent
{
    public Vector3 axis;
    public int     slice;
    public bool    reverse;
}

public class CubeResolve : MonoBehaviour
{
    [SerializeField] private float resolveSpeed = 500;

    public  bool                resolving { get; private set; } = false;
    private List<RotationEvent> rotationEvents                  = new List<RotationEvent>();
    private int                 curRotationEvent                = -1;
    private CubeRotation        cubeRotation;
    private CubeShuffle         cubeShuffle;

    private TextMeshProUGUI resolvingText;
    private Button          resolveButton;
    private Button          undoButton;
    private Button          redoButton;


    void Awake()
    {
        cubeRotation  = gameObject.GetComponent<CubeRotation>();
        cubeShuffle   = gameObject.GetComponent<CubeShuffle >();
        resolvingText = GameObject.Find("ResolvingText").GetComponent<TextMeshProUGUI>();
        resolveButton = GameObject.Find("ResolveButton").GetComponent<Button>();
        undoButton    = GameObject.Find("UndoButton"   ).GetComponent<Button>();
        redoButton    = GameObject.Find("RedoButton"   ).GetComponent<Button>();
        
        resolveButton.onClick.AddListener(StartResolve);
        undoButton.onClick.AddListener(Undo);
        redoButton.onClick.AddListener(Redo);
    }

    private IEnumerator ResolveCube()
    {
        resolving = true;
        float cubeRotationSpeed = cubeRotation.rotationSpeed;
        cubeRotation.rotationSpeed = resolveSpeed;
        StartCoroutine(Extensions.FadeText(resolvingText, 0.8f, true));

        // Iterate over the rotations in reverse order and undo them one by one.
        for (int i = curRotationEvent; i >= 0; i--) {
            while (cubeRotation.rotating)
                yield return null;
            
            cubeRotation.RotateSlice(rotationEvents[i].axis, rotationEvents[i].slice, !rotationEvents[i].reverse);
        }

        StartCoroutine(Extensions.FadeText(resolvingText, 0.8f, false));
        rotationEvents.Clear();
        curRotationEvent = -1;
        cubeRotation.rotationSpeed = cubeRotationSpeed;
        resolving = false;
    }

    private void StartResolve()
    {
        if (!resolving && !cubeShuffle.shuffling)
            StartCoroutine(ResolveCube());
    }

    public void StopResolve()
    {
        if (resolving) {
            resolving = false;
            StopAllCoroutines();
            StartCoroutine(Extensions.FadeText(resolvingText, 0.8f, false));
        }
    }

    public void AddRotationEvent(Vector3 rotationAxis, int rotationSlice, bool rotationReverse)
    {
        if (curRotationEvent + 1 != rotationEvents.Count)
            rotationEvents.RemoveRange(curRotationEvent + 1, rotationEvents.Count - curRotationEvent - 1);

        rotationEvents.Add(new RotationEvent{ 
            axis    = rotationAxis, 
            slice   = rotationSlice, 
            reverse = rotationReverse 
        });
        curRotationEvent++;
    }

    public void ResetHistory()
    {
        rotationEvents.Clear();
        curRotationEvent = -1;
    }

    public void Undo()
    {
        if (!cubeRotation.rotating && !resolving && !cubeShuffle.shuffling && curRotationEvent >= 0) 
        {
            cubeRotation.RotateSlice(rotationEvents[curRotationEvent].axis, 
                                     rotationEvents[curRotationEvent].slice, 
                                    !rotationEvents[curRotationEvent].reverse, false);
            curRotationEvent--;
        }
    }

    public void Redo()
    {
        if (!cubeRotation.rotating && !resolving && !cubeShuffle.shuffling && curRotationEvent + 1 < rotationEvents.Count) 
        {
            curRotationEvent++;
            cubeRotation.RotateSlice(rotationEvents[curRotationEvent].axis, 
                                     rotationEvents[curRotationEvent].slice, 
                                     rotationEvents[curRotationEvent].reverse, false);
        }
    }
}
