using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class RubikBehaviour : MonoBehaviour
{
    private uint m_RubikSize = 3;
    private GameObject[] m_Cubes = new GameObject[0];
    [SerializeField] private GameObject m_CubePrefab = null;
    [SerializeField] private GameObject m_Camera = null;

    [Header("Materials")]
    [SerializeField] private Material m_MaterialFront = null;
    [SerializeField] private Material m_MaterialBack = null;
    [SerializeField] private Material m_MaterialTop = null;
    [SerializeField] private Material m_MaterialBottom = null;
    [SerializeField] private Material m_MaterialLeft = null;
    [SerializeField] private Material m_MaterialRight = null;


    [Header("Controls")]
    [SerializeField, Tooltip("Degree per pixel")] private float m_AngularSpeed = 20;
    [SerializeField, Tooltip("pixel before turning")] private float m_deltaThreshold = 20;


    [Header("Plane Detector")]
    [SerializeField] private OnMouseOverColor m_PlaneDetectorScript;
    private MouseOverNormal m_MouseOverNormal;



    private Vector2 m_PreviousPos = Vector2.zero;
    private float temp_angleofrotation = 0;
    private Vector3 temp_axis = Vector3.zero;

    void Start()
    {
        m_MouseOverNormal = new MouseOverNormal();
        if (m_CubePrefab == null || m_Camera == null)
            ErrorDetected("One or multiple field unset in RubikBehaviour");

        if (m_MaterialFront == null || m_MaterialBack == null ||
            m_MaterialTop == null || m_MaterialBottom == null ||
            m_MaterialLeft == null || m_MaterialRight == null)
            ErrorDetected("One or multiple texture missing in RubikBehaviour");
        Reload(m_RubikSize, 0);
    }

    private void Update()
    {
        RotateAllCtrl();
        m_MouseOverNormal.Update();
            RotateFaceCtrl(m_MouseOverNormal.normal);
    }

    void RotateAllCtrl()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            m_PreviousPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton((int)MouseButton.RightMouse))
        {
            Vector2 newPos = Input.mousePosition;
            if (newPos == m_PreviousPos)
                return;
            Vector2 dir = m_PreviousPos - newPos;

            float angleXAxis = -dir.y * m_AngularSpeed / 180f;
            float angleYAxis = dir.x * m_AngularSpeed / 180f;

            RotateAll(angleXAxis, angleYAxis);

            m_PreviousPos = newPos;
        }
    }

    void RotateAll(float _angleXAxis, float _angleYAxis)
    {
        Quaternion yAxis = Quaternion.AngleAxis(_angleYAxis, Vector3.up);
        Quaternion xAxis = Quaternion.AngleAxis(_angleXAxis, Vector3.right);

        transform.rotation = xAxis * yAxis * transform.rotation;
    }
    void RotateFaceCtrl(Vector3 _facenormal)
    {
        //On click
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            if (m_MouseOverNormal.normal == Vector3.zero)
                return;
            m_PreviousPos = Input.mousePosition;
            temp_axis = _facenormal;
        }
        //On hold
        else if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            Vector2 newPos = Input.mousePosition;
            if (newPos == m_PreviousPos)
                return;
            Vector2 dir = m_PreviousPos - newPos;
            if (dir.x < m_deltaThreshold && dir.y < m_deltaThreshold)
                return;
            float deltangle = dir.x* m_AngularSpeed / 180f;
            temp_angleofrotation += deltangle;
            //Precise here X or Y
            RotateFace(temp_axis, deltangle);

            //float angleXAxis = -dir.y * m_AngularSpeed / 180f;
            //float angleYAxis = dir.x * m_AngularSpeed / 180f;
            //RotateAll(angleXAxis, angleYAxis);
            m_PreviousPos = newPos;
        }
        //On release
        else if (temp_angleofrotation != 0f)
        {
            int moves = Mathf.RoundToInt(temp_angleofrotation / 90) % 4;//4 rotations = back to start
            RotateFace(temp_axis, -temp_angleofrotation +
                //Clip to nearest angle
                moves * 90);
            temp_angleofrotation = 0f;
            //Here add 1 more move to list
            //m_moves.add(new Move(moves,axis))
            temp_axis = Vector3.zero;
        }
    }
    void RotateFace(Vector3 _normal, float _angle)
    {
        foreach (GameObject cube in m_Cubes)
        {
            foreach (Transform face in cube.transform)
            {
                if (!face.CompareTag("ext"))
                    continue;
                else
                    //Care for approximation: could use a dotproduct instead
                    if (face.forward == -_normal)
                {
                    Vector3 oldposition = cube.transform.position;
                    Quaternion rotation = Quaternion.AngleAxis(_angle, _normal);
                    cube.transform.rotation = rotation * cube.transform.rotation;
                    Quaternion newposition = rotation * new Quaternion(oldposition.x, oldposition.y, oldposition.z, 0f) * Quaternion.Inverse(rotation);
                    cube.transform.position = new Vector3(newposition.x, newposition.y, newposition.z);
                }
            }
        }
    }

    void HandleMouseOver()
    {
        // This method will be called when a child face is moused over.

    }

    void ErrorDetected(string _error)
    {
        Debug.LogError(_error);
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }

    public void Reload(uint _newSize, uint _shuffles)
    {
        DestroyRubik();
        m_RubikSize = _newSize;
        m_Camera.GetComponent<CameraBehaviour>().SizeChanged(_newSize);
        //Rotate to see 3 faces
        CreateRubik();
        RotateAll(-30, 45);
    }

    void DestroyRubik()
    {
        if (m_Cubes.Length > 0)
            for (uint id = 0; id < m_Cubes.Length; id++)
                Destroy(m_Cubes[id]);

        m_Cubes = new GameObject[0];
        transform.rotation = Quaternion.identity;
    }

    void CreateRubik()
    {
        uint cubesInFace = m_RubikSize * m_RubikSize;
        uint cubesToMake = cubesInFace * m_RubikSize;
        m_Cubes = new GameObject[cubesToMake];

        float limit = .5f * (m_RubikSize - 1);
        float x = -limit, y = -limit, z = -limit;
        for (uint id = 0; id < cubesToMake; id++)
        {
            m_Cubes[id] = Instantiate(m_CubePrefab, transform);
            m_Cubes[id].name = "Cube_" + id;

            if ((id % cubesInFace) == 0) z++;
            if (z > limit) z = -limit;

            if ((id % m_RubikSize) == 0) y++;
            if (y > limit) y = -limit;

            x++;
            if (x > limit) x = -limit;

            m_Cubes[id].transform.position = new Vector3(x, y, z);

            Transform extFace;
            if (z == -limit) // front face
            {
                extFace = m_Cubes[id].transform.Find("Front");
                extFace.GetComponent<MeshRenderer>().material = m_MaterialFront;
                OnMouseOverColor script = extFace.gameObject.AddComponent(typeof(OnMouseOverColor)) as OnMouseOverColor;
                script.onMouseOverAction += HandleMouseOver;
                extFace.gameObject.tag = "ext";
            }
            else if (z == limit) // back face
            {
                extFace = m_Cubes[id].transform.Find("Back");
                extFace.GetComponent<MeshRenderer>().material = m_MaterialBack;
                OnMouseOverColor script = extFace.gameObject.AddComponent(typeof(OnMouseOverColor)) as OnMouseOverColor;
                script.onMouseOverAction += HandleMouseOver;
                extFace.gameObject.tag = "ext";
            }
            if (y == -limit) // bottom face
            {
                extFace = m_Cubes[id].transform.Find("Bottom");
                extFace.GetComponent<MeshRenderer>().material = m_MaterialBottom;
                OnMouseOverColor script = extFace.gameObject.AddComponent(typeof(OnMouseOverColor)) as OnMouseOverColor;
                script.onMouseOverAction += HandleMouseOver;
                extFace.gameObject.tag = "ext";
            }
            else if (y == limit) // top face
            {
                extFace = m_Cubes[id].transform.Find("Top");
                extFace.GetComponent<MeshRenderer>().material = m_MaterialTop;
                OnMouseOverColor script = extFace.gameObject.AddComponent(typeof(OnMouseOverColor)) as OnMouseOverColor;
                script.onMouseOverAction += HandleMouseOver;
                extFace.gameObject.tag = "ext";
            }
            if (x == -limit) // left face
            {
                extFace = m_Cubes[id].transform.Find("Left");
                extFace.GetComponent<MeshRenderer>().material = m_MaterialLeft;
                OnMouseOverColor script = extFace.gameObject.AddComponent(typeof(OnMouseOverColor)) as OnMouseOverColor;
                script.onMouseOverAction += HandleMouseOver;
                extFace.gameObject.tag = "ext";
            }
            else if (x == limit) // right face
            {
                extFace = m_Cubes[id].transform.Find("Right");
                extFace.GetComponent<MeshRenderer>().material = m_MaterialRight;
                OnMouseOverColor script = extFace.gameObject.AddComponent(typeof(OnMouseOverColor)) as OnMouseOverColor;
                script.onMouseOverAction += HandleMouseOver;
                extFace.gameObject.tag = "ext";
            }
        }
    }
}
