using System.Threading.Tasks;
using Godot;

public class C302 : Curtain
{

    protected override async Task C0(int curtainId)
    {
        if(Find.Game.Flags.Contains("锦囊1"))
        {
            Find.Game.ShowCurtain(303);
            return;
        }else if(Find.Game.Flags.Contains("锦囊2"))
        {
            Find.Game.ShowCurtain(304);
            return;
        }else if(Find.Game.Flags.Contains("锦囊3"))
        {
            Find.Game.ShowCurtain(305);
            return;
        }
        Find.Game.ShowCurtain(curtainId);
    }
}