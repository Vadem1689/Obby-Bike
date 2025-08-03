using System;
using UnityEngine;
using Zenject;

public class ProgressBarPresenter : IInitializable, IDisposable
{
    private readonly ProgressBar _progressBar;
    private readonly int _pointCount;
    private int _currentPointIndex;

    public ProgressBarPresenter(ProgressBar progressBar, GameManager gameManager)
    {
        _progressBar = progressBar;
        _pointCount = gameManager.CheckPointCount;
    }

    public void Initialize()
    {
        CheckPoints.CheckPointReached += OnPlayerPointReached;
    }

    public void Dispose()
    {
        CheckPoints.CheckPointReached -= OnPlayerPointReached;
    }

    private void OnPlayerPointReached()
    {
        _currentPointIndex = PlayerPrefs.GetInt("point");
        float progress = (float)_currentPointIndex / _pointCount * 100;
        
        _progressBar.AnimatePlayerProgress(progress);
    }
}