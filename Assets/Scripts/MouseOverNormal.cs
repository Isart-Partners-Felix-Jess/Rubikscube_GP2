using UnityEngine;

public class MouseOverNormal
{
    public Vector3 Normal { get; private set; } = Vector3.zero;
    public GameObject SelectedObject { get; private set; } = null;

    public void Update()
    {
        // Check if the mouse is over a collider
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if we're on a face
            if (hit.transform.CompareTag("ext"))
            {
                // The hit.normal vector contains the normal of the surface where the mouse is hovering
                Normal = hit.normal;
                SelectedObject = hit.transform.gameObject;
            }
        }
        else
        {
            Normal = Vector3.zero;
            SelectedObject = null;
        }
    }
}