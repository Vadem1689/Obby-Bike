using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Events;

public class InternetConnectionChecker : MonoBehaviour
{
    public UnityEvent ConnectionLost;
    public UnityEvent Connected;

    [SerializeField] private float checkInterval = 10f;

    private bool isConnected = false;
    private const string checkUrl = "https://www.google.com";

    void Start()
    {
        StartCoroutine(CheckInternetConnectionRoutine());
    }

    private IEnumerator CheckInternetConnectionRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(CheckInternetConnectionAsync());
            yield return new WaitForSecondsRealtime(checkInterval);
        }
    }

    private IEnumerator CheckInternetConnectionAsync()
    {
        using (UnityWebRequest request = UnityWebRequest.Head(checkUrl))
        {
            request.timeout = 2;
            yield return request.SendWebRequest();

            bool currentConnectionStatus = request.result == UnityWebRequest.Result.Success;

            if (currentConnectionStatus != isConnected)
            {
                isConnected = currentConnectionStatus;
                
                if (isConnected)
                    Connected.Invoke();
                else
                    ConnectionLost.Invoke();
            }
        }
    }
    
    public void SetCheckInterval(float newInterval)
    {
        if (newInterval > 0)
        {
            checkInterval = newInterval;
        }
    }
    
    public void CheckInternetNow()
    {
        StartCoroutine(CheckInternetConnectionAsync());
    }
}