using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sliceable))]
public class Detail : MonoBehaviour
{
    [SerializeField] string _name, description;
    [HideInInspector] public string _Name => _name;
    [HideInInspector] public string Description => description;
    Material materialComponent;
    Color defaultColor;
    bool isActionMade;
    public bool IsActionMade => isActionMade;

    void OnEnable()
    {
        materialComponent = gameObject.GetComponent<Renderer>().material;
        defaultColor = materialComponent.color;
    }

    // Действие по умолчанию
    public virtual void Action()
    {
        materialComponent = gameObject.GetComponent<Renderer>().material;
        materialComponent.color = Color.green;

        isActionMade = true;
    }
    public void UndoAction()
    {
        materialComponent.color = defaultColor;

        isActionMade = false;
    }
}
