using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sizeText = null;
    [SerializeField] private Slider sizeSlider = null;
    [SerializeField] private Button sizeButton = null;

    [SerializeField] private GameObject rubiksCube = null;

    public void Start()
    {
        if (sizeText == null || sizeSlider == null || sizeButton == null || rubiksCube == null)
            ErrorDetected("One or multiple field unset in UIBehaviour");

        sizeSlider.onValueChanged.AddListener(delegate { SizeChanged(); });
        sizeButton.onClick.AddListener(delegate { SizeConfirmed(); });
    }

    void ErrorDetected(string error)
    {
        Debug.LogError(error);
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }

    void SizeChanged()
    {
        sizeText.text = "Cube Size : ";
        if (sizeSlider.value < 10) sizeText.text += "0" + sizeSlider.value.ToString();
        else sizeText.text += sizeSlider.value.ToString();
    }

    void SizeConfirmed()
    {
        rubiksCube.GetComponent<RubikBehaviour>().Reload((uint)sizeSlider.value);
    }
}
