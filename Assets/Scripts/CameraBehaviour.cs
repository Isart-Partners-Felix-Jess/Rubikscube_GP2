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
        /*if (Input.GetMouseButtonDown(1))
        {
            m_PreviousPos = m_Camera.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 newPos = m_Camera.ScreenToViewportPoint(Input.mousePosition);
            Vector3 dir = m_PreviousPos - newPos;

            float angleXAxis = dir.y * 180f;
            float angleYAxis = -dir.x * 180f;

            transform.position = m_Cube.position;

            Quaternion xAxis = new Quaternion(1, 0, 0, angleXAxis / 2f);
            Quaternion yAxis = new Quaternion(0, 1, 0, angleYAxis / 2f);

            transform.rotation = xAxis * transform.rotation;// * Quaternion.Inverse(xAxis);
            //transform.rotation = yAxis * transform.rotation * Quaternion.Inverse(yAxis);

            //transform.Rotate(new Vector3(0, 1, 0), angleYAxis, Space.World); // TODO: CHANGER CA, PAS LE DROIT, C'EST PAS LEGAL, NEED MATH FROM FELIX LATER

            transform.Translate(new Vector3(0, 0, -m_DistanceFromTarget));

            m_PreviousPos = newPos;
        }*/
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
