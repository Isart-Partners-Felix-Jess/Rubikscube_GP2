using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverNormal
{
    public Vector3 normal { get; private set; } = Vector3.zero;
    public GameObject selectedObject { get; private set; } = null;

    public void Update()
    {
        // Check if the mouse is over a collider
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            //check if we're on a face
            if (hit.transform.CompareTag("ext"))
            {
                // The hit.normal vector contains the normal of the surface where the mouse is hovering
                normal = hit.normal;
                selectedObject = hit.transform.gameObject;
            }
        }
        else
        {
            normal = Vector3.zero;
            selectedObject = null;
        }
    }
}