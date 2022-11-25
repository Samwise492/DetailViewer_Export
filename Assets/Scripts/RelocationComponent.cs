using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RelocationComponent : MonoBehaviour
{
    List<Vector3> defaultComponentPosition = new List<Vector3>();
    bool isRelocated;
    public bool IsRelocated => isRelocated;
    float scalingValue = 2f;
    float speedValue = 0.05f;

    public void Relocate()
    {
        isRelocated = true;

        foreach(Transform detail in gameObject.transform)
        {
            if (detail.childCount > 0)
            {
                foreach (Transform part in detail)
                {
                    defaultComponentPosition.Add(part.position);
                    
                    StartCoroutine(FlyAwayComponent(part, detail));
                }
            }
        }
    }
    public void ReturnOriginalDetail()
    {
        var index = 0;

        foreach(Transform detail in gameObject.transform)
        {
            if (detail.childCount > 0)
            {
                foreach (Transform part in detail)
                {
                    part.transform.position = defaultComponentPosition.ElementAt(index);
                    index++;
                }
            }
        }

        isRelocated = false;
    }

    IEnumerator FlyAwayComponent(Transform part, Transform detail)
    {
        var leftEndPoint = part.localPosition.x-scalingValue;
        var rightEndPoint = part.localPosition.x+scalingValue;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            
            // дочерний объект находится правее детали
            if (part.position.x > detail.position.x)
            {
                if (part.localPosition.x <= rightEndPoint)
                    part.localPosition += new Vector3(speedValue, 0, 0);
                else yield break;
            }
            // дочерний объект находится левее детали
            else if (part.position.x < detail.position.x)
            {
                if (part.localPosition.x >= leftEndPoint)
                    part.localPosition -= new Vector3(speedValue, 0, 0);
                else yield break;
            }
        }
    }
}
