using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeData
{
    public Vector2 fromPos;
    public Vector2 toPos;
    public Vector2 curSwipe;
    public bool    swipeEnded;

    public Vector2 GetRoundedSwipe(float roundingAngle)
    {
        if (swipeEnded)
        {
            float angle        = Vector2.SignedAngle(Vector2.right, curSwipe);
            float roundedAngle = Mathf.Round(angle / roundingAngle) * roundingAngle;
            return new Vector2(Mathf.Cos(Mathf.Deg2Rad * roundedAngle), Mathf.Sin(Mathf.Deg2Rad * roundedAngle));
        }
        return Vector2.zero;
    }
}

public class MouseSwipeController : MonoBehaviour
{
    public  Vector3 mouseDelta { get; private set; }
    private Vector3 prevMousePosition;
    private SwipeData[] swipeData = { new SwipeData(), new SwipeData() };

    void Update()
    {
        mouseDelta = Input.mousePosition - prevMousePosition;

        for (int i = 0; i < 2; i++) {
            if (Input.GetMouseButtonDown(i)) {
                swipeData[i].fromPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }
            if (Input.GetMouseButtonUp(i)) {
                swipeData[i].toPos    = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                swipeData[i].curSwipe = new Vector2(swipeData[i].toPos.x - swipeData[i].fromPos.x, 
                                                    swipeData[i].toPos.y - swipeData[i].fromPos.y).normalized;
                swipeData[i].swipeEnded = true;
            }
            else if (swipeData[i].swipeEnded)
            {
                swipeData[i].fromPos    = Vector2.zero;
                swipeData[i].toPos      = Vector2.zero;
                swipeData[i].curSwipe   = Vector2.zero;
                swipeData[i].swipeEnded = false;
            }
        }

        prevMousePosition = Input.mousePosition;
    }

    public SwipeData LeftSwipe()
    {
        return swipeData[0];
    }

    public SwipeData RightSwipe()
    {
        return swipeData[1];
    }
}
