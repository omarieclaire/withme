using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{

    public int numFacePairs;
    public float faceSize;
    public GameObject facePrefab;
    public float faceVerticality;
    public List<HugFace> faces;

    public Controller controller;

    public float activationRadius;

    public void OnEnable()
    {

        while (faces.Count > 0)
        {
            HugFace face = faces[0];
            faces.RemoveAt(0);
            Destroy(faces[0].gameObject);
        }

        faces.Clear();
        for (int i = 0; i < numFacePairs; i++)
        {
            HugFace face1 = Instantiate(facePrefab).GetComponent<HugFace>();
            face1.transform.position = controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
            face1.transform.localScale = Vector3.one * faceSize;


            HugFace face2 = Instantiate(facePrefab).GetComponent<HugFace>();
            face2.transform.position = controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
            face2.transform.localScale = Vector3.one * faceSize;


            face1.hug = this;
            face2.hug = this;

            face1.smileID = i;
            face2.smileID = i;

            face1.partner = face2.gameObject;
            face2.partner = face1.gameObject;

            face1.gameObject.SetActive(true);
            face2.gameObject.SetActive(true);

            faces.Add(face1);
            faces.Add(face2);


        }

    }


    public void Update()
    {

        for (int i = 0; i < controller.activePlayers.Count; i++)
        {

            int closestFaceID = -1;
            float closestFaceDistance = 1000000;
            for (int j = 0; j < faces.Count; j++)
            {

                float distance = Vector3.Distance(controller.activePlayers[i].transform.position, faces[j].transform.position);
                if (distance < closestFaceDistance)
                {
                    closestFaceDistance = distance;
                    closestFaceID = j;
                }

            }


            if (closestFaceDistance < activationRadius)
            {


            }


        }

    }

}
