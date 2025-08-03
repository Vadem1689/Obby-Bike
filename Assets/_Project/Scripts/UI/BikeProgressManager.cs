using System;
using UnityEngine;
using Zenject;
using System.Collections;

public class BikeProgressManager : MonoBehaviour
{
    [System.Serializable]
    public class BikeSkinData
    {
        public SkinDefinition skin;
        public Sprite uiSprite;
        public Color spriteColor;
    }

    public static event Action<float, Sprite, Color> OnProgressUpdated;
    public static event Action OnBikeUnlocked;
    public static event Action OnProgressReset;

    [SerializeField] private float currentProgress = 0f;
    [SerializeField] private int checkpointCount = 0;
    [SerializeField] private float progressStep = 25f;
    [SerializeField] private BikeSkinData[] bikeSkins;
    [SerializeField] private int currentBikeIndex = 0;
    private bool isFirstUnlock = true;

    [Inject] private PlayerSkin playerSkin;

    private void Start()
    {
        CheckPoints.CheckPointReached += OnCheckpointReached;
        currentBikeIndex = PlayerPrefs.GetInt("CurrentBikeIndex", 0);
        currentBikeIndex = 0;
    }

    private void OnDestroy()
    {
        CheckPoints.CheckPointReached -= OnCheckpointReached;
    }

    private void OnCheckpointReached()
    {
        checkpointCount++;
        if (checkpointCount % 3 == 0)
        {
            currentProgress += progressStep;
            if (currentBikeIndex + 1 < bikeSkins.Length || isFirstUnlock)
            {
                OnProgressUpdated?.Invoke(currentProgress,
                    isFirstUnlock ? bikeSkins[0].uiSprite : bikeSkins[currentBikeIndex + 1].uiSprite, isFirstUnlock ? bikeSkins[0].spriteColor : bikeSkins[currentBikeIndex + 1].spriteColor);
            }
        }

        if (currentProgress >= 100f)
        {
            OnBikeUnlocked?.Invoke();
            UnlockNextBike();
            StartCoroutine(HandleBikeUnlock());
        }
    }

    private IEnumerator HandleBikeUnlock()
    {
        yield return new WaitForSeconds(3f);
        ResetProgress();
    }

    private void UnlockNextBike()
    {
        if (!isFirstUnlock)
        {
            currentBikeIndex++;
            if (currentBikeIndex >= bikeSkins.Length)
            {
                currentBikeIndex = 0;
            }
        }

        isFirstUnlock = false;
        //PlayerPrefs.SetInt("CurrentBikeIndex", currentBikeIndex);
        if (currentBikeIndex < bikeSkins.Length)
        {
            ApplyBikeSkin(bikeSkins[currentBikeIndex]);
        }
    }

    private void ResetProgress()
    {
        currentProgress = 0f;
        checkpointCount = 0;
        OnProgressReset?.Invoke();
    }

    private void ApplyBikeSkin(BikeSkinData bikeSkin)
    {
        if (bikeSkin != null && bikeSkin.skin != null)
        {
            _ = playerSkin.ApplyBikeSkinAsync(bikeSkin.skin);
        }
    }

    public float GetCurrentProgress()
    {
        return currentProgress;
    }
}