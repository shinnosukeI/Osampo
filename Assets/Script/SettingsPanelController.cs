using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Button closeButton;

    [Header("Appearance")]
    [SerializeField] private Color activeColor = new Color(1f, 0.7f, 0.7f); // 薄い赤
    [SerializeField] private Color inactiveColor = Color.white;

    void Start()
    {
        // Initialize sliders with current values
        if (SoundManager.Instance != null)
        {
            if (bgmSlider != null)
            {
                bgmSlider.value = SoundManager.Instance.BGMVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMValueChanged);
                ApplySliderColors(bgmSlider);
            }

            if (seSlider != null)
            {
                seSlider.value = SoundManager.Instance.SEVolume;
                seSlider.onValueChanged.AddListener(OnSEValueChanged);
                ApplySliderColors(seSlider);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void ApplySliderColors(Slider slider)
    {
        if (slider == null) return;

        // 1. Background (Inactive)
        // 標準的なUnity UI Sliderでは "Background" という名前の子オブジェクトがある
        Transform bgTrans = slider.transform.Find("Background");
        if (bgTrans != null)
        {
            Image bgImage = bgTrans.GetComponent<Image>();
            if (bgImage != null) bgImage.color = inactiveColor;
        }

        // 2. Fill (Active)
        // Sliderコンポーネントが FillRect の参照を持っている
        if (slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null) fillImage.color = activeColor;
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
