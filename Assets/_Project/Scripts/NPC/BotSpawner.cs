using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    [Header("Spawn settings")]
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private Waypoint _startPoint;
    [SerializeField] private Transform _finPoint;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(1, 0, 0);
    [SerializeField] private LayerMask _occupancyMask;
    [SerializeField] private int _botCount = 5;
    [SerializeField] private float _spawnCheckRadius = 1f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float _retryInterval = 0.5f;
    [SerializeField, Min(0f)] private float _leaveDistance = 1.2f;

    [Header("Trail settings")]
    [SerializeField, Range(0f, 1f)] private float _trailChance = 0.5f;

    [Header("Progress Bar settings")]
    [SerializeField] private ProgressBar _progressBar;
    
    private readonly List<BotProgressTracker> _progressTrackers = new List<BotProgressTracker>();
    
    private void Start()
    {
        if (_botPrefab == null || _startPoint == null || _finPoint == null || _progressBar == null)
            return;
        
        StartCoroutine(SpawnBotsSequentially());
    }
    
    // private void Start()
    // {
    //     if (_botPrefab == null || _startPoint == null || _finPoint == null || _progressBar == null)
    //     {
    //         Debug.LogError("BotSpawner: Missing required references (_botPrefab, _startPoint, _finPoint, or _progressBar)");
    //         return;
    //     }
    //
    //     // Calculate total distance from start to finish
    //     float totalDistance = Vector3.Distance(_startPoint.transform.position, _finPoint.transform.position);
    //
    //     for (int i = 0; i < _botCount; i++)
    //     {
    //         Vector3 position = _startPoint.transform.position + _spawnOffset * i;
    //         GameObject botPrefab = Instantiate(_botPrefab, position, Quaternion.identity);
    //
    //         bool useTrail = Random.value < _trailChance;
    //         TrailRenderer trail = botPrefab.GetComponentInChildren<TrailRenderer>();
    //
    //         if (trail != null)
    //             trail.enabled = useTrail;
    //
    //         BotController controller = botPrefab.GetComponent<BotController>() ?? botPrefab.GetComponentInChildren<BotController>();
    //         BotInput botInput = new BotInput(botPrefab.transform, _startPoint);
    //
    //         controller.SetInput(botInput);
    //
    //         BotRespawner respawner = botPrefab.GetComponent<BotRespawner>() ?? botPrefab.AddComponent<BotRespawner>();
    //         respawner.Initialize(botInput, _startPoint);
    //
    //         // Initialize BotProgress
    //         BotProgress botProgress = botPrefab.GetComponent<BotProgress>() ?? botPrefab.AddComponent<BotProgress>();
    //         botProgress.Initialize(_startPoint.transform.position, _finPoint.transform.position);
    //         _progressBar.InitializeNPC(botPrefab);
    //         botProgress.OnProgressUpdated += (progress) => _progressBar.UpdateNPCProgress(botPrefab, progress);
    //
    //         botPrefab.SetActive(true);
    //
    //         StartCoroutine(TickBot(botInput, botProgress, botPrefab.transform));
    //     }
    // }
    
    private void Update()
    {
        for (int i = _progressTrackers.Count - 1; i >= 0; i--)
        {
            BotProgressTracker tracker = _progressTrackers[i];
            
            if (!tracker.IsAlive)
            {
                tracker.Dispose();
                _progressTrackers.RemoveAt(i);
            }
            else
            {
                tracker.Tick();
            }
        }
    }

    private IEnumerator SpawnBotsSequentially()
    {
        for (int i = 0; i < _botCount; i++)
        {
            Vector3 spawnPos = _startPoint.transform.position + _spawnOffset * i;

            while (!IsPositionClear(spawnPos, _spawnCheckRadius, _occupancyMask))
            {
                yield return new WaitForSeconds(_retryInterval);
            }

            GameObject botInstance = Instantiate(_botPrefab, spawnPos, Quaternion.identity);

            bool useTrail = Random.value < _trailChance;
            TrailRenderer trail = botInstance.GetComponentInChildren<TrailRenderer>();
            
            if (trail != null)
                trail.enabled = useTrail;

            BotController controller = botInstance.GetComponent<BotController>() ?? botInstance.GetComponentInChildren<BotController>();
            BotInput botInput = new BotInput(botInstance.transform, _startPoint);
            controller.SetInput(botInput);

            if (botInstance.TryGetComponent(out BotRespawner respawner))
                respawner.Initialize(botInput, _startPoint);
            
            BotProgress botProgress = botInstance.GetComponent<BotProgress>(); 
            BotProgressTracker tracker = new BotProgressTracker(botProgress, _progressBar, botInstance, _startPoint.transform.position, _finPoint.transform.position); 
            _progressTrackers.Add(tracker);
            
            botInstance.SetActive(true);

            StartCoroutine(TickBot(botInput));

            float timeout = 5f;
            float waited = 0f;

            while (Vector3.Distance(botInstance.transform.position, spawnPos) < _leaveDistance && waited < timeout)
            {
                yield return null;
                
                waited += Time.deltaTime;
            }
        }
    }

    private bool IsPositionClear(Vector3 position, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius, mask);

        if (hits == null || hits.Length == 0)
            return true;

        return false;
    }

    private IEnumerator TickBot(BotInput input)
    {
        while (true)
        {
            input.Tick();

            yield return null;
        }
    }
    
    // private IEnumerator TickBot(BotInput input, BotProgress botProgress, Transform botTransform)
    // {
    //     while (true)
    //     {
    //         input.Tick();
    //         botProgress.UpdateProgressFromPosition(botTransform.position);
    //         yield return new WaitForSeconds(Random.Range(1f, 2f)); // Update every 1–2 seconds
    //     }
    // }
}