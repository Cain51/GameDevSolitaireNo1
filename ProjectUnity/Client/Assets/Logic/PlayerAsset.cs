using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;

public class PlayerAsset
{
    public Dictionary<int, int> itemCnt = new Dictionary<int, int>();
    public List<Asset> models = new List<Asset>();
    public int CheckItem(int id)
    {
        int cnt = 0;
        foreach (var item in models)
        {
            if (item.ca.id == id)
            {
                cnt++;
            }
        }
        if (cnt > 0) { return cnt; }
        if (itemCnt.ContainsKey(id) == false) { return 0; }
        return itemCnt[id];
    }
    public void AddItem(int id, int cnt = 1)
    {
        AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
        if (id > 2000 && id < 3000)
        {
			Asset asset = assetFactory.Produce(id) as Asset;
			models.Add(asset);
            return;
        }
        if (itemCnt.ContainsKey(id))
        {
            itemCnt[id]++;
        }
        else
        {
            itemCnt.Add(id, cnt);
        }
    }
    public bool RemoveItem(CABase data, int cnt = 1)
    {
        if (data is AssetCA)
        {
            bool enable = false;
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].ca == data)
                {
                    models.RemoveAt(i);
                    enable = true;
                    break;
                }
            }
            return enable;
        }

        return RemoveItem(data.id, cnt);
    }
    public bool RemoveItem(int id, int cnt = 1)
    {
        if (id > 2000 && id < 3000)
        {
            bool enable = false;
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].ca.id == id)
                {
                    models.RemoveAt(i);
                    enable = true;
                    break;
                }
            }
            return enable;
        }

        if (itemCnt.ContainsKey(id) == false)
        {
            return false;
        }
        else if (itemCnt[id] < cnt)
        {
            return false;
        }
        itemCnt[id] -= cnt;
        return true;
    }
}
