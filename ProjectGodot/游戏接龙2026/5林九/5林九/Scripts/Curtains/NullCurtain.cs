using System.Threading.Tasks;

public class NullCurtain : Curtain
{
    public override Task OnSelect(int index, int curtainId)
    {
        Find.Game.ShowCurtain(curtainId);
        return base.OnSelect(index, curtainId);
    }
}