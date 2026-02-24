using System.Threading.Tasks;
using Godot;

public class C124 : Curtain
{
    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("这里的气体，充满了焦虑，让人扁桃体发炎");
        await M.Append("伤害检定中...");
        await M.Append("由于你没有扁桃体，发炎伤害豁免...");

        if(Attr(Attributes.智力) <= 2)
        {
            await M.Append("你的智力并不足以理解，什么是焦虑。理智伤害豁免。");
            await M.Append("你感觉自己好像什么都没发生一样，一路冲进了车间。");
            Find.Game.ShowCurtain(128);//前往车间
            return;
        }
        else if(Attr(Attributes.美貌) >= 3)
        {
            await M.Append("你很美，但并没卵用。你还是很焦虑。");
        }
        await M.Append("由于焦虑，你感觉头晕晕的。 智力 -1");
        Find.Player.AddAttr(Attributes.智力, -1);
        Find.Player.Name = Find.Player.GetName(Find.Player.AttributeValues);
        Find.Player.ShowAttr();
        await M.Append("由于焦虑，你变的班味很重。 美貌 -1");
        Find.Player.AddAttr(Attributes.美貌, -1);
        Find.Player.Name = Find.Player.GetName(Find.Player.AttributeValues);
        Find.Player.ShowAttr();
        await M.Append("你现在是一个" + Find.Player.Name + "的大四学生");
        Find.Game.ShowCurtain(curtainId);
    }
}