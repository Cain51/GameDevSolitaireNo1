using System.Threading.Tasks;
using Godot;

public class C112 : Curtain
{
    protected override async Task C0(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你转身要跑");
        await M.Append("逃跑检定中....");

        if(Attr(Attributes.体力) <= 2)
        {
            await M.Append("但是你身体弱得一批，也并没有鹿腿。");
            await M.Append("她。抓到你了");
        }else if(Attr(Attributes.体力) >= 3)
        {
            await M.Append("你体魄强健，即使没有鹿腿，也跑的飞快");
            await M.Append("可是你怎么跑，她都在你面前");
        }else if(Attr(Attributes.武德) >= 3)
        {
            await M.Append("你高尚的武德，不允许你逃跑。");
            await M.Append("你正面刚了上去");
        }
        Find.Game.ShowCurtain(curtainId);
    }

    protected override async Task C1(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你想要感化她");
        await M.Append("魅力检定中....");

        if(Attr(Attributes.美貌) <= 2)
        {
            await M.Append("你太丑了，对方拒绝交流。并啐了一口。");
        }else if(Attr(Attributes.美貌) >= 3)
        {
            await M.Append("前妻看着楚楚可怜的你，心软了，决定给你一个痛快。");
        }else if(Attr(Attributes.武德) >= 3)
        {
            await M.Append("你高尚的武德，不允许你求饶。");
            await M.Append("你正面刚了上去");
        }
        Find.Game.ShowCurtain(curtainId);
    }

    protected override async Task C2(int curtainId)
    {
        var M = Find.Game.EventCheckMessage;
        await M.Show("你脑子一片空白，跪了下去。");
        await M.Append("魅力检定中....");

        if(Attr(Attributes.体力) <= 2)
        {
            await M.Append("你脆弱的膝盖，碎了。你再也站不起来了。");
            await M.Append("你眼睁睁的看着她向你走来。");
        }else if(Attr(Attributes.武德) >= 3)
        {
            await M.Append("你高尚的武德，不允许你求饶。");
            await M.Append("你正面刚了上去");
        }
        Find.Game.ShowCurtain(curtainId);
    }
}