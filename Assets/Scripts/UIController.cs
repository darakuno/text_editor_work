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
        this.fontDropdown = editor.fontDropdown;
        this.fileDropdown = editor.fileDropdown;
        this.availableFonts = editor.availableFonts;
        this.fileTitleLabel = editor.fileTitleLabel;
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
        if (fontDropdown != null)
        {
            fontDropdown.onValueChanged.RemoveListener(OnFontSelected);
        }
        if (fileDropdown != null)
        {
            fileDropdown.onValueChanged.RemoveListener(OnFileActionSelected);
        }
    }

    private IEnumerator InitializeUIHandlers()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        StartCoroutine(InitializeFontDropdownCoroutine());
    }

    private IEnumerator InitializeFontDropdownCoroutine()
    {
        if (fontDropdown == null || availableFonts == null || availableFonts.Length == 0) yield break;

        fontDropdown.ClearOptions();
        List<string> fontNames = new List<string>();

        yield return null;

        foreach (TMP_FontAsset fontAsset in availableFonts)
        {
            if (fontAsset != null)
            {
                fontNames.Add(fontAsset.name);
            }
        }

        fontDropdown.AddOptions(fontNames);
        fontDropdown.SetValueWithoutNotify(0);
        fontDropdown.onValueChanged.AddListener(OnFontSelected);

        if (availableFonts.Length > 0 && availableFonts[0] != null)
        {
            StartCoroutine(ApplyFontChangeDelayed(availableFonts[0]));
        }
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
        if (fileTitleLabel == null) return;
        fileTitleLabel.text = "Файл: " + fileName;
    }

    private void OnFontSelected(int index)
    {
        if (availableFonts == null || index < 0 || index >= availableFonts.Length || availableFonts[index] == null) return;
        StartCoroutine(ApplyFontChangeDelayed(availableFonts[index]));
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

    private IEnumerator ApplyFontChangeDelayed(TMP_FontAsset newFont)
    {
        yield return null;
        if (inputField != null)
        {
            inputField.fontAsset = newFont;
            inputField.ForceLabelUpdate();
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