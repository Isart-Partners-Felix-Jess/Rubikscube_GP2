using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sizeText = null;
    [SerializeField] private Slider sizeSlider = null;

    [SerializeField] private TextMeshProUGUI shuffleText = null;
    [SerializeField] private Slider shuffleSlider = null;

    [SerializeField] private Button confirmButton = null;

    [SerializeField] private Button instructionButton = null;
    [SerializeField] private GameObject instructionPanel = null;

    [SerializeField] private TextMeshProUGUI successText = null;

    [SerializeField] private GameObject rubiksCube = null;

    void Start()
    {
        if (sizeText == null || sizeSlider == null ||
            shuffleText == null || shuffleSlider == null ||
            confirmButton == null || successText == null || rubiksCube == null ||
            instructionPanel == null || instructionButton == null)
            ErrorDetected("One or multiple field unset in UIBehaviour");

        sizeSlider.onValueChanged.AddListener(delegate { SizeChanged(); });
        shuffleSlider.onValueChanged.AddListener(delegate { ShuffleChanged(); });

        confirmButton.onClick.AddListener(delegate { ConfirmedPressed(); });
        instructionButton.onClick.AddListener(delegate { InstructionPressed(); });
    }

    void ErrorDetected(string error)
    {
        Debug.LogError(error);
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }

    void ConfirmedPressed()
    {
        successText.gameObject.SetActive(false);
        rubiksCube.GetComponent<RubikBehaviour>().Reload((uint)sizeSlider.value, (uint)shuffleSlider.value);
    }

    void InstructionPressed()
    {
        Vector3 currentScale = instructionButton.transform.localScale;
        instructionButton.transform.localScale = new(currentScale.x, -currentScale.y, currentScale.z);
        instructionPanel.SetActive(!instructionPanel.activeInHierarchy);
    }

    void SizeChanged()
    {
        sizeText.text = "Cube Size : ";
        if (sizeSlider.value < 10) 
            sizeText.text += "0" + sizeSlider.value.ToString();
        else 
            sizeText.text += sizeSlider.value.ToString();
    }

    void ShuffleChanged()
    {
        shuffleText.text = "Shuffles : ";
        if (shuffleSlider.value < 100)
        {
            shuffleText.text += "0";
            if (shuffleSlider.value < 10) 
                shuffleText.text += "0" + shuffleSlider.value.ToString();
            else
                shuffleText.text += shuffleSlider.value.ToString();
        }
        else shuffleText.text += shuffleSlider.value.ToString();
    }

    void Successful()
    {
        successText.gameObject.SetActive(true);
    }
}
