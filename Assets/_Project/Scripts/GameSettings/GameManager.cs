using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Zenject;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _checkPoints;
    [SerializeField] private TextMeshProUGUI _timer;
    [SerializeField] private TextMeshProUGUI _timeNowText;
    [SerializeField] private TextMeshProUGUI _bestTimeText;
    [SerializeField] private Pause _pause;

    private Player _player;
    private float _time;

    [Inject]
    public void Construct(Player player, IInput input)
    {
        _player = player;
        _player.transform.position = _checkPoints.Length > 0 ? _checkPoints[0].transform.position : Vector3.zero;
    }

    public Player Player => _player;
    
    public int CheckPointCount => _checkPoints.Length;

    private void Awake()
    {
        Application.targetFrameRate = (Mathf.Max(60, Convert.ToInt32(Screen.currentResolution.refreshRateRatio.value)));
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        _pause.gameObject.SetActive(false);
        _pause.Resume();
        
        SetPos();
    }
    
    public void SetPos()
    {
        int point = PlayerSessionProgress.Point;
        GameObject player = _player.PlayerController.gameObject;

        int clampedIndex = Mathf.Clamp(point, 0, _checkPoints.Length - 1);
        if (_checkPoints.Length == 0)
            return;

        player.transform.position = _checkPoints[clampedIndex].transform.position;
        player.transform.rotation = Quaternion.identity;
        player.SetActive(true);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        
        if (rb != null)
            rb.velocity = Vector3.zero;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        _time = (float)Time.timeSinceLevelLoad;
        _timer.text = (Math.Round(_time, 2) + "");
        
        if (Reklama.IsReclama)
        {
            Reklama.IsReclama = false;
            
            _pause.gameObject.SetActive(true);
            _pause.TryUsePause();
            
            if (!Application.isMobilePlatform)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
    
    public void HideCursore()
    {
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    public void Finish()
    {
        float bestTime = PlayerPrefs.GetFloat("BestTime");
        
        if (_time < bestTime)
        {
            PlayerPrefs.SetFloat("BestTime", _time);
            bestTime = _time;
        }
        
        _timeNowText.text = "�����: " + Math.Round(_time, 2);
        _bestTimeText.text = "������ �����: " + Math.Round(bestTime, 2);
        
        PlayerSessionProgress.Reset();
    }
    
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}