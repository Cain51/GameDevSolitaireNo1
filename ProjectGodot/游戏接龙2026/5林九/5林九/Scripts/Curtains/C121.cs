using System.Threading.Tasks;
using Godot;

public class C121 : Curtain
{
    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你打算直接莽上去。");
        await M.Append("攻击检定中...");
        await M.Append("由于你没有扁桃体，发炎伤害豁免...");

        if(Attr(Attributes.体力) <= 2)
        {
            await M.Append("别闹了，没有虎哥，你根本走不了路。");
            await M.Append("关门放虎哥！");
            Find.Game.ShowCurtain(123);
            return;
        }
        else if(Attr(Attributes.智力) >= 3)
        {
            await M.Append("你觉得直接莽上去不是个好主意。");
            await M.Append("但是虎哥已经上了。");
            Find.Game.ShowCurtain(123);
        }
        Find.Game.ShowCurtain(curtainId);
    }
}