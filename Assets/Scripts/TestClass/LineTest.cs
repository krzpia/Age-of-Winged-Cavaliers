using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTest : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform[] points;
    [SerializeField] private DragLineController line;

    void Start()
    {
        // OBSOLETE
        //line.SetUpLine(points);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
