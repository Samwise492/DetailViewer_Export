using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    SlicingObject slicingObject;
    RelocationComponent relocationComponent;
    DetailInfoHandler detailInfoHandler;
    DetailChanger detailChanger;
    Transform detailsStorage;
    List<Detail> details = new List<Detail>();
    [SerializeField] Button previousButton, nextButton;

    void Start()
    {
        slicingObject = GameObject.FindObjectOfType<SlicingObject>();
        relocationComponent = GameObject.FindObjectOfType<RelocationComponent>();
        detailInfoHandler = gameObject.GetComponent<DetailInfoHandler>();
        detailChanger = gameObject.GetComponent<DetailChanger>();

        detailsStorage = GameObject.FindGameObjectWithTag("Details Storage").transform;
        detailInfoHandler.InitialiseNewDetail();
        InitialiseDetails();
    }

    public void OnClickSlice()
    {
        InitialiseDetails();

        if (relocationComponent.IsRelocated)
            relocationComponent.ReturnOriginalDetail();
        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
            {
                if (detail.IsActionMade)
                    detail.UndoAction();
            }
        }

        if (!slicingObject.IsSliced)
            StartCoroutine(slicingObject.MakeSlice());
    }
    // Разлет дочерних объектов
    public void OnClickRelocate()
    {
        InitialiseDetails();

        if (slicingObject.IsSliced)
            slicingObject.ReturnOriginalDetail();
        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
            {
                if (detail.IsActionMade)
                    detail.UndoAction();
            }
        }
        
        if (!relocationComponent.IsRelocated)
            relocationComponent.Relocate();
    }
    public void OnClickActivateSkill()
    {
        if (slicingObject.IsSliced)
            slicingObject.ReturnOriginalDetail();
        if (relocationComponent.IsRelocated)
            relocationComponent.ReturnOriginalDetail();

        InitialiseDetails();

        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
            {
                if (!detail.IsActionMade)
                    detail.Action();
            }
        }
    }
    public void OnClickChooseNewDetail()
    {
        InitialiseDetails();

        if (DetailPoolAvailability().Contains(availabilityEnum.nextAvailable))
        {
            detailChanger.ChangeOnNewDetail(details);
            detailInfoHandler.InitialiseNewDetail();
        }
    }
    public void OnClickChoosePreviousDetail()
    {
        InitialiseDetails();

        if (DetailPoolAvailability().Contains(availabilityEnum.previousAvailable))
        {
            detailChanger.ChangeOnPreviousDetail(details);
            detailInfoHandler.InitialiseNewDetail();
        }
    }
    public void OnClickReturn()
    {
        InitialiseDetails();
        
        if (slicingObject.IsSliced)
            slicingObject.ReturnOriginalDetail();
        if (relocationComponent.IsRelocated)
            relocationComponent.ReturnOriginalDetail();
        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
            {
                if (detail.IsActionMade)
                    detail.UndoAction();
            }
        }
    }

    void InitialiseDetails()
    {
        details.Clear();
        var countIndex = 0;

        foreach (Transform detail in detailsStorage)
        {
            if (detail.GetComponent<Detail>() == null)
                Debug.LogError("На объекте номер " + countIndex + " нет компонента Detail. Добавьте компонент");
            else
                details.Add(detail.GetComponent<Detail>());

            countIndex++;
        }
    }
    List<availabilityEnum> DetailPoolAvailability()
    {
        List<availabilityEnum> _enum = new List<availabilityEnum>();

        foreach (Detail detail in details)
        {
            if (detail.gameObject.activeSelf)
            {
                if (details.IndexOf(detail) == 0)
                {
                    previousButton.enabled = false;
                    _enum.Add(availabilityEnum.previousNotAvailable);
                    _enum.Add(availabilityEnum.nextAvailable);
                }
                else if (details.IndexOf(detail) == details.Count-1)
                {
                    nextButton.enabled = false;
                    _enum.Add(availabilityEnum.previousAvailable);
                    _enum.Add(availabilityEnum.nextNotAvailable);
                }
                else
                {
                    previousButton.enabled = true;
                    nextButton.enabled = true;
                    _enum.Add(availabilityEnum.previousAvailable);
                    _enum.Add(availabilityEnum.nextAvailable);
                }
            }
        }

        return _enum;
    }
    enum availabilityEnum
    {
        previousAvailable,
        previousNotAvailable,
        nextAvailable,
        nextNotAvailable
    }
}
