using System.Threading.Tasks;
using Godot;

public class C100 : Curtain
{
    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你想继续装睡");
        await M.Append("开始潜行检定....");
        var result = CheckStealth(3);
        if (result)
        {
            await M.Append("潜行成功，你成功地装睡了过去");
            Find.Game.ShowCurtain(curtainId);
        }
        else
        {
            await M.Append(result.Message);
            await M.Append("潜行失败，你被对方发现了");
            await M.Append("武德检定中...");
            if(Find.Player.GetAttributeLevel(Attributes.武德) > 3)
            {
                await M.Append("你武德高尚，让对手先出招");
                Find.Game.ShowCurtain(10301);
                return;
            }
            else
            {
                await M.Append("你武德低下，恬不知耻，决定先发制人");
                Find.Game.ShowCurtain(102);
            }
        }
    }

    protected override async Task C1(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你想进行偷袭，先发制人");
        await M.Append("开始偷袭检定....");
        var result = CheckAmbush(3);
        if (result)
        {
            await M.Append("偷袭成功，你成功地进行了偷袭");
            Find.Game.ShowCurtain(curtainId);
        }
        else
        {
            await M.Append(result.Message);
            await M.Append("偷袭失败，你被对方发现了");
            Find.Game.ShowCurtain(10301);
        }
    }

    protected override async Task C2(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你想进行自裁");
        await M.Append("开始伤害检定....");
        if(Find.Player.GetAttributeLevel(Attributes.体力) >= 4)
        {
            await M.Append("你有无敌的体魄，是宇宙间最强的生物，猝死失败");
            await M.Append("你毫无畏惧，决定跟她深入交流");
            Find.Game.ShowCurtain(102);
            return;
        }
        else
        {
            await M.Append("你死了");
            Find.Game.ShowCurtain(curtainId);
        }
    }
}