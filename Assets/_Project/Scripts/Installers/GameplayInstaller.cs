using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private Player _player;
    [SerializeField] private MobileInput _mobileInput;
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private BoostTimerUI _boostTimerUI;
    
    public override void InstallBindings()
    {
        Container.Bind<UIInfo>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<CameraControl>().FromComponentInHierarchy().AsSingle().NonLazy();
        
        Container.BindInterfacesAndSelfTo<Player>().FromComponentInNewPrefab(_player).AsSingle().NonLazy();
        Container.Bind<PlayerSkin>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle().NonLazy();
        
        Container.BindInterfacesAndSelfTo<GameManager>().FromComponentInHierarchy().AsSingle().NonLazy();

        if (Application.isMobilePlatform)
        {
            _mobileInput.Activate();
            Container.BindInterfacesAndSelfTo<MobileInput>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
        else
        {
            Container.BindInterfacesAndSelfTo<DesktopInput>().AsSingle().NonLazy();
        }      

        Container.Bind<ProgressBar>().FromInstance(_progressBar).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<UIController>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ProgressBarPresenter>().AsSingle().NonLazy();
        Container.Bind<BoostTimerUI>().FromInstance(_boostTimerUI).AsSingle().NonLazy();
        
        Container.Bind<Restart>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}