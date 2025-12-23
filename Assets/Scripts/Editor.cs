using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.InputSystem;
using System.Collections;

public class Editor : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_FontAsset[] availableFonts;
    public TMP_Dropdown fontDropdown;
    public TMP_Dropdown fileDropdown;
    public TMP_Text fileTitleLabel;
    public TMP_Text statsLabel;

    [HideInInspector] public EditorActions editorActions;
    private UIController uiController;
    private InputHandler inputHandler;
    private FileHandler fileHandler;

    public FileHandler FileHandler => fileHandler;
    public UIController UIController => uiController;

    public static Editor Instance { get; private set; }
    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    IEnumerator Start()
    {
        yield return null;

        editorActions = new EditorActions();
        fileHandler = new FileHandler(this);

        uiController = gameObject.AddComponent<UIController>();
        uiController.Initialize(this);

        inputHandler = gameObject.AddComponent<InputHandler>();
        inputHandler.Initialize(this);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        editorActions?.Dispose();
    }

    public void IncreaseTextSize() => uiController?.IncreaseTextSize();
    public void DecreaseTextSize() => uiController?.DecreaseTextSize();
    public void SaveText() => fileHandler?.SaveCurrentText(inputField.text);
}