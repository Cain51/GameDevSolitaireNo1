using DBUtility;
using RG.Basic;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;

public static class DataCenter
{

    public static string dataPath = "/Data";

    public static Map<string, Map<int, DataRow>> data;
    public static void Init()
    {
        data = new Map<string, Map<int, DataRow>>();
        DirectoryInfo info = new DirectoryInfo(Application.streamingAssetsPath + dataPath);
        FileInfo[] csvs = info.GetFiles("*.csv");

        Map<int, DataRow> rowData;
        for (int idx = 0; idx < csvs.Length; idx++)
        {
            DataTable rel = CsvHelper.OpenCSV(csvs[idx].FullName, 2);
            rowData = new Map<int, DataRow>();
            for (int i = 0; i < rel.Rows.Count; i++)
            {
                try
                {
                    rowData.Add(int.Parse(rel.Rows[i]["cid"].ToString()), rel.Rows[i]);
                }
                catch { }
            }
            data.Add(csvs[idx].Name.Replace(".csv", ""), rowData);
           
        }
    }
    public static string[] ResAry = new string[] { "Achievement" , "Enemy", "Hero", "MonterBo", "Skill", "Talent" };
    public static void InitWWW()
    {
        data = new Map<string, Map<int, DataRow>>();
        //DirectoryInfo info = new DirectoryInfo(Application.streamingAssetsPath + dataPath);
        //FileInfo[] csvs = info.GetFiles("*.csv");
      
        Map<int, DataRow> rowData;
        for (int idx = 0; idx < ResAry.Length; idx++)
        {
            TextAsset t = Resources.Load<TextAsset>("Data/"+ResAry[idx]);
            Stream s = new MemoryStream(ASCIIEncoding.Default.GetBytes(t.text));
            DataTable rel = CsvHelper.OpenCSV(s, 2);
            rowData = new Map<int, DataRow>();
            for (int i = 0; i < rel.Rows.Count; i++)
            {
                try
                {
                    rowData.Add(int.Parse(rel.Rows[i]["cid"].ToString()), rel.Rows[i]);
                }
                catch { }
            }
            data.Add(ResAry[idx], rowData);

        }
    }
    public static Map<int, DataRow> GetTable(string n)
    {
        if (data == null)
        {
            InitWWW();
        }
        return data[n];
    }
    public static DataRow GetData(string n, int id)
    {
        if (data == null)
        {
            InitWWW();
        }
        if (!data[n].ContainsKey(id))
        {
            return null;
        }
        return data[n][id];
    }
    public static List<int> GetTableIDList(string tableName)
    {
        DataTable rel = CsvHelper.OpenCSV(Application.dataPath + dataPath + "\\" + tableName + ".csv", 2);
        List<int> IDList = new List<int>();
        for (int i = 0; i < rel.Rows.Count; i++)
        {
            try
            {
                IDList.Add(int.Parse(rel.Rows[i]["cid"].ToString()));
            }
            catch { }
        }
        return IDList;
    }

}
