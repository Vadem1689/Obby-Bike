using UnityEngine;
using Zenject;

public class Obsone : MonoBehaviour
{
    private Restart _restartPanel;

    [Inject]
    public void Construct(Restart restartPanel)
    {
        _restartPanel = restartPanel;
    }
    
    private void FixedUpdate()
    {
        transform.Rotate(0, 1.5f, 0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BotRespawner botRespawner))
        {
            botRespawner.Respawn();
            return;
        }
        
        if (!other.TryGetComponent(out PlayerController _))
            return;

        if (other.TryGetComponent(out PlayerBoostTarget boostTarget))
            boostTarget.StopBoost();
        
        DieEffectManager.Instance.PlayDieEffect(other.transform.position);
        Time.timeScale = 0;
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        
        other.gameObject.SetActive(false);
        _restartPanel.gameObject.SetActive(true);
        CameraControl.IsPause = true;
    }
}