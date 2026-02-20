using RG.Zeluda;
using UnityEngine;
using UnityEngine.UI;

public class BuildItem : MonoBehaviour
{
    public int index;
    public Text lbl_gr;
    public Text lbl_name;
    public Image img_icon;
    public Asset model;

    public void Init(int idx, Asset m)
    {
        index = idx;
        model = m;
        lbl_name.text = model.ca.name;
    }
    public void OnClick()
    {
		if (model == null || model.ca == null) { return; }
		AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
		if (am == null) { return; }
		int cost = model.ca.cost;
		if (cost > 0 && am.CheckCoint(cost) == false)
		{
			TipManager.Tip("金币不足");
			return;
		}
		if (cost > 0)
		{
			am.RemoveCoin(cost);
		}
		am.Add(model.ca.id, 1);
		TipManager.Tip($"获得{model.ca.name}");
    }
}
