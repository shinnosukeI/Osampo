using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button closeButton;

    void Start()
    {
        // Initialize sliders with current values
        if (SoundManager.Instance != null)
        {
            if (bgmSlider != null)
            {
                bgmSlider.value = SoundManager.Instance.BGMVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMValueChanged);
            }

            if (seSlider != null)
            {
                seSlider.value = SoundManager.Instance.SEVolume;
                seSlider.onValueChanged.AddListener(OnSEValueChanged);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void OnBGMValueChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.BGMVolume = value;
        }
    }

    private void OnSEValueChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SEVolume = value;
        }
    }

    private void OnCloseButtonClicked()
    {
        // Save settings when closing
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SaveVolume();
            SoundManager.Instance.PlayCommonButtonSE(); // Feedback
        }

        // Hide the panel
        gameObject.SetActive(false);
    }
}
