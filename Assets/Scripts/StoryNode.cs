using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNode : MonoBehaviour
{

    public StoryTreeManager manager;
    public StoryNode previousNode;
    public List<StoryNode> nextNodes;
    public LineRenderer connectionLine;
    public float lineOffset;

    public string text;
    public TMPro.TextMeshPro textRenderering;





    public void OnTriggerEnter(Collider other)
    {

        print("HIIII");

        manager.OnStoryEntered(this);


    }

    public void OnTriggerExit(Collider other)
    {

        print("BYEEE");
        manager.OnStoryLeft(this);
    }

    public Vector3 getArch(Vector3 start, Vector3 end, float nID)
    {
        Vector3 basePos = (start - end) * nID + end;

        basePos += Vector3.up * Mathf.Clamp(1 - Mathf.Pow(nID * 2 - 1, 2), 0, 1) * lineOffset;

        basePos = transform.InverseTransformPoint(basePos);

        return basePos;
    }

    public int archCount = 50;

    // Start is called before the first frame update
    void OnEnable()
    {

        if (previousNode != null)
        {

            connectionLine.positionCount = archCount;
            for (int i = 0; i < archCount; i++)
            {
                connectionLine.SetPosition(i, getArch(previousNode.transform.position, transform.position, i / (float)archCount));
            }

        }
        else
        {
            connectionLine.positionCount = archCount;
            for (int i = 0; i < archCount; i++)
            {
                connectionLine.SetPosition(i, getArch(manager.transform.position, transform.position, i / (float)archCount));
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
