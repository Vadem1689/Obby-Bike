public class BoosterAcceleration
{
    public void TryApplyAcceleration(PlayerBoostTarget target, BoostZonePreset preset)
    {
        if (target.TryGetComponent(out PlayerController controller))
        {
            controller.ApplyTemporarySpeedBoost(preset.AccelerationMultiplier, preset.AccelerationDuration);
        }
    }
}