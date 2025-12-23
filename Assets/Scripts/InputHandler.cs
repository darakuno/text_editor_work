using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class InputHandler : MonoBehaviour
{
    private TMP_InputField inputField;
    private EditorActions editorActions;

    private int caretPosition;
    private bool isDebouncing = false;
    private const float DebounceDuration = 0.1f;
    private System.Action<InputAction.CallbackContext> formatBoldDelegate;
    private System.Action<InputAction.CallbackContext> formatItalicDelegate;

    public void Initialize(Editor editor)
    {
        this.inputField = editor.inputField;
        this.editorActions = editor.editorActions;

        formatBoldDelegate = ctx => OnFormatFormatAction(ctx, "<b>", "</b>");
        formatItalicDelegate = ctx => OnFormatFormatAction(ctx, "<i>", "</i>");

        var uiEditorMap = editorActions.UIEditor;

        uiEditorMap.FormatBold.performed += formatBoldDelegate;
        uiEditorMap.FormatItalic.performed += formatItalicDelegate;
        uiEditorMap.InsertTab.performed += OnInsertTab;
        uiEditorMap.Enable();

        inputField.onSelect.AddListener(OnInputFieldFocus);
        inputField.onDeselect.AddListener(OnInputFieldLostFocus);
    }

    void OnDestroy()
    {
        if (editorActions != null)
        {
            var uiEditorMap = editorActions.UIEditor;

            uiEditorMap.FormatBold.performed -= formatBoldDelegate;
            uiEditorMap.FormatItalic.performed -= formatItalicDelegate;
            uiEditorMap.InsertTab.performed -= OnInsertTab;

            uiEditorMap.Disable();
            editorActions.Dispose();
            editorActions = null;
        }

        if (inputField != null)
        {
            inputField.onSelect.RemoveListener(OnInputFieldFocus);
            inputField.onDeselect.RemoveListener(OnInputFieldLostFocus);
        }
    }

    void Update()
    {
        if (inputField != null && inputField.isFocused)
        {
            caretPosition = inputField.caretPosition;
        }
    }

    private void OnInputFieldFocus(string text) { }
    private void OnInputFieldLostFocus(string text) { }

    private void OnInsertTab(InputAction.CallbackContext context)
    {
        if (inputField != null && inputField.isFocused)
        {
            context.ReadValueAsObject();
            string tabSpace = "    ";
            inputField.text = inputField.text.Insert(caretPosition, tabSpace);
            inputField.caretPosition = caretPosition + tabSpace.Length;
        }
    }

    private void OnFormatFormatAction(InputAction.CallbackContext context, string startTag, string endTag)
    {
        if (isDebouncing) return;
        context.ReadValueAsObject();
        if (inputField != null && inputField.isFocused)
            ApplyFormatting(startTag, endTag);
    }

    private void ApplyFormatting(string startTag, string endTag)
    {
        if (inputField == null) return;

        if (string.IsNullOrEmpty(inputField.text))
        {
            inputField.text = startTag + endTag;
            inputField.caretPosition = startTag.Length;
            return;
        }

        int anchor = inputField.selectionAnchorPosition;
        int focus = inputField.selectionFocusPosition;
        int startIndex = Mathf.Min(anchor, focus);
        int endIndex = Mathf.Max(anchor, focus);
        int selectionLength = endIndex - startIndex;
        if (selectionLength > 0)
        {
            bool isEnclosingTags = startIndex > 0 &&
                                   (startIndex - startTag.Length) >= 0 &&
                                   inputField.text.Length >= (endIndex + endTag.Length) &&
                                   inputField.text.Substring(startIndex - startTag.Length, startTag.Length) == startTag &&
                                   inputField.text.Substring(endIndex, endTag.Length) == endTag;

            if (isEnclosingTags)
            {
                inputField.text = inputField.text.Remove(endIndex, endTag.Length);
                inputField.text = inputField.text.Remove(startIndex - startTag.Length, startTag.Length);

                inputField.Select();
                inputField.selectionAnchorPosition = startIndex - startTag.Length;
                inputField.selectionFocusPosition = endIndex - startTag.Length;
            }
            else
            {
                string selectedText = inputField.text.Substring(startIndex, selectionLength);
                string replacementText = startTag + selectedText + endTag;

                inputField.text = inputField.text.Remove(startIndex, selectionLength).Insert(startIndex, replacementText);

                inputField.Select();
                inputField.selectionAnchorPosition = startIndex;
                inputField.selectionFocusPosition = startIndex + replacementText.Length;
            }
        }
        else
        {
            bool hasTagsAroundCaret = caretPosition >= startTag.Length &&
                                      (caretPosition + endTag.Length) <= inputField.text.Length &&
                                      inputField.text.Substring(caretPosition - startTag.Length, startTag.Length) == startTag &&
                                      inputField.text.Substring(caretPosition, endTag.Length) == endTag;

            if (hasTagsAroundCaret)
            {
                inputField.text = inputField.text.Remove(caretPosition, endTag.Length);
                inputField.text = inputField.text.Remove(caretPosition - startTag.Length, startTag.Length);

                StartCoroutine(DebounceCoroutine(DebounceDuration));

                inputField.caretPosition = caretPosition - startTag.Length;
            }
            else
            {
                inputField.text = inputField.text.Insert(caretPosition, startTag + endTag);
                inputField.caretPosition = caretPosition + startTag.Length;
            }
        }
    }

    private IEnumerator DebounceCoroutine(float duration)
    {
        isDebouncing = true;
        yield return new WaitForSecondsRealtime(duration);
        isDebouncing = false;
    }
}