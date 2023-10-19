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
using System.Collections;
using static UnityEditor.PlayerSettings;
using System.Reflection;
using System.Drawing;

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
    [SerializeField, Tooltip("Pixels before turning")] private float m_deltaThreshold = 20;
    private bool m_BlockedCtrls = false;

    [Header("Plane Detector")]
    [SerializeField] private OnMouseOverColor m_PlaneDetectorScript;
    private MouseOverNormal m_MouseOverNormal;

    [Header("Auto controls")]
    [SerializeField, Tooltip("In seconds")] private float m_TimePerMoveToSolve = 0.5f;
    [SerializeField, Tooltip("In seconds")] private float m_TimePerMoveToShuffle = 0.25f;

    private GameObject m_SelectedFace = null;
    private GameObject m_SelectedCube = null;
    private List<GameObject> m_SelectedGroupCubes;

    private bool m_AxisDecided = false;
    private Vector2 m_PreviousPos = Vector2.zero;

    //Temp Move
    bool m_MouseXoverY;
    private int temp_axis;
    private int temp_index;
    private float temp_angleofrotation = 0;

    public event Action MovesChanged;
    //Setter
    private List<MoveClass> m_Moves;
    //UI Getter
    public List<MoveClass> moves
    {
        get { return m_Moves; }
    }

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

        RotateFaceCtrl();
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

    private int SelectAxis(bool _XoverY)
    {
        float tolerance = 0.00001f;
        Vector3 axis;
        if (_XoverY)
        {
            //if(transform.right == m_SelectedFace.transform.forward)
            //    axis = Vector3.Cross(transform.right, m_SelectedFace.transform.forward);
            //else if (transform.right == -m_SelectedFace.transform.forward)
            //    axis = Vector3.Cross(transform.right, m_SelectedFace.transform.forward);
            //axis = Vector3.Cross(transform.right, m_SelectedFace.transform.forward);
            axis = m_SelectedFace.transform.up;
        }
        else
            //axis = Vector3.Cross(transform.up, m_SelectedFace.transform.forward);
            axis = m_SelectedFace.transform.right;

        //Bottom
        if (Mathf.Abs(Vector3.Dot(axis, transform.up)) < tolerance)
        {
            temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.y);
            SelectFace(1);
            return temp_axis = 1;
        }
        //Up
        if (Mathf.Abs(Vector3.Dot(axis, -transform.up)) < tolerance)
        {
            temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.y);
            SelectFace(1);
            return temp_axis = -1;
        }
        //Left
        if (Mathf.Abs(Vector3.Dot(axis, transform.right)) < tolerance)
        {
            temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.x);
            SelectFace(0);
            return temp_axis = 0;
        }
        //Right
        if (Mathf.Abs(Vector3.Dot(axis, -transform.right)) < tolerance)
        {
            temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.x);
            SelectFace(0);
            return temp_axis = -0;
        }
        //Bottom
        if (Mathf.Abs(Vector3.Dot(axis, transform.forward)) < tolerance)
        {
            temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.z);
            SelectFace(2);
            return temp_axis = 2;
        }
        //Up
        if (Mathf.Abs(Vector3.Dot(axis, -transform.forward)) < tolerance)
        {
            temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.z);
            SelectFace(2);
            return temp_axis = -2;
        }



        //Rotate mouse axis
        //Vector3 diroriented = m_SelectedFace.transform.rotation * _dir;// * Quaternion.Inverse(m_SelectedFace.transform.rotation);
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

        //Front Face
        if (m_SelectedFace.transform.forward == transform.forward)
            if (_XoverY)
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.y);
                SelectFace(1);
                return temp_axis = 1;
                //return transform.up; 
            }
            else
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.x);
                SelectFace(0);
                return temp_axis = 0;
                //return transform.right;
            }
        //Back Face
        if (m_SelectedFace.transform.forward == -transform.forward)
            if (_XoverY)
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.y);
                SelectFace(1);
                return temp_axis = -1;
                //return -transform.up;
            }
            else
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.x);
                SelectFace(0);
                return temp_axis = -0; //Could be a problem
                                       //return -transform.right;
            };
        //Up Face
        if (m_SelectedFace.transform.forward == -transform.up)
            if (_XoverY)
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.z);
                SelectFace(2);
                return temp_axis = 2;
                //return transform.forward;
            }
            else
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.x);
                SelectFace(0);
                return temp_axis = 0;
                //return transform.right;
            }
        //Bottom Face
        if (m_SelectedFace.transform.forward == transform.up)
            if (_XoverY)
                if (_XoverY)
                {
                    temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.z);
                    SelectFace(2);
                    return temp_axis = -2;
                    //return -transform.forward;
                }
                else
                {
                    temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.x);
                    SelectFace(0);
                    return temp_axis = 0;
                    //return transform.right;
                }
        //Left Face
        if (m_SelectedFace.transform.forward == transform.right)
            if (_XoverY)
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.y);
                SelectFace(1);
                return temp_axis = 1;
                //return transform.up; 
            }
            else
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.z);
                SelectFace(2);
                return temp_axis = -2;
                //return -transform.forward;
            }
        //Left Face
        if (m_SelectedFace.transform.forward == -transform.right)
            if (_XoverY)
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.y);
                SelectFace(1);
                return temp_axis = 1;
                //return transform.up;
            }
            else
            {
                temp_index = PositionToIndex(m_SelectedCube.transform.localPosition.z);
                SelectFace(2);
                return temp_axis = 2;
                //return transform.forward;
            }

        //float up = Vector3.Angle(diroriented, m_SelectedFace.transform.up);
        //float right = Vector3.Angle(diroriented, m_SelectedFace.transform.right);
        ////float forward = Vector3.Angle(_dir, m_SelectedFace.transform.forward);

        //float min = Mathf.Min(Mathf.Abs(up), Mathf.Abs(right)/*, Mathf.Abs(forward)*/);
        ////up = Vector3.Angle(m_SelectedFace.transform.up, Camera.current.transform.up);
        //if (Mathf.Abs(up) == min)
        //{
        //    SelectFace(1);
        //    return m_SelectedFace.transform.up;
        //}
        //if (Mathf.Abs(right) == min)
        //{
        //    SelectFace(0);
        //    return m_SelectedFace.transform.right;
        //}
        m_SelectedGroupCubes.Clear();
        return 0;
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
        else if (_axis == 1)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.y == m_SelectedCube.transform.localPosition.y)
                    m_SelectedGroupCubes.Add(cube);
        }
        else if (_axis == 2)
        {
            foreach (GameObject cube in m_Cubes)
                if (cube.transform.localPosition.z == m_SelectedCube.transform.localPosition.z)
                    m_SelectedGroupCubes.Add(cube);
        }
    }
    private int PositionToIndex(float _pos)
    {
        return Mathf.RoundToInt(_pos + (m_RubikSize - 1) * 0.5f);

    }
    private float IndexToPosition(uint _index)
    {
        return _index - (m_RubikSize - 1) * 0.5f;
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
    private void RotateFaceCtrl()
    {
        //On click
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            m_MouseOverNormal.Update();
            if (m_MouseOverNormal.selectedObject == null || m_BlockedCtrls)
                return;
            //SelectedObject is a face, so we take its parent to get the Cube
            m_SelectedFace = m_MouseOverNormal.selectedObject;
            m_SelectedCube = m_MouseOverNormal.selectedObject.transform.parent.gameObject;
            m_PreviousPos = Input.mousePosition;
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
            Vector2 mouseDir = m_PreviousPos - newPos;

            //Shortcut by Unity
            if (!m_AxisDecided)
            {
                //test over squared distance
                //if (Mathf.Abs(mouseDir.y) > m_deltaThreshold || Mathf.Abs(mouseDir.y) > m_deltaThreshold)
                if (Vector3.SqrMagnitude(mouseDir) > m_deltaThreshold * m_deltaThreshold)
                {
                    //Shortcut unity for Qr * Qimpur(mousdir,0,0) * Qrbar
                    mouseDir = Quaternion.Inverse(m_SelectedCube.transform.rotation) * mouseDir;
                    if (Mathf.Abs(mouseDir.x) >= Mathf.Abs(mouseDir.y))
                        m_MouseXoverY = false;
                    else
                        m_MouseXoverY = true;
                }
                else
                    return;
                temp_axis = SelectAxis(!m_MouseXoverY);
                m_AxisDecided = true;
            }
            float deltangle = /*(temp_axis < +0 ? -1 : 1) */ (m_MouseXoverY ? mouseDir.y : mouseDir.x) * m_AngularSpeed / 90;
            temp_angleofrotation += deltangle;

            //Precise here X or Y
            RotateFace((uint)Mathf.Abs(temp_axis), (uint)temp_index, deltangle);

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
                int moves = Mathf.RoundToInt(temp_angleofrotation / 90f) % 4;//4 rotations = back to start
                RotateFace((uint)Mathf.Abs(temp_axis), (uint)temp_index, -temp_angleofrotation //reset rotation
                                                                         + moves * 90f);       //Clip to nearest angle

                //RotateFace(temp_axis, -temp_angleofrotation //reset rotation
                //                        + moves * 90);     //Clip to nearest angle

                RoundFacePositions((m_RubikSize % 2) == 0);
                //Here add 1 more move to list
                if (moves != 0)
                    AddMove((uint)Mathf.Abs(temp_axis), (uint)temp_index, moves);
            }
            //Reset Variables
            temp_angleofrotation = 0f;
            temp_axis = 0;
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
                    Quaternion currentRotation = Quaternion.AngleAxis(_angle, _normal);
                    cube.transform.rotation = currentRotation * cube.transform.rotation;
                    Quaternion newposition = currentRotation * new Quaternion(oldposition.x, oldposition.y, oldposition.z, 0f) * Quaternion.Inverse(currentRotation);
                    cube.transform.position = new Vector3(newposition.x, newposition.y, newposition.z);
                }
            }
        }
    }
    private void RotateFace(Vector3 _axis, float _angle)
    {
        foreach (GameObject cube in m_SelectedGroupCubes)
        {
            //Vector3 pivot = cube.transform.position;
            //Vector3 relativePosition = cube.transform.position - pivot;

            //Quaternion rotation = Quaternion.AngleAxis(_angle, _axis);

            //// Apply the rotation to the relative position
            //Vector3 newPosition = rotation * relativePosition;

            //// Add the pivot point back to the new position
            //newPosition += pivot;

            //// Apply the new position
            //cube.transform.position = newPosition;

            //// Apply the rotation
            //cube.transform.rotation = rotation * cube.transform.rotation;


            Vector3 oldposition = cube.transform.position;
            Quaternion currentRotation = Quaternion.AngleAxis(_angle, _axis);
            Quaternion newposition = currentRotation * new Quaternion(oldposition.x, oldposition.y, oldposition.z, 0f) * Quaternion.Inverse(currentRotation);
            cube.transform.position = new Vector3(newposition.x, newposition.y, newposition.z);
            cube.transform.rotation = currentRotation * cube.transform.rotation;
        }
    }
    private void RotateFace(uint _axis, uint _index, float _angle)
    {
        Vector3 axis;
        switch (_axis)
        {
            case 0:
                axis = transform.right;
                break;
            case 1:
                axis = transform.up;
                break;
            case 2:
                axis = transform.forward;
                break;
            default:
                return;
        }
        RotateFace(axis, _angle);
    }
    //Rotate a fixed number of 90deg rotations
    private void RotateFace(uint _axis, uint _index, int _number)
    {
        SelectFaceNumber(_axis, _index);
        RotateFace(_axis, _index, (float)_number * 90f);
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
            {
                m_Moves.RemoveAt(m_Moves.Count - 1);
            }
        }
        else
        {
            m_Moves.Add(new MoveClass(_axis, _index, _number));
        }
        CyclicRemove();
        MovesChanged?.Invoke();
    }
    private void CyclicRemove()
    {
        if (m_Moves.Count < 3)
            return;
        MoveClass last = m_Moves.Last();
        MoveClass beforeLast = m_Moves[m_Moves.Count - 2];
        MoveClass thirdLast = m_Moves[m_Moves.Count - 3];
        if (
        (last.number == beforeLast.number) &&       //same number
        (last.number == thirdLast.number) &&        //same number
        (last.axis == beforeLast.axis) &&           //same axis
        (last.axis == thirdLast.axis) &&            //same axis
        (last.index != beforeLast.index) &&         //different indexes
        (last.index != thirdLast.index) &&          //different indexes
        (beforeLast.index != thirdLast.index))      //different indexes
        { m_Moves.RemoveRange(m_Moves.Count - 3, 3); } //Erase 3 last
        else return;
        CyclicRemove();
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
        StopAllCoroutines();
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
        StartCoroutine(ShuffleWithDelay(_number));
    }
    private IEnumerator ShuffleWithDelay(uint _number)
    {
        while (m_Moves.Count() < _number)
        {
            uint axis = (uint)UnityEngine.Random.Range(0, 2);
            uint index = (uint)UnityEngine.Random.Range(0, m_RubikSize - 1);
            int number = UnityEngine.Random.Range(1, 3);
            RotateFace(axis, index, number);
            AddMove(axis, index, number);
            m_BlockedCtrls = true;
            yield return new WaitForSeconds(m_TimePerMoveToShuffle); // Adjust the time as needed
        }
        m_BlockedCtrls = false;
    }
    public void Solve()
    {
        StartCoroutine(SolveWithDelay());
    }

    private IEnumerator SolveWithDelay()
    {
        while (m_Moves.Count() > 0)
        {
            uint axis = m_Moves.Last().axis;
            uint index = m_Moves.Last().index;
            int number = -m_Moves.Last().number;

            // Rotate the face
            RotateFace(axis, index, number);
            // Add the move
            AddMove(axis, index, number);
            m_BlockedCtrls = true;
            // Wait for a specified amount of time (e.g., 1 second)
            yield return new WaitForSeconds(m_TimePerMoveToSolve); // Adjust the time as needed
        }
        m_BlockedCtrls = false;
    }
}

