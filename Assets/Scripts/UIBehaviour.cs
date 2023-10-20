using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] private TextMeshProUGUI m_SizeText = null;
    [SerializeField] private Slider m_SizeSlider = null;

    [Header("Shuffle")]
    [SerializeField] private TextMeshProUGUI m_ShuffleText = null;
    [SerializeField] private Slider m_ShuffleSlider = null;

    [Header("Instruction")]
    [SerializeField] private Button m_InstructionButton = null;
    [SerializeField] private GameObject m_InstructionPanel = null;

    [Header("Other")]
    [SerializeField] private Button m_ConfirmButton = null;
    [SerializeField] private Button m_SolveButton = null;
    [SerializeField] private TextMeshProUGUI m_SuccessText = null;
    [SerializeField] private TextMeshProUGUI m_MovesLeftText = null;
    [SerializeField] private GameObject m_Cube = null;
    private RubikBehaviour m_RubikBehaviour;

    private void Start()
    {
        if (m_SizeText == null || m_SizeSlider == null ||
            m_ShuffleText == null || m_ShuffleSlider == null ||
            m_ConfirmButton == null || m_SuccessText == null ||
            m_SolveButton == null || m_MovesLeftText == null || m_Cube == null ||
            m_InstructionPanel == null || m_InstructionButton == null)
            ErrorDetected("One or multiple field unset in UIBehaviour");

        m_SizeSlider.onValueChanged.AddListener(delegate { SizeChanged(); });
        m_ShuffleSlider.onValueChanged.AddListener(delegate { ShuffleChanged(); });

        m_ConfirmButton.onClick.AddListener(delegate { ConfirmedPressed(); });
        m_SolveButton.onClick.AddListener(delegate { SolvePressed(); });

        m_InstructionButton.onClick.AddListener(delegate { InstructionPressed(); });

        m_RubikBehaviour = m_Cube.GetComponent<RubikBehaviour>();
        m_RubikBehaviour.MovesChanged += MovesChanged;
    }

    private void ErrorDetected(string _error)
    {
        Debug.LogError(_error);
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }

    private void ConfirmedPressed()
    {
        m_SuccessText.gameObject.SetActive(false);
        m_RubikBehaviour.Reload((uint)m_SizeSlider.value, (uint)m_ShuffleSlider.value);
    }

    private void SolvePressed()
    {
        m_SuccessText.gameObject.SetActive(false);
        m_RubikBehaviour.Solve();
    }

    private void InstructionPressed()
    {
        Vector3 currentScale = m_InstructionButton.transform.localScale;
        m_InstructionButton.transform.localScale = new(currentScale.x, -currentScale.y, currentScale.z);
        m_InstructionPanel.SetActive(!m_InstructionPanel.activeInHierarchy);
    }

    private void SizeChanged()
    {
        m_SizeText.text = "Cube Size : ";
        if (m_SizeSlider.value < 10)
            m_SizeText.text += "0" + m_SizeSlider.value.ToString();
        else
            m_SizeText.text += m_SizeSlider.value.ToString();
    }

    private void ShuffleChanged()
    {
        m_ShuffleText.text = "Shuffles : ";
        if (m_ShuffleSlider.value < 100)
        {
            m_ShuffleText.text += "0";
            if (m_ShuffleSlider.value < 10)
                m_ShuffleText.text += "0" + m_ShuffleSlider.value.ToString();
            else
                m_ShuffleText.text += m_ShuffleSlider.value.ToString();
        }
        else
            m_ShuffleText.text += m_ShuffleSlider.value.ToString();
    }

    private void MovesChanged()
    {
        int count = m_RubikBehaviour.Moves.Count;
        string str_count = m_RubikBehaviour.Moves.Count.ToString();

        m_MovesLeftText.text = "Moves Left : ";
        if (count < 100)
        {
            m_MovesLeftText.text += "0";
            if (count < 10)
                m_MovesLeftText.text += "0" + str_count;
            else
                m_MovesLeftText.text += str_count;
        }
        else
            m_MovesLeftText.text += str_count;

        if (count == 0)
            Successful();
        else
            m_SuccessText.gameObject.SetActive(false);
    }

    private void Successful()
    {
        m_SuccessText.gameObject.SetActive(true);
    }
}