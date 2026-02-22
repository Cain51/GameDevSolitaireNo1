using RG.Zeluda;
using UnityEngine;
using UnityEngine.UI;

public class GroundPanel : PanelBase
{
	public GameObject pfb_item;
	public Transform tran_content;
	public Image img_bg;
	public Sprite[] mapBackgrounds;

	public void CreateGround(Ground g)
	{
		GameObject obj = GameObject.Instantiate(pfb_item);
		obj.transform.SetParent(tran_content);
		obj.SetActive(true);
		GroundItem item = obj.GetComponent<GroundItem>();
		g.view = item;
		item.Refresh(g);
	}

	public Sprite GetBackgroundSprite()
	{
		if (mapBackgrounds == null || mapBackgrounds.Length == 0) { return null; }
		SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
		if (slm == null || slm.mapCA == null) { return null; }
		string mapName = slm.mapCA.name;
		return GetBackgroundSpriteByName(mapName);
	}

	public Sprite GetBackgroundSpriteByName(string mapName)
	{
		if (mapBackgrounds == null || mapBackgrounds.Length == 0) { return null; }
		if (string.IsNullOrEmpty(mapName)) { return null; }
		if (mapName.StartsWith("你的")) { mapName = mapName.Substring(2); }
		mapName = mapName.Trim();
		for (int i = 0; i < mapBackgrounds.Length; i++)
		{
			Sprite sprite = mapBackgrounds[i];
			if (sprite != null && sprite.name.Trim() == mapName)
			{
				return sprite;
			}
		}
		Debug.LogWarning($"GroundPanel: Background sprite not found for map '{mapName}'");
		return null;
	}

	public void RefreshBackground()
	{
		if (img_bg == null) { return; }
		Sprite target = GetBackgroundSprite();
		if (target != null) { img_bg.sprite = target; }
	}
}
