using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndingPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI statsText;
    public Button returnButton;

    [Header("Typing Effect")]
    public float typingSpeed = 0.02f;

    private Coroutine typingCoroutine;
    private System.Action onReturnCallback;
    private string fullBodyText;
    private bool isTyping = false;
    private bool hasBeenShown = false;

    void Awake()
    {
        // Bind in Awake ¡ª runs immediately when activated, before Start
        if (returnButton != null)
            returnButton.onClick.AddListener(OnReturnClicked);
    }

    void Start()
    {
        // Only auto-hide if Show() hasn't already been called
        if (!hasBeenShown && panel != null)
            panel.SetActive(false);
    }

    public void Show(string title, string body, int mimicsAllowed, int mimicsDenied, System.Action onReturn = null)
    {
        hasBeenShown = true;
        onReturnCallback = onReturn;
        fullBodyText = body;

        if (titleText != null)
            titleText.text = title;

        if (statsText != null)
            statsText.text = $"MIMICS INTERCEPTED: {mimicsDenied}  |  MIMICS CLEARED: {mimicsAllowed}  |  TOTAL: {mimicsAllowed + mimicsDenied}";

        if (returnButton != null)
            returnButton.gameObject.SetActive(false);

        if (panel != null)
            panel.SetActive(true);

        // Force canvas update so layout is correct
        Canvas.ForceUpdateCanvases();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = true;
        typingCoroutine = StartCoroutine(TypeBody(body));
    }

    IEnumerator TypeBody(string text)
    {
        if (bodyText == null) yield break;

        bodyText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            bodyText.text += text[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        ShowReturnButton();
    }

    void ShowReturnButton()
    {
        if (returnButton != null)
        {
            returnButton.gameObject.SetActive(true);
            returnButton.interactable = true;
        }
    }

    void OnReturnClicked()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            isTyping = false;

            if (bodyText != null)
                bodyText.text = fullBodyText;

            ShowReturnButton();
            return;
        }

        hasBeenShown = false;

        if (panel != null)
            panel.SetActive(false);

        var callback = onReturnCallback;
        onReturnCallback = null;
        callback?.Invoke();
    }

    public void Hide()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
        hasBeenShown = false;

        if (panel != null)
            panel.SetActive(false);
    }
}