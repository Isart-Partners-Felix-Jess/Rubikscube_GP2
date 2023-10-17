using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Linq;
using System.Runtime.CompilerServices;
using static UnityEngine.Random;
using System.Globalization;

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



    private GameObject m_SelectedFace = null;
    private GameObject m_SelectedCube = null;
    private List<GameObject> m_SelectedGroupCubes;

    private bool m_AxisDecided = false;
    private Vector2 m_PreviousPos = Vector2.zero;
    private Vector3 temp_axis = Vector3.zero;
    private float temp_angleofrotation = 0;

    private List<MoveClass> m_Moves;


    private void Start()
    {
        m_SelectedGroupCubes = new List<GameObject>();
        m_Moves = new List<MoveClass>();
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
        // Check if the mouse pointer is over a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Mouse is over a UI element, so ignore clicks
            return;
        }

        m_MouseOverNormal.Update();
        if (m_MouseOverNormal.selectedObject != null)
            RotateFaceCtrl(m_MouseOverNormal.normal);
        RotateAllCtrl();
    }

    private void RotateAllCtrl()
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

    private void RotateAll(float _angleXAxis, float _angleYAxis)
    {
        Quaternion yAxis = Quaternion.AngleAxis(_angleYAxis, Vector3.up);
        Quaternion xAxis = Quaternion.AngleAxis(_angleXAxis, Vector3.right);

        transform.rotation = xAxis * yAxis * transform.rotation;
    }

    private Vector3 SelectAxis(Vector2 _dir)
    {
        //Rotate mouse axis
        Vector3 diroriented = m_SelectedFace.transform.rotation * _dir;// * Quaternion.Inverse(m_SelectedFace.transform.rotation);
        //float up = Vector3.Dot(_dir, m_SelectedFace.transform.up);
        //float right = Vector3.Dot(_dir, m_SelectedFace.transform.right);
        //float forward = Vector3.Dot(_dir, m_SelectedFace.transform.forward);
        //
        //float max = Mathf.Max(Mathf.Abs(right), Mathf.Abs(forward));


        //if (Mathf.Abs(up) >= max)
        //{
        //SelectFace(0);
        //return m_SelectedFace.transform.right;
        //}
        //else
        //{
        //if (Mathf.Abs(right) >= max)
        //SelectFace(1);
        //else if (Mathf.Abs(forward) >= max)
        //SelectFace(2);
        //return transform.up;
        //}
        //else if (max == Mathf.Abs(forward))
        //{
        //    SelectFace(2);
        //    return m_SelectedFace.transform.forward;
        //}

        float up = Vector3.Angle(diroriented, m_SelectedFace.transform.up);
        float right = Vector3.Angle(diroriented, m_SelectedFace.transform.right);
        //float forward = Vector3.Angle(_dir, m_SelectedFace.transform.forward);

        float min = Mathf.Min(Mathf.Abs(up), Mathf.Abs(right)/*, Mathf.Abs(forward)*/);
        //up = Vector3.Angle(m_SelectedFace.transform.up, Camera.current.transform.up);
        if (Mathf.Abs(up) == min)
        {
            SelectFace(1);
            return m_SelectedFace.transform.up;
        }
        if (Mathf.Abs(right) == min)
        {
            SelectFace(0);
            return m_SelectedFace.transform.right;
        }
        return Vector3.zero;
    }
    //index 0<->x, 1<->y, 2<->z 
    private void SelectFace(uint _axis)
    {
        if (_axis == 0)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.x == m_SelectedCube.transform.localPosition.x)
                    m_SelectedGroupCubes.Add(cube);
        }
        if (_axis == 1)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.y == m_SelectedCube.transform.localPosition.y)
                    m_SelectedGroupCubes.Add(cube);
        }
        if (_axis == 2)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.z == m_SelectedCube.transform.localPosition.z)
                    m_SelectedGroupCubes.Add(cube);
        }
    }
    private void SelectFaceNumber(uint _axis, uint _index)
    {
        float pos = _index - (m_RubikSize - 1) * 0.5f;
        if (_axis == 0)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.x == pos)
                    m_SelectedGroupCubes.Add(cube);
        }
        if (_axis == 1)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.y == pos)
                    m_SelectedGroupCubes.Add(cube);
        }
        if (_axis == 2)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.z == pos)
                    m_SelectedGroupCubes.Add(cube);
        }
    }
    private void RotateFaceCtrl(Vector3 _facenormal)
    {
        //On click
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            if (m_MouseOverNormal.selectedObject == null)
                return;
            //SelectedObject is a face, so we take its parent to get the Cube
            m_SelectedFace = m_MouseOverNormal.selectedObject;
            m_SelectedCube = m_MouseOverNormal.selectedObject.transform.parent.gameObject;
            m_PreviousPos = Input.mousePosition;
            //temp_axis = _facenormal;
        }
        //On hold
        else if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            //if clicked outside
            if (m_SelectedCube == null)
                return;
            Vector2 newPos = Input.mousePosition;
            if (newPos == m_PreviousPos)
                return;
            Vector2 dir = m_PreviousPos - newPos;
            if (!m_AxisDecided)
            {
                //test over squared distance
                if (Vector2.SqrMagnitude(dir) < m_deltaThreshold * m_deltaThreshold)
                    return;
                temp_axis = SelectAxis(dir);
                m_AxisDecided = true;
            }
            float deltangle = Vector3.Dot(dir, temp_axis) * m_AngularSpeed / 180f;
            temp_angleofrotation += deltangle;

            //Precise here X or Y
            RotateFace(temp_axis, deltangle);

            //float angleXAxis = -dir.y * m_AngularSpeed / 180f;
            //float angleYAxis = dir.x * m_AngularSpeed / 180f;
            //RotateAll(angleXAxis, angleYAxis);
            m_PreviousPos = newPos;
        }
        //On release
        else
        {
            if (temp_angleofrotation != 0f)
            {
                int moves = Mathf.RoundToInt(temp_angleofrotation / 90) % 4;//4 rotations = back to start
                RotateFace(temp_axis, -temp_angleofrotation //reset rotation
                                                            //Clip to nearest angle
                    + moves * 90);
                RoundFacePositions((m_RubikSize % 2) == 0);
                //Here add 1 more move to list
                //m_moves.add(new Move(moves,axis))
            }
            //Reset Variables
            temp_angleofrotation = 0f;
            temp_axis = Vector3.zero;
            m_SelectedFace = null;
            m_SelectedCube = null;
            m_AxisDecided = false;
            m_SelectedGroupCubes.Clear();
        }
    }
    private void RotateFaceAroundNormal(Vector3 _normal, float _angle)
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
    private void RotateFace(Vector3 _axis, float _angle)
    {
        foreach (GameObject cube in m_SelectedGroupCubes)
        {
            Vector3 oldposition = cube.transform.position;
            Quaternion rotation = Quaternion.AngleAxis(_angle, _axis);
            cube.transform.rotation = rotation * cube.transform.rotation;
            Quaternion newposition = rotation * new Quaternion(oldposition.x, oldposition.y, oldposition.z, 0f) * Quaternion.Inverse(rotation);
            cube.transform.position = new Vector3(newposition.x, newposition.y, newposition.z);
        }
    }
    private void RotateFace(uint _axis, uint _index, int _number)
    {
        SelectFaceNumber(_axis, _index);
        Vector3 axis;
        switch (_axis)
        {
            case 0:
                axis = Vector3.right;
                break;
            case 1:
                axis = Vector3.up;
                break;
            case 2:
                axis = Vector3.forward;
                break;
            default:
                return;
        }
        RotateFace(axis, _number * 90);
        RoundFacePositions((m_RubikSize % 2) == 0);
        m_SelectedGroupCubes.Clear();
    }
    private void RoundFacePositions(bool _evenNumberOfFaces)
    {
        foreach (GameObject cube in m_SelectedGroupCubes)
        {
            Vector3 oldposition = cube.transform.localPosition;
            if (_evenNumberOfFaces)
                oldposition -= new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 newposition = new Vector3(Mathf.RoundToInt(oldposition.x), Mathf.RoundToInt(oldposition.y), Mathf.RoundToInt(oldposition.z));
            if (_evenNumberOfFaces)
                newposition += new Vector3(0.5f, 0.5f, 0.5f);
            cube.transform.localPosition = newposition;
        }
    }
    private void AddMove(uint _axis, uint _index, int _number)
    {
        if (m_Moves.Count != 0 && m_Moves.Last().axis == _axis && m_Moves.Last().index == _index)
        {
            m_Moves.Last().number = (m_Moves.Last().number + _number) % 4;
            if (m_Moves.Last().number == 0)
                m_Moves.RemoveAt(m_Moves.Count - 1);
        }
        else
            m_Moves.Add(new MoveClass(_axis, _index, _number));
    }

    private void HandleMouseOver()
    {
        // This method will be called when a child face is moused over.

    }

    private void ErrorDetected(string _error)
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
        Shuffle(_shuffles);
        RotateAll(-30, 45);
    }

    private void DestroyRubik()
    {
        if (m_Cubes.Length > 0)
            for (uint id = 0; id < m_Cubes.Length; id++)
                Destroy(m_Cubes[id]);

        m_Cubes = new GameObject[0];
        transform.rotation = Quaternion.identity;
        m_Moves.Clear();
    }

    private void CreateRubik()
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

    private void Shuffle(uint _number)
    {
        
        while( m_Moves.Count() < _number) 
        {
            uint axis = (uint)UnityEngine.Random.Range(0, 2);
            uint index = (uint)UnityEngine.Random.Range(0, m_RubikSize-1);
            int number = UnityEngine.Random.Range(1, 3);
            RotateFace(axis, index, number);
            AddMove(axis, index, number);
        }
    }
    private void Solve()
    {

        while (m_Moves.Count() > 0)
        {
            uint axis = m_Moves.Last().axis;
            uint index = m_Moves.Last().index;
            int number = -m_Moves.Last().number;
            RotateFace(axis, index, number);
            AddMove(axis, index, number);
        }
    }
}

