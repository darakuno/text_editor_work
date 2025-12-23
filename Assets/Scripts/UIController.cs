using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class UIController : MonoBehaviour
{
    private const float SizeStep = 2.0f;

    private TMP_InputField inputField;
    private TMP_Dropdown fontDropdown;
    private TMP_Dropdown fileDropdown;
    private TMP_FontAsset[] availableFonts;
    private TMP_Text fileTitleLabel;
    private FileHandler fileHandler;

    public void Initialize(Editor editor)
    {
        this.inputField = editor.inputField;
        this.fileDropdown = editor.fileDropdown;
        this.availableFonts = editor.availableFonts;
        this.fileHandler = editor.FileHandler;

        if (this.fileHandler != null)
        {
            this.fileHandler.OnFilePathChanged += UpdateFileTitle;
        }

        StartCoroutine(InitializeUIHandlers());
        InitializeFileDropdown();
    }

    void OnDestroy()
    {
        if (fileHandler != null)
        {
            fileHandler.OnFilePathChanged -= UpdateFileTitle;
        }
        if (fileDropdown != null)
        {
            fileDropdown.onValueChanged.RemoveListener(OnFileActionSelected);
        }
    }

    private IEnumerator InitializeUIHandlers()
    {
        yield return new WaitForSecondsRealtime(0.05f);
    }

    private void InitializeFileDropdown()
    {
        if (fileDropdown == null) return;

        fileDropdown.ClearOptions();
        List<string> options = new List<string> {
            "Файл", "Открыть", "Сохранить", "Сохранить как..."
        };

        fileDropdown.AddOptions(options);
        fileDropdown.SetValueWithoutNotify(0);
        fileDropdown.onValueChanged.AddListener(OnFileActionSelected);
    }

    private void UpdateFileTitle(string fileName)
    {
        Debug.Log("Текущий файл: " + fileName);
    }

    private void OnFileActionSelected(int index)
    {
        fileDropdown.SetValueWithoutNotify(0);

        switch (index)
        {
            case 1: 
                fileHandler.LoadFile();
                break;
            case 2:
                fileHandler.SaveCurrentText(inputField.text);
                break;
            case 3: 
                fileHandler.SaveFileAs();
                break;
        }
    }

    public void IncreaseTextSize() => ApplySizeChange(true);
    public void DecreaseTextSize() => ApplySizeChange(false);

    private void ApplySizeChange(bool increase)
    {
        if (inputField == null) return;
        float currentSize = inputField.pointSize;
        float newSize = increase ? currentSize + SizeStep : currentSize - SizeStep;
        newSize = Mathf.Max(newSize, 5f);
        inputField.pointSize = Mathf.RoundToInt(newSize);
        inputField.ForceLabelUpdate();
    }
}