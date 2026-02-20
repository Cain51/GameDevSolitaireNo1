using DG.Tweening.Core.Easing;
using RG.Zeluda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntrance : MonoBehaviour
{
    void Start()
    {
        UIManager ui = CBus.Instance.GetManager(ManagerName.UIManager, true) as UIManager;
        if (ui != null)
        {
            ui.OpenPanel("LobbyPanel");
        }
    }

}
