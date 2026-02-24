public static class Find
{
    public static Game Game { get; set; }

    public static Actor Player => Game.Player;
}