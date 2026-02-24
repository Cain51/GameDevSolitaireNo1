using System.Threading.Tasks;
using Godot;

public class C115 : Curtain
{
    bool mark = false;

    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你失去扁桃体，嗓子里中空落落的");
        await M.Append("伤害检定中...");
        if(mark is false)
        {
            await M.Append("你的扁桃体，再也不会发炎了，你感觉身体好多了。");
            await M.Append("体力+1");
            mark = true;
            Find.Game.Player.AddAttr(Attributes.体力, 1);
            Find.Game.Player.Name = Find.Game.Player.GetName(Find.Game.Player.AttributeValues);
            Find.Player.ShowAttr();
            await M.Append("你现在是一个" + Find.Player.Name + "的大四学生");
        }
        Find.Game.ShowCurtain(curtainId);
    }
}