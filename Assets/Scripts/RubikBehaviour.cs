using UnityEditor;
using UnityEngine;

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
    [SerializeField] private float m_AngularSpeed = 1;


    private Vector2 m_PreviousPos = Vector2.zero;

    void Start()
    {
        if (m_CubePrefab == null || m_Camera == null)
            ErrorDetected("One or multiple field unset in RubikBehaviour");

        if (m_MaterialFront == null || m_MaterialBack == null || 
            m_MaterialTop == null || m_MaterialBottom == null || 
            m_MaterialLeft == null || m_MaterialRight == null)
            ErrorDetected("One or multiple texture missing in RubikBehaviour");
        Reload(m_RubikSize,0);
    }

    private void Update()
    {
        RotateAll();
    }

    void RotateAll()
    {
        if (Input.GetMouseButtonDown(1))
        {
            m_PreviousPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 newPos = Input.mousePosition;
            if (newPos == m_PreviousPos)
                return;
            Vector2 dir = m_PreviousPos - newPos;

            float angleXAxis = -dir.y * m_AngularSpeed;
            float angleYAxis = dir.x * m_AngularSpeed;


            Quaternion xAxis = Quaternion.AngleAxis(angleXAxis /2f, Vector3.right);
            Quaternion yAxis = Quaternion.AngleAxis(angleYAxis /2f, Vector3.up);

            transform.rotation = yAxis * xAxis * transform.rotation;


            m_PreviousPos = newPos;
        }
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
        CreateRubik();
    }

    void DestroyRubik()
    {
        if (m_Cubes.Length > 0)
            for (uint id = 0; id < m_Cubes.Length; id++)
                Destroy(m_Cubes[id]);

        m_Cubes = new GameObject[0];
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

            if (z == -limit) // front face
                m_Cubes[id].transform.Find("Front").GetComponent<MeshRenderer>().material= m_MaterialFront;
            else if (z == limit) // back face
                m_Cubes[id].transform.Find("Back").GetComponent<MeshRenderer>().material = m_MaterialBack;

            if (y == -limit) // bottom face
                m_Cubes[id].transform.Find("Bottom").GetComponent<MeshRenderer>().material = m_MaterialBottom;
            else if (y == limit) // top face
                m_Cubes[id].transform.Find("Top").GetComponent<MeshRenderer>().material = m_MaterialTop;

            if (x == -limit) // left face
                m_Cubes[id].transform.Find("Left").GetComponent<MeshRenderer>().material = m_MaterialLeft;
            else if (x == limit) // right face
                m_Cubes[id].transform.Find("Right").GetComponent<MeshRenderer>().material = m_MaterialRight;
        }
    }
}
