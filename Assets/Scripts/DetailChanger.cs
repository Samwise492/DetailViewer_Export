using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DetailChanger : MonoBehaviour
{
    public void ChangeOnNewDetail(List<Detail> details)
    {
        int currentIndex = 0;

        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
                currentIndex = details.IndexOf(detail);
        }
        foreach (Detail detail in details)
        {
            detail.gameObject.SetActive(false);
        }
        foreach (Detail detail in details)
        {
            if (details.IndexOf(detail) == currentIndex+1)
                detail.gameObject.SetActive(true);
        }
    }
    public void ChangeOnPreviousDetail(List<Detail> details)
    {
        int currentIndex = 0;

        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
                currentIndex = details.IndexOf(detail);
        }
        foreach (Detail detail in details)
        {
            detail.gameObject.SetActive(false);
        }
        foreach (Detail detail in details)
        {
            if (details.IndexOf(detail) == currentIndex-1)
            {
                detail.gameObject.SetActive(true);
            }
        }
    }
}
