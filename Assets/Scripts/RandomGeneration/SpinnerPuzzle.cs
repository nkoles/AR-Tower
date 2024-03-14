using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerPuzzle : BaseInput
{
    public float spinTime = 3f;
    public float timeHeld = 0f;

    public Transform spinnerCrank;
    public Collider spinnerCollider;

    public Transform step1, step2;

    public float step1DefaultRotation, step2DefaultRotation;

    public AudioSource gearSFX;

    private void Update()
    {
        if (output != null)
        {
            if (step1 == null)
            {
                step1 = output.transform.GetChild(4);

                step1DefaultRotation = step1.localRotation.z;
            }

            if (step2 == null)
            {
                step2 = output.transform.GetChild(5);

                step2DefaultRotation = step2.localRotation.z;
            }

            if(gearSFX == null)
            {
                gearSFX = output.GetComponent<AudioSource>();
            }
        }

        if (step1 != null && step2 != null)
        {
            if (timeHeld >= spinTime)
            {
                if (!isCompleted)
                {
                    CompletionFeedBack();

                    isCompleted = true;
                }
            }
            else
            {
                spinnerCrank.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360, timeHeld/spinTime));

                step1.eulerAngles = new Vector3(0, 0, Mathf.Lerp(step1DefaultRotation, 0, timeHeld / spinTime));
                step2.eulerAngles = new Vector3(0, 0, Mathf.Lerp(step2DefaultRotation, 0, timeHeld / spinTime));
            }
        }
    }

    private void FixedUpdate()
    {
        if(IsTouchingObject(spinnerCollider, true) && !isCompleted)
        {
            timeHeld += Time.fixedDeltaTime;
            gearSFX.volume = 1;
        } else
        {
            gearSFX.volume = 0;
        }
    }
}
