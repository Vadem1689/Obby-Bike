using UnityEngine;

public class DieEffectManager : MonoBehaviour
{
    public static DieEffectManager Instance { get; private set; }

    [SerializeField] private GameObject dieEffectPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayDieEffect(Vector3 position)
    {
        if (dieEffectPrefab != null)
        {
            GameObject effect = Instantiate(dieEffectPrefab, position, Quaternion.identity);

            if (effect.TryGetComponent(out ParticleSystem particleSystem))
            {
                if (particleSystem != null)
                {
                    var main = particleSystem.main;
                    
                    main.useUnscaledTime = true;
                }
            }
        }
    }
}