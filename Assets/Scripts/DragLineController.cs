using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragLineController : MonoBehaviour
{
    private LineRenderer lr;
    // OBSOLETE private Transform[] points;
    private Vector3 startPos;
    private Vector3 endPos;

    public bool isDrawing = false;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();  
    }

    // OBSOLETE
    //public void SetUpLine(Transform[] points)
    //{
    //    lr.positionCount= points.Length;
    //    this.points = points;
    //}

    void Start()
    {
        
    }

    public void DrawLine(Vector3 startPos, Vector2 endPos)
    {
        isDrawing = true;
        this.startPos = new Vector3(startPos.x, startPos.y, -1f);
        this.endPos = new Vector3(endPos.x, endPos.y, -1f);
        //Debug.Log("DRAW LINE, line renderer: " + lr);
    }

    public void EndDrawLine()
    {
        lr.SetPosition(0, new Vector3(0,0,0));
        lr.SetPosition(1, new Vector3(0,0,0));
        isDrawing= false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDrawing)
        {
            // OBSOLETE //
            //for (int i = 0; i < points.Length; i++)
            //{
            //    lr.SetPosition(i, points[i].position);
            //}
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
        }
        
    }
}
