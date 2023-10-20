using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OnMouseOverColor : MonoBehaviour
{
    public Action onMouseOverAction;

    // When the mouse hovers over the GameObject, it turns to this color (red)
    private Color m_MouseOverColor = new(0.5f, 0.5f, 0.5f, 0.5f);

    // This stores the GameObject’s original color
    private Color m_OriginalColor;

    // Get the GameObject’s mesh renderer to access the GameObject’s material and color
    private MeshRenderer m_Renderer;

    private void Start()
    {
        // Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();
        // Fetch the original color of the GameObject
        m_OriginalColor = m_Renderer.material.color;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            m_Renderer.material.color = m_OriginalColor;
        // Change the color of the GameObject to red when the mouse is over GameObject
        else
        {
            m_Renderer.material.color = m_MouseOverColor;
            onMouseOverAction?.Invoke();
        }
    }

    private void OnMouseExit()
    {
        if (Input.GetMouseButton(0))
            return;
        else
            // Reset the color of the GameObject back to normal
            m_Renderer.material.color = m_OriginalColor;
    }
}