using System;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public event Action<bool> PauseStateSwitched;
    
    [SerializeField] private GameObject _cursore;
    
    private void OnEnable()
    {
        if (_cursore != null)
            _cursore.SetActive(false);
    }

    public void TryUsePause()  
    {
        Time.timeScale = 0;
        
        PauseStateSwitched?.Invoke(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (_cursore != null)
            _cursore.SetActive(true);
        
        PauseStateSwitched?.Invoke(false);
    }
}