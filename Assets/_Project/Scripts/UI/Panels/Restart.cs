using UnityEngine;

public class Restart : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Pause _pause;
    [SerializeField] private GameObject _cursor;
    [SerializeField] private GameObject _pauseButton;
    
    private void OnEnable()
    {
        _cursor.SetActive(false);
        _pauseButton.SetActive(false);
    }
    
    public void TryRestart()
    {
        GameObject player = _gameManager.Player.PlayerController.gameObject;
        
        if (player.TryGetComponent(out PlayerBoostTarget boostTarget))
            boostTarget.StopBoost();
        
        gameObject.SetActive(false);
        _gameManager.SetPos();
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        Time.timeScale = 1;
        CameraControl.IsPause = false;
        
        if (_cursor != null)
            _cursor.SetActive(true);
        
        _pauseButton.SetActive(true);
    }
}