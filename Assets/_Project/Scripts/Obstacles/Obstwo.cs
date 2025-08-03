using UnityEngine;
using Zenject;

public class Obstwo : MonoBehaviour
{
    [SerializeField] float Speed;
    [SerializeField] Vector3 position;
    [SerializeField] [Range(0, 1)] float range;
    
    private Restart _restartPanel;
    private Vector3 _startPosition;
    
    [Inject]
    public void Construct(Restart restartPanel)
    {
        _restartPanel = restartPanel;
    }
    
    private void Start()
    {
        _startPosition = transform.position;
    }
    
    private void FixedUpdate()
    {
        range = Mathf.PingPong(Time.time * Speed, 1);
        transform.position = _startPosition + range * position;
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