using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPuzzle : BaseInput
{
    GameObject steps;

    private void Update()
    {
        if( output!=null && steps == null)
        {
            steps = output.transform.GetChild(4).gameObject;
        }

        if (!isCompleted)
        {
            if (IsTouchingObject())
            {
                steps.SetActive(true);

                CompletionFeedBack();

                isCompleted = true;
            }
        }
    }
}
