using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepVisibility : MonoBehaviour
{
    private List<Collider> steps = new List<Collider>();

    //Testing
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Steps"))
        {
            ///StartCoroutine(BlockSpawn(other.gameObject));
            other.GetComponent<MeshRenderer>().enabled = true;
            steps.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (steps.Contains(other))
        {
            other.GetComponent<MeshRenderer>().enabled = false;
            steps.Remove(other);
        }
    }

    //private void Update()
    //{
    //    foreach (var step in steps)
    //    {
    //        StartCoroutine(BlockSpawn(step.gameObject));
    //    }
    //}

    //private IEnumerator BlockSpawn(GameObject module, float speed = 10, bool isSpawning = true)
    //{
    //    module.GetComponent<MeshRenderer>().enabled = isSpawning;

    //    Vector3 target = module.transform.localScale;
    //    Vector3 start = Vector3.zero;

    //    if (!isSpawning)
    //    {
    //        target = Vector3.zero;
    //        start = module.transform.localScale;
    //    }

    //    module.transform.localScale = start;

    //    while (module.transform.localScale == target)
    //    {
    //        // lerpValue += speed / 100;
    //        module.transform.localScale = Vector3.Lerp(start, target, Time.deltaTime*speed);
    //        print(module.transform.localScale);

    //        yield return null;
    //    }

    //    print("bruh");
    //}
}
