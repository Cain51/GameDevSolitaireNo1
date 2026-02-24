using System.Threading.Tasks;
using Godot;

public class C104 : Curtain
{
    int count = 0;

    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你伸手抓向了湿漉漉的袋子");
        Find.Game.Flags.Add("锦囊1");
        await CommonCheck(curtainId);
    }

    protected override async Task C1(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你伸手抓向了破旧的袋子");
        Find.Game.Flags.Add("锦囊2");
        await CommonCheck(curtainId);
    }

    protected override async Task C2(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你伸手抓向了闪发亮的袋子");
        Find.Game.Flags.Add("锦囊3");
        await CommonCheck(curtainId);
    }

    async Task CommonCheck(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Append("开始开箱检定....");
        if(Attr(Attributes.体力) >= 4)
        {
            await M.Append("你有无敌的体魄，是宇宙间最强的生物");
            await M.Append("袋子被捏成了齑粉");
            Find.Game.ShowCurtain(Cs.锦囊后108);
            return;
        }
        
        if(Attr(Attributes.体力) < 1)
        {
            await M.Append("你比霍金还弱，根本拿不了袋子，你被气死了");
            await M.Append("你死了");

            if(Attr(Attributes.幸运) >= 3)
            {
                if(Attr(Attributes.体力) > 3)
                {
                    await M.Append("死前，你脆弱的手臂折断了，碰到了背上的传送阵");
                    Find.Game.ShowCurtain(Cs.前妻森林112);
                    return;
                }
                else
                {
                    await M.Append("但是你太幸运了，一股双黄连泉水喷出，苦的你的火气全消，并穿越了！");
                    Find.Game.ShowCurtain(Cs.未来303);
                    return;
                }
            }
            count ++;
            if(count >= 2)
            {
                await M.Append("霍金也没你死的次数多，体质+1");
                Find.Player.AddAttr(Attributes.体力, 1);
                Find.Game.Player.Name = Find.Game.Player.GetName(Find.Game.Player.AttributeValues);
                Find.Player.ShowAttr();
                await M.Append("你现在是一个" + Find.Player.Name + "的大四学生");
                count = -10;
            }
            Find.Game.ShowCurtain(Cs.End150);
            return;
        }

        if(Attr(Attributes.武德) < 2)
        {
            await M.Append("你没有武德，想拿所有的袋子");
            await M.Append("三个袋子混合在一起，发出了奇怪的声音");
            Find.Game.ShowCurtain(Cs.前妻森林112);
            return;
        }

        Find.Game.ShowCurtain(curtainId);
    }
}