public class HealResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int RemainingPotions { get; set; }

    public static HealResult Fail(string msg)
        => new() { IsSuccess = false, Message = msg };

    public static HealResult Success(int hp, int max, int remaining)
        => new()
        {
            IsSuccess = true,
            Health = hp,
            MaxHealth = max,
            RemainingPotions = remaining
        };
}