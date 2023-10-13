using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(BoxCollider))]
public class OnMouseOverColor : MonoBehaviour
{
    bool m_IsHighlighted = false;
    public Action onMouseOverAction;

    //When the mouse hovers over the GameObject, it turns to this color (red)
    Color m_MouseOverColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    //This stores the GameObject’s original color
    Color m_OriginalColor;

    //Get the GameObject’s mesh renderer to access the GameObject’s material and color
    MeshRenderer m_Renderer;

    void Start()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();
        //Fetch the original color of the GameObject
        m_OriginalColor = m_Renderer.material.color;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            m_Renderer.material.color = m_OriginalColor;
        // Change the color of the GameObject to red when the mouse is over GameObject
        else
        {
            m_Renderer.material.color = m_MouseOverColor;
            m_IsHighlighted = true;
            onMouseOverAction?.Invoke();
        }
    }

    void OnMouseExit()
    {
        if (Input.GetMouseButton(0))
            return;
        else
        {
            // Reset the color of the GameObject back to normal
            m_Renderer.material.color = m_OriginalColor;
            m_IsHighlighted = false;
        }
    }
}