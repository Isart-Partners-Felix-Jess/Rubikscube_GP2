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
    [SerializeField] private TextMeshProUGUI m_SuccessText = null;
    [SerializeField] private GameObject m_Cube = null;

    void Start()
    {
        if (m_SizeText == null || m_SizeSlider == null ||
            m_ShuffleText == null || m_ShuffleSlider == null ||
            m_ConfirmButton == null || m_SuccessText == null || m_Cube == null ||
            m_InstructionPanel == null || m_InstructionButton == null)
            ErrorDetected("One or multiple field unset in UIBehaviour");

        m_SizeSlider.onValueChanged.AddListener(delegate { SizeChanged(); });
        m_ShuffleSlider.onValueChanged.AddListener(delegate { ShuffleChanged(); });

        m_ConfirmButton.onClick.AddListener(delegate { ConfirmedPressed(); });
        m_InstructionButton.onClick.AddListener(delegate { InstructionPressed(); });
    }

    void ErrorDetected(string _error)
    {
        Debug.LogError(_error);
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }

    void ConfirmedPressed()
    {
        m_SuccessText.gameObject.SetActive(false);
        m_Cube.GetComponent<RubikBehaviour>().Reload((uint)m_SizeSlider.value, (uint)m_ShuffleSlider.value);
    }

    void InstructionPressed()
    {
        Vector3 currentScale = m_InstructionButton.transform.localScale;
        m_InstructionButton.transform.localScale = new(currentScale.x, -currentScale.y, currentScale.z);
        m_InstructionPanel.SetActive(!m_InstructionPanel.activeInHierarchy);
    }

    void SizeChanged()
    {
        m_SizeText.text = "Cube Size : ";
        if (m_SizeSlider.value < 10) 
            m_SizeText.text += "0" + m_SizeSlider.value.ToString();
        else 
            m_SizeText.text += m_SizeSlider.value.ToString();
    }

    void ShuffleChanged()
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
        else m_ShuffleText.text += m_ShuffleSlider.value.ToString();
    }

    void Successful()
    {
        m_SuccessText.gameObject.SetActive(true);
    }
}
