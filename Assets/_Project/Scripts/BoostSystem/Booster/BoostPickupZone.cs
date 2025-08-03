using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoostPickupZone : MonoBehaviour
{
    [SerializeField] private BoostZone _boostZone;
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private float _fillTime = 2f;

    private Coroutine _fillRoutine;
    private PlayerBoostTarget _currentTarget;

    private void OnTriggerEnter(Collider other)
    {
        BoostType type = _boostZone.ZoneType;
        
        if (type != BoostType.Acceleration && type != BoostType.Jump)
            return;

        
        if (other.TryGetComponent(out PlayerBoostTarget boostTarget))
        {
            _currentTarget = boostTarget;
            _fillRoutine = StartCoroutine(FillCoroutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerBoostTarget boostTarget) && boostTarget == _currentTarget)
        {
            if (_fillRoutine != null)
                StopCoroutine(_fillRoutine);

            _progressBar.SetPlayerProgress(0f);
            _currentTarget = null;
        }
    }

    private IEnumerator FillCoroutine()
    {
        float time = 0f;

        while (time < _fillTime)
        {
            time += Time.deltaTime;
            _progressBar.SetPlayerProgress(Mathf.Clamp01(time / _fillTime));

            yield return null;
        }
        
        _boostZone.ApplyBoost(_currentTarget);
        _progressBar.SetPlayerProgress(0f);
        _currentTarget = null;
    }
}