using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGO_fpsCamera : MonoBehaviour
{
    /// <summary>
    /// MGO
    /// </summary>
    public float stepSize = .3f;

    private float xPlane = 0;
    private float yPlane = 0;
    private Vector3 mousePos;
    void Start()
    {
        mousePos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
            this.transform.Translate(transform.forward * stepSize);
        if (Input.GetKey(KeyCode.S))
            this.transform.Translate(transform.forward * -1f * stepSize);
        if (Input.GetKey(KeyCode.A))
            this.transform.Translate(transform.right * -1f * stepSize);
        if (Input.GetKey(KeyCode.D))
            this.transform.Translate(transform.right * stepSize);
        Vector3 currMouse = Input.mousePosition;
        if (currMouse == mousePos)
            return;


        Vector3 mouseOffset = mousePos - currMouse;
        mousePos = currMouse;

        transform.Rotate(mouseOffset.y, -mouseOffset.x, 0);
    }
}
