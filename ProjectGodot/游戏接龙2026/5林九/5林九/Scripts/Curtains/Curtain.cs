using System.Threading.Tasks;
using Godot;

public class Curtain
{
    public static Curtain NULL = new NullCurtain();

    public async virtual Task OnEnter()
    {
        GD.Print("进入了一个新的幕布");
    }

    public async virtual Task OnExit()
    {
        GD.Print("离开了一个幕布");
    }

    public async virtual Task OnSelect(int index, int curtainId)
    {
        if(index == 0)
        {
            await C0(curtainId);
        }
        if(index == 1)
        {
            await C1(curtainId);
        }
        if(index == 2)
        {
            await C2(curtainId);
        }
        Find.Game.EventCheckMessage.ForceClose();
    }

    protected async virtual Task C0(int curtainId)
    {
        Find.Game.ShowCurtain(curtainId);
    }

    protected async virtual Task C1(int curtainId)
    {
        Find.Game.ShowCurtain(curtainId);
    }

    protected async virtual Task C2(int curtainId)
    {
        Find.Game.ShowCurtain(curtainId);
    }

    public int Attr(Attributes attr)
    {
        return Find.Player.GetAttributeLevel(attr);
    }

    public CheckResult CheckStealth(int threshold)
    {
        if(Find.Player.GetAttributeLevel(Attributes.智力) < threshold)
        {
            return "你的智力不足以让你进行潜行";
        }

        if(Find.Player.GetAttributeLevel(Attributes.武德) >= 3)
        {
            return "你高尚的武德不允许你进行潜行，并让对手先出招";
        }

        return CheckResult.Success();
    }

    public CheckResult CheckCommunication(int threshold)
    {
        if(Find.Player.GetAttributeLevel(Attributes.美貌) < threshold)
        {
            return "你太丑了，对方拒绝交流";
        }

        if(Find.Player.GetAttributeLevel(Attributes.智力) < threshold)
        {
            return "你的智力不足以让你进行交流";
        }

        return CheckResult.Success();
    }

    public CheckResult CheckAmbush(int threshold)
    {
        if(Find.Player.GetAttributeLevel(Attributes.智力) < threshold)
        {
            return "你的智力不足以让你进行偷袭";
        }

        if(Find.Player.GetAttributeLevel(Attributes.武德) >= 3)
        {
            return "你高尚的武德不允许你偷袭，并让对手先出招";
        }

        return CheckResult.Success();
    }
}