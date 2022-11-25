using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class Sliceable : MonoBehaviour
{ 
    static List<int> zValues = new List<int>();
    public static int sidesNumberToCreate;
    static GameObject slicedObjectsNest;
    public static GameObject SlicedObjectsNest => slicedObjectsNest;
    public static GameObject[] Slice(Plane plane, GameObject objectToCut)
    {            
        if (slicedObjectsNest == null)
        {
            slicedObjectsNest = new GameObject();
            slicedObjectsNest.name = "Sliced Objects";
            Instantiate(slicedObjectsNest);

            var garbageClone = GameObject.Find(slicedObjectsNest.name + "(Clone)");
            if (garbageClone!= null)
                Destroy(garbageClone);
        }

        Mesh mesh = objectToCut.GetComponent<MeshFilter>().mesh;
        Sliceable sliceable = objectToCut.GetComponent<Sliceable>();
        
        SlicesMetadata slicesMeta = new SlicesMetadata(plane, mesh); // Данные для создания задней и передней стороны 

        GameObject backObject = null;
        GameObject frontObject = null;

        if (sidesNumberToCreate == 1)
        {
            backObject = CreateMeshGameObject(objectToCut);
            backObject.name = string.Format("{0}_back", objectToCut.name);

            var backSideMeshData = slicesMeta.backSideMesh;
            backObject.GetComponent<MeshFilter>().mesh = backSideMeshData;

            SetupCollidersAndRigidBodies(ref backObject, backSideMeshData);

            return new GameObject[] { backObject};
        }
        if (sidesNumberToCreate == 2)
        {
            frontObject = CreateMeshGameObject(objectToCut);
            frontObject.name = string.Format("{0}_front", objectToCut.name);
            
            var frontSideMeshData = slicesMeta.frontSideMesh;
            frontObject.GetComponent<MeshFilter>().mesh = frontSideMeshData;

            SetupCollidersAndRigidBodies(ref frontObject, frontSideMeshData);

            return new GameObject[] { frontObject, backObject};
        }
        
        return null;
    }        
    static GameObject CreateMeshGameObject(GameObject originalObject)
    {
        var originalMaterial = originalObject.GetComponent<MeshRenderer>().materials;

        GameObject meshGameObject = new GameObject();

        meshGameObject.AddComponent<MeshFilter>();
        meshGameObject.AddComponent<MeshRenderer>();

        meshGameObject.GetComponent<MeshRenderer>().materials = originalMaterial;

        meshGameObject.transform.localScale = originalObject.transform.localScale;
        meshGameObject.transform.rotation = originalObject.transform.rotation;
        ObjectPosition_UncrossTextures(originalObject, meshGameObject);
        
        meshGameObject.transform.parent = slicedObjectsNest.transform;
        meshGameObject.tag = originalObject.tag;

        return meshGameObject;
    }

    // Добавляем небольшое отдаление/приближение, чтобы текстуры объектов не пересекались
    static void ObjectPosition_UncrossTextures(GameObject originalObject, GameObject meshGameObject)
    {
        DetailComponentMoveHandler detailComponentMoveHandler = GameObject.FindGameObjectWithTag("Details Storage").GetComponent<DetailComponentMoveHandler>();

        if (detailComponentMoveHandler.zValueIndex == 1)
            meshGameObject.transform.position = new Vector3 (originalObject.transform.position.x, originalObject.transform.position.y,
            originalObject.transform.position.z+0.005f);
        else if (detailComponentMoveHandler.zValueIndex == 2)
            meshGameObject.transform.position = new Vector3 (originalObject.transform.position.x, originalObject.transform.position.y,
            originalObject.transform.position.z+0.006f);
        else if (detailComponentMoveHandler.zValueIndex == 3)
            meshGameObject.transform.position = new Vector3 (originalObject.transform.position.x, originalObject.transform.position.y,
            originalObject.transform.position.z-0.005f);
        else if (detailComponentMoveHandler.zValueIndex == 4)
            meshGameObject.transform.position = new Vector3 (originalObject.transform.position.x, originalObject.transform.position.y,
            originalObject.transform.position.z-0.006f);
        
        
        zValues.Add(detailComponentMoveHandler.zValueIndex);
        detailComponentMoveHandler.zValueIndex += 1;

        if (zValues.Count == 4)
        {
            zValues.Clear();
            detailComponentMoveHandler.zValueIndex = 1;
        }
    }
    static void SetupCollidersAndRigidBodies(ref GameObject gameObject, Mesh mesh)
    {                     
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        var rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }
}
