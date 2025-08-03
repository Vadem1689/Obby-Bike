using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using System.Collections;

public class PlayerSkin : MonoBehaviour
{
    [SerializeField] private Transform _skinContainer;
    [SerializeField] private Transform _bikeContainer;
    [SerializeField] private Animator _animator;

    public UnityEvent skinEquiped;

    private AsyncOperationHandle<GameObject>? _handle;

    [Inject] private Player _player;

    public async Task ApplyCharacterSkinAsync(SkinDefinition skin)
    {
        if (_skinContainer == null)
        {
            Debug.LogWarning("[PlayerSkin] SkinContainer не задан!");
            return;
        }

        Transform parent = _skinContainer.parent;
        string containerName = _skinContainer.name;
        if (parent == null)
        {
            Debug.LogWarning("[PlayerSkin] Родитель SkinContainer не найден!");
            return;
        }

        Destroy(_skinContainer.gameObject);

        if (_handle.HasValue)
            Addressables.Release(_handle.Value);

        GameObject prefab = skin.Prefab;
        if (prefab == null && skin.PrefabReference.RuntimeKeyIsValid())
        {
            _handle = skin.PrefabReference.LoadAssetAsync<GameObject>();
            prefab = await _handle.Value.Task;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[PlayerSkin] Нет Prefab для персонажа в {skin.name}");
            return;
        }

        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            Transform child = prefab.transform.GetChild(i);
            if (child.GetComponentInChildren<Canvas>() != null)
                continue;
            
            GameObject instantiate = Instantiate(child.gameObject, parent, true);
            instantiate.name = child.name;

            RenameRecursively(instantiate.transform);

            instantiate.transform.localPosition = child.localPosition;
            instantiate.transform.localRotation = child.localRotation;
            
            instantiate.transform.localScale = child.localScale;
            if (i== 0)
            {

                _skinContainer = instantiate.transform;
            }
        }


        StartCoroutine(UpdateAnimatorCoroutine());
        skinEquiped?.Invoke();
    }

    public async Task ApplyBikeSkinAsync(SkinDefinition skin)
    {
        if (_bikeContainer == null)
        {
            Debug.LogWarning("[PlayerSkin] BikeContainer не задан!");
            return;
        }

        // Очистка bikeContainer
        for (int i = _bikeContainer.childCount - 1; i >= 0; i--)
            Destroy(_bikeContainer.GetChild(i).gameObject);

        if (_handle.HasValue)
            Addressables.Release(_handle.Value);

        GameObject prefab = skin.bikePrefab;
        if (prefab == null && skin.PrefabReference.RuntimeKeyIsValid())
        {
            _handle = skin.PrefabReference.LoadAssetAsync<GameObject>();
            prefab = await _handle.Value.Task;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[PlayerSkin] Нет bikePrefab в {skin.name}");
            return;
        }

        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            Transform child = prefab.transform.GetChild(i);
            if (child.GetComponentInChildren<Canvas>() != null)
                continue;

            GameObject instantiate = Instantiate(child.gameObject, _bikeContainer, true);
            instantiate.name = child.name;

            RenameRecursively(instantiate.transform);

            instantiate.transform.localPosition = child.localPosition;
            instantiate.transform.localRotation = child.localRotation;
            instantiate.transform.localScale = child.localScale;
        }

        StartCoroutine(UpdateAnimatorCoroutine());
        skinEquiped?.Invoke();
    }

    private IEnumerator UpdateAnimatorCoroutine()
    {
        if (_animator != null)
        {
            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;

            _animator.enabled = false;

            yield return new WaitForSeconds(0.01f);

            _animator.enabled = true;

            if (_animator.runtimeAnimatorController == null)
            {
                _animator.runtimeAnimatorController = controller;
            }

            _player.PlayerController.SetAnimator(_animator);
        }
        else
        {
            Debug.LogWarning("[PlayerSkin] Не найден Animator!");
        }
    }

    private void RenameRecursively(Transform transform)
    {
        transform.name = transform.name.Replace("(Clone)", "");
        foreach (Transform c in transform)
            RenameRecursively(c);
    }
}