using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartData : MonoBehaviour
{
    public static StartData inst;
    [Header("开局金币")]
    public int money = 10;
    [Header("体力")]
    public int power;
    [Header("初始资金")]
    public string[] item = new string[] { "2000:1" };
    [Header("商店物品")]
    public int[] shop = new int[] { 3001, 3002, 3003 };

    private void Awake()
    {
        inst = this;
    }
}
