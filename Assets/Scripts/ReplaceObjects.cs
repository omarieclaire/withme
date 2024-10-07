using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ReplaceObjects : MonoBehaviour
{
    public GameObject oldObject;  // The old object to replace
    public GameObject newObject;  // The new object to replace it with

    [ContextMenu("Replace All References In Scene")]
    void ReplaceAllReferencesInScene()
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Replace oldObject with newObject in all components that reference it
            SerializedObject so = new SerializedObject(obj);
            SerializedProperty prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == oldObject)
                {
                    prop.objectReferenceValue = newObject;
                    so.ApplyModifiedProperties();
                }
            }
        }

        Debug.Log("All references to the old object have been replaced.");
    }
}
