using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BaseInput : MonoBehaviour
{
    //Base Class for all input types

    //Output references
    public GameObject output;
    public int outputInstanceID;

    //Puzzle Completion Check
    public bool isCompleted;

    //Puzzle Completion Feedback
    public AudioClip completionSFX;
    public GameObject pePrefab;

    //Touchscreen handler stuff
    public Touch touchScreenHandle;

    //TouchScreen Handler for touching a puzzle Collider
    //
    //ignoreTouchCount -> Checks for multiple keys as well as the begining and end of a touch
    public bool IsTouchingObject(Collider collider, bool ignoreTouchCount = false)
    {
        if (GenerateTower.isConstructed)
        {
            if (Input.touchCount > 0)
                touchScreenHandle = Input.GetTouch(0);

            if (Input.touchCount > 0)
            {
                Ray touchPosWorld = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

                RaycastHit touchRayInformation;

                Physics.Raycast(touchPosWorld, out touchRayInformation);

                if (touchRayInformation.collider == collider)
                {
                    if (ignoreTouchCount)
                        return true;
                    else
                    {
                        if (touchScreenHandle.phase == TouchPhase.Began)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }

        }

        return false;
    }

    //Handler that gets its own collider
    public bool IsTouchingObject(bool ignoreTouchCount = false)
    {
        Collider m_collider = GetComponent<Collider>();

        return IsTouchingObject(m_collider, ignoreTouchCount);
    }


    //Puzzle Completion Code
    //
    //If any other feedback to add - use it within this function
    public void CompletionFeedBack()
    {
        Instantiate(pePrefab, output.transform.position, pePrefab.transform.rotation, output.transform);
        AudioSource.PlayClipAtPoint(completionSFX, output.transform.position);

        //Insert Here Coin Addition
    }
}
