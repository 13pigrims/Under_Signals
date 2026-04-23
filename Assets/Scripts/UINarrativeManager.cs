using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UINarrativeManager : MonoBehaviour
{
    public GameObject narrativePanel;
    public TextMeshProUGUI narrativeText;
    public Button continueButton;
    public float typingSpeed = 0.02f;

    private Coroutine typingCoroutine;
    private System.Action onComplete;
    private bool isTyping = false;
    private string currentFullText;

    void Start()
    {
        if (narrativePanel != null)
            narrativePanel.SetActive(false);
        else
            Debug.LogError("UINarrativeManager: narrativePanel not assigned!");

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        else
            Debug.LogError("UINarrativeManager: continueButton not assigned!");
    }

    public void ShowNarrative(string text, System.Action onCompleteCallback = null)
    {
        Debug.Log($"[UINarrativeManager] ShowNarrative called, text length: {text?.Length}");

        if (narrativePanel == null || narrativeText == null)
        {
            Debug.LogError("UINarrativeManager: narrativePanel or narrativeText not assigned!");
            onCompleteCallback?.Invoke();
            return;
        }

        narrativePanel.SetActive(true);
        Canvas.ForceUpdateCanvases();

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        onComplete = onCompleteCallback;
        currentFullText = text;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string fullText)
    {
        narrativeText.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            narrativeText.text += fullText[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        Debug.Log("[UINarrativeManager] Typing finished. Showing continue button.");

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
        }
    }

    void OnContinueClicked()
    {
        // If still typing, skip to full text instead of closing
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            isTyping = false;
            narrativeText.text = currentFullText;
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.interactable = true;
            }
            return;
        }

        Debug.Log("[UINarrativeManager] Continue button clicked. Closing panel.");

        if (narrativePanel != null)
            narrativePanel.SetActive(false);

        // бя FIX: save callback to local var BEFORE nulling,
        //   so if the callback calls ShowNarrative (setting a new onComplete),
        //   we don't overwrite it afterwards.
        var callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    public void ShowEnding(string text, System.Action onCompleteCallback = null)
    {
        Debug.Log("[UINarrativeManager] ShowEnding called.");
        if (narrativePanel == null || narrativeText == null) return;

        narrativeText.text = text;
        narrativePanel.SetActive(true);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
        }

        onComplete = onCompleteCallback;
    }
}