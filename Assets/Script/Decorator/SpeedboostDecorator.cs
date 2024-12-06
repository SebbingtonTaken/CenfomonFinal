public class SpeedBoostDecorator : IDecorator
{
    private float speedBoostAmount;

    public SpeedBoostDecorator(float boostAmount)
    {
        speedBoostAmount = boostAmount;
    }

    public void ApplyDecorator(PlayerController player)
    {
        player.ApplySpeedDecorator(speedBoostAmount, 3600);
    }


}