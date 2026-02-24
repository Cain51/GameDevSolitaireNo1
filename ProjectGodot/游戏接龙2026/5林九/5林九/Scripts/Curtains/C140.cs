using System.Threading.Tasks;

public class C140 : Curtain
{
    bool leave = false;
    public override async Task OnEnter()
    {
        leave = false;
        //wait for 30 seconds
        await Task.Delay(30000);
        
        if(leave) return;
        Find.Game.ShowCurtain(144);
    }

    public override async Task OnExit()
    {
        leave = true;
    }

    protected async override Task C0(int curtainId)
    {
        await C1(curtainId);
    }
    protected async override Task C1(int curtainId)
    {
        await C2(curtainId);
    }

    async override protected Task C2(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("运气检定中...");
        if(Attr(Attributes.幸运) <= 0)
        {
            await M.Append("由于你的运气惨绝人寰，系统决定补偿你。");
            Find.Game.ShowCurtain(146);
            return;
        }
        else
        {
            await M.Append("无事发生。");
        }
        Find.Game.ShowCurtain(curtainId);
    }
}