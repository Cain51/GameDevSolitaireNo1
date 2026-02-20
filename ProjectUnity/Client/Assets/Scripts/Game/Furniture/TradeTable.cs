
public class TradeTable : Furniture
{
	public TradeTable(CABase ca) : base(ca)
	{
	}
	public override void OpenEventPanel()
	{
		UIManager uiManager = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
		if (uiManager == null) { return; }
		string panelName = ca != null && string.IsNullOrEmpty(ca.panel) == false ? ca.panel : "TradePanel";
		PanelBase panel = uiManager.OpenPanel(panelName);
		TradePanel tradePanel = panel as TradePanel;
		if (tradePanel != null)
		{
			tradePanel.SetEvent(null);
		}
	}
}
