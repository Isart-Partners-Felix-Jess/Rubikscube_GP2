using UnityEditor;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Camera m_Camera = null;
    [SerializeField] private Transform m_Cube = null;

    private float m_DistanceFromTarget = 6;

    //private Vector3 m_PreviousPos = Vector3.zero;

    private void Start()
    {
        if (m_Camera == null || m_Cube == null)
            ErrorDetected("One or multiple field unset in CameraBehaviour");

        ReloadCamera();
    }

    void ErrorDetected(string _error)
    {
        Debug.LogError(_error);
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }

    public void SizeChanged(uint _newSize)
    {
        m_DistanceFromTarget = 2 * _newSize;
        ReloadCamera();
    }

    private void Update()
    {
        if(Input.mouseScrollDelta.y != 0f)
        {
            m_Camera.fieldOfView -= Input.mouseScrollDelta.y;
        }
    }

    private void ReloadCamera()
    {
        transform.position = m_Cube.position;
        transform.Translate(new Vector3(0, 0, -m_DistanceFromTarget));
    }
}
