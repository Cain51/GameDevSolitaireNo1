using System.Threading.Tasks;
using Godot;

public class C108 : Curtain
{
    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你想吃点东西");
        await M.Append("开始进食检定....");

        if(Attr(Attributes.智力) <= 1)
        {
            await M.Append("你智力低下，并不能理解什么是吃。");
            await M.Append("遂决定睡一觉。");
            Find.Game.ShowCurtain(Cs.前妻森林112);
            return;
        }
        
        Find.Game.ShowCurtain(curtainId);
    }

    protected override async Task C1(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你伸手摸向后背");
        if(Attr(Attributes.体力) <= 1)
        {
            await M.Append("你体制虚弱，伸向后背的手，从肩膀上脱臼了。");
            await M.Append("你疼晕了过去。");
            Find.Game.ShowCurtain(Cs.前妻森林112);
            return;
        }
        Find.Game.ShowCurtain(curtainId);
    }

    protected override async Task C2(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你打开了信件");
        if(Attr(Attributes.智力) <= 1)
        {
            await M.Append("你的智力不足以阅读信件。");
            await M.Append("你把信件放进了嘴里。");
            await M.Append("这一觉，你睡得很甜。");
            Find.Game.ShowCurtain(Cs.前妻森林112);
            return;
        }
        Find.Game.ShowCurtain(curtainId);
    }
}