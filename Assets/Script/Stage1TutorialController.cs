using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Stage1TutorialController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialImage;
    [SerializeField] private Image wImage;
    [SerializeField] private Image aImage;
    [SerializeField] private Image sImage;
    [SerializeField] private Image dImage;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 45.0f;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); // 薄灰色

    private void Start()
    {
        if (tutorialImage != null)
        {
            tutorialImage.SetActive(true);
            StartCoroutine(HideTutorialAfterDelay());
        }
        else
        {
            Debug.LogWarning("Stage1TutorialController: tutorialImage is not assigned.");
        }
    }

    private void Update()
    {
        // New Input System (Input System Package) を使用してキー入力を取得
        if (Keyboard.current != null)
        {
            UpdateKeyVisual(wImage, Keyboard.current.wKey.isPressed);
            UpdateKeyVisual(aImage, Keyboard.current.aKey.isPressed);
            UpdateKeyVisual(sImage, Keyboard.current.sKey.isPressed);
            UpdateKeyVisual(dImage, Keyboard.current.dKey.isPressed);
        }
    }

    private void UpdateKeyVisual(Image image, bool isPressed)
    {
        if (image != null)
        {
            image.color = isPressed ? pressedColor : defaultColor;
        }
    }

    private IEnumerator HideTutorialAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        
        if (tutorialImage != null)
        {
            tutorialImage.SetActive(false);
        }
    }

    // 外部から強制的に非表示にする場合などに使用
    public void HideTutorial()
    {
        if (tutorialImage != null)
        {
            tutorialImage.SetActive(false);
        }
    }
}
