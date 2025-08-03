using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class BikeProgressUI : MonoBehaviour
{
    [SerializeField] private Image fillableProgressImage;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text newBikeUnlockedText;
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Image bikeBackground;
    [SerializeField] public UnityEvent ProgressShown;

    [SerializeField] private float fillDuration = 1f;

    private void Start()
    {
        BikeProgressManager.OnProgressUpdated += UpdateProgressUI;
        BikeProgressManager.OnBikeUnlocked += OnBikeUnlocked;
        BikeProgressManager.OnProgressReset += OnProgressReset;

        progressPanel.SetActive(false);
        if (newBikeUnlockedText != null)
        {
            newBikeUnlockedText.gameObject.SetActive(false);
        }

        if (fillableProgressImage != null)
        {
            fillableProgressImage.type = Image.Type.Filled;
            fillableProgressImage.fillMethod = Image.FillMethod.Vertical;
            fillableProgressImage.fillAmount = 0f;
        }
    }

    private void OnDestroy()
    {
        BikeProgressManager.OnProgressUpdated -= UpdateProgressUI;
        BikeProgressManager.OnBikeUnlocked -= OnBikeUnlocked;
        BikeProgressManager.OnProgressReset -= OnProgressReset;
    }

    private void ShowProgressPanel()
    {
        progressPanel.SetActive(true);
        ProgressShown?.Invoke();
    }

    private void UpdateProgressUI(float progress, Sprite uiSprite, Color color)
    {
        ShowProgressPanel();

        if (fillableProgressImage != null)
        {
            fillableProgressImage.color = color;
            StartCoroutine(AnimateFill(progress / 100f));
            if (uiSprite != null)
            {
                fillableProgressImage.sprite = uiSprite;
            }
        }

        if (bikeBackground != null && uiSprite != null)
        {
            bikeBackground.sprite = uiSprite;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = $"{Mathf.RoundToInt(progress)}%";
        }

        if (newBikeUnlockedText != null)
        {
            newBikeUnlockedText.gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateFill(float targetFill)
    {
        float startFill = fillableProgressImage.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fillDuration;
            float easedT = 1f - (1f - t) * (1f - t);
            fillableProgressImage.fillAmount = Mathf.Lerp(startFill, Mathf.Clamp01(targetFill), easedT);
            yield return null;
        }

        fillableProgressImage.fillAmount = Mathf.Clamp01(targetFill);
    }

    private void OnBikeUnlocked()
    {
        ShowProgressPanel();
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
        }
        if (newBikeUnlockedText != null)
        {
            newBikeUnlockedText.gameObject.SetActive(true);
        }
        StartCoroutine(HideUnlockedTextAfterDelay());
    }

    private void OnProgressReset()
    {
        progressPanel.SetActive(false);
        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }
        if (newBikeUnlockedText != null)
        {
            newBikeUnlockedText.gameObject.SetActive(false);
        }
        if (fillableProgressImage != null)
        {
            fillableProgressImage.fillAmount = 0f;
        }
    }

    private IEnumerator HideUnlockedTextAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (newBikeUnlockedText != null)
        {
            newBikeUnlockedText.gameObject.SetActive(false);
        }
    }
}