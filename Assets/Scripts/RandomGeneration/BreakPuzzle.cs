using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakPuzzle : BaseInput
{
    public List<GameObject> breakableParts = new List<GameObject>();

    public AudioClip breakableClip;
    
    void Update()
    {
        if (IsTouchingObject() && breakableParts.Count > 0 && !isCompleted)
        {
            int randomIndex = Random.Range(0, breakableParts.Count);
            Destroy(breakableParts[randomIndex]);
            breakableParts.RemoveAt(randomIndex);

            AudioSource.PlayClipAtPoint(breakableClip, output.transform.position);
        }

        if(breakableParts.Count <= 0)
        {
            if (!isCompleted)
            {

                CompletionFeedBack();

                isCompleted = true;
            }
        }
    }
}
