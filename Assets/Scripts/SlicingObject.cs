using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicingObject : MonoBehaviour
{
    [SerializeField] GameObject startSlicePoint, endSlicePoint; // Далее встречаются как ESP и SSP
    [SerializeField] float forceAppliedToCut = 3f;
    [SerializeField] GameObject backUpStorage;
    GameObject originalDetailsStorage;
    Vector3 triggerEnterPosition_SSP, triggerEnterPosition_ESP;
    Vector3 triggerExitPosition_ESP;
    Vector3 defaultPosition;
    bool isSliced;
    public bool IsSliced => isSliced;
    int originalObjectIndex;

    void Start()
    {
        defaultPosition = transform.position;
        originalDetailsStorage = GameObject.FindGameObjectWithTag("Details Storage");
    }
    void OnTriggerEnter(Collider other)
    {
        BackUpOriginalDetail(other);

        triggerEnterPosition_ESP = endSlicePoint.transform.position;
        triggerEnterPosition_SSP = startSlicePoint.transform.position;
    }
    void OnTriggerExit(Collider other)
    {
        isSliced = true;
        StartCoroutine(SliceObject(other));
    }
    
    void BackUpOriginalDetail(Collider other)
    {
        if (other.transform.parent.name == originalDetailsStorage.name)
        {
            var _obj = Instantiate(other.gameObject, backUpStorage.transform);
            _obj.transform.position = other.gameObject.transform.position;
            _obj.name = other.gameObject.name;
            originalObjectIndex = other.gameObject.transform.GetSiblingIndex();
            _obj.SetActive(false);  
        }
    }
    public void ReturnOriginalDetail()
    {
        foreach(Transform detail in backUpStorage.transform)
        {
            if (detail.parent.name == backUpStorage.name)
            {
                detail.SetParent(originalDetailsStorage.transform);
                detail.gameObject.SetActive(true);
                detail.SetSiblingIndex(originalObjectIndex);
            }
        }
        foreach(Transform slicedDetail in Sliceable.SlicedObjectsNest.transform)
        {
            Destroy(slicedDetail.gameObject);
        }
        isSliced = false;
    }
    
    public IEnumerator MakeSlice()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            
            if (transform.position.y > -0.2f)
            {
                transform.position += new Vector3(transform.position.x, -0.3f, transform.position.z);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                transform.position = defaultPosition;
                yield break;
            }
        }
    }
    IEnumerator SliceObject(Collider other)
    {
        yield return new WaitForSeconds(0.1f);

        triggerExitPosition_ESP = endSlicePoint.transform.position;

        // Создаем треугольник между конеч. точкой и начальной, чтобы получить вектор, перпендикулярный плоскости
        Vector3 side1 = triggerExitPosition_ESP - triggerEnterPosition_ESP;
        Vector3 side2 = triggerExitPosition_ESP - triggerEnterPosition_SSP;

        // Получаем точку перпендикуляра треугольника выше которой находится вектор, перпендикулярный плоскости
        Vector3 normal = Vector3.Cross(side1, side2).normalized;

        // Трансформируем этот вектор, чтобы он соединялся с объектом, у которого делаем разрез
        Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

        // Получаем входную позицию, равную local transform объекта разрезки
        Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(triggerEnterPosition_ESP);

        Plane plane = new Plane();

        plane.SetNormalAndPosition(transformedNormal, transformedStartingPoint);
        var direction = Vector3.Dot(Vector3.up, transformedNormal);

        // Переворачиваем плоскость, чтобы всегда знать где находится какая сторона
        if (direction < 0)
        {
            plane = plane.flipped;
        }

        Sliceable.sidesNumberToCreate = 1;
        GameObject[] slices = Sliceable.Slice(plane, other.gameObject);
        Destroy(other.gameObject);

        Rigidbody rigidbody = new Rigidbody();
        if (Sliceable.sidesNumberToCreate == 1)
            rigidbody = slices[0].GetComponent<Rigidbody>();
        else if (Sliceable.sidesNumberToCreate == 2)
            rigidbody = slices[1].GetComponent<Rigidbody>();

        Vector3 newNormal = transformedNormal + Vector3.up * forceAppliedToCut;
        rigidbody.AddForce(newNormal, ForceMode.Impulse);

        yield break;
    }
}
