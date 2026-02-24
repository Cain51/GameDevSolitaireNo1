using System.Threading.Tasks;
using Godot;

public class C10301 : Curtain
{
    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("两团白色占满了你的双眼，你被击中了");
        await M.Append("开始伤害检定....");
        if(Find.Player.GetAttributeLevel(Attributes.体力) >= 4)
        {
            await M.Append("你有无敌的体魄，是宇宙间最强的生物，你毫发无伤");
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