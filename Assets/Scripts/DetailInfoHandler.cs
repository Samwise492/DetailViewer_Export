using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailInfoHandler : MonoBehaviour
{
    Transform detailsStorage;
    [SerializeField] Text nameTextField, descriptionTextField;
    string detailName, detailDescription;

    void Awake()
    {
        detailsStorage = GameObject.FindGameObjectWithTag("Details Storage").transform;
    }

    void GetDetailData()
    {
        foreach (Transform detail in detailsStorage)
        {
            if (detail.gameObject.activeSelf)
            {
                detailName = detail.GetComponent<Detail>()._Name;
                detailDescription = detail.GetComponent<Detail>().Description;
            }
        }
    }
    public void InitialiseNewDetail()
    {
        GetDetailData();

        nameTextField.text = detailName;
        descriptionTextField.text = detailDescription;
    }
}
