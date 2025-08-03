public class BoosterJump
{
    public void TryApplyJumpBoost(PlayerBoostTarget target, BoostZonePreset preset)
    {
        if (target.TryGetComponent(out PlayerController controller))
        {
            controller.ApplyTemporaryJumpBoost(preset.JumpMultiplier, preset.JumpDuration);
        }
    }
}