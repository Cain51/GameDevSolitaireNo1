using UnityEngine;
using UnityEditor;
using System.Data;
using System.Collections.Generic;

public class DataCenterViewerWindow : EditorWindow
{
    private string[] tableNames;
    private int selectedTableIndex = 0;
    private Vector2 scrollPos;

    [MenuItem("Tools/DataCenter 数据查看器")]
    public static void ShowWindow()
    {
        GetWindow<DataCenterViewerWindow>("DataCenter 数据查看器");
    }

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("请在游戏运行时使用。", MessageType.Info);
            return;
        }

        if (DataCenter.data == null || DataCenter.data.Count == 0)
        {
            EditorGUILayout.HelpBox("DataCenter 尚未加载任何数据。", MessageType.Warning);
            if (GUILayout.Button("手动初始化 DataCenter"))
            {
                DataCenter.InitWWW();
            }
            return;
        }

        // 获取所有表名
        if (tableNames == null || tableNames.Length != DataCenter.data.Count)
        {
            var keys = new List<string>(DataCenter.data.Keys);
            tableNames = keys.ToArray();
        }

        // 表选择
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("选择数据表：", GUILayout.Width(80));
        selectedTableIndex = EditorGUILayout.Popup(selectedTableIndex, tableNames);
        EditorGUILayout.EndHorizontal();

        if (tableNames.Length == 0) return;

        string tableName = tableNames[selectedTableIndex];
        var table = DataCenter.data[tableName];

        if (table == null || table.Count == 0)
        {
            EditorGUILayout.LabelField("该表无数据。");
            return;
        }

        // 获取列名
        DataRow firstRow = null;
        foreach (var row in table.Values)
        {
            firstRow = row;
            break;
        }
        if (firstRow == null)
        {
            EditorGUILayout.LabelField("该表无数据。");
            return;
        }

        var columns = new List<string>();
        foreach (DataColumn col in firstRow.Table.Columns)
        {
            columns.Add(col.ColumnName);
        }

        // 数据展示
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // 表头
        EditorGUILayout.BeginHorizontal();
        foreach (var col in columns)
        {
            EditorGUILayout.LabelField(col, EditorStyles.boldLabel, GUILayout.Width(100));
        }
        EditorGUILayout.EndHorizontal();

        // 数据行
        foreach (var kv in table)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var col in columns)
            {
                string val = kv.Value[col]?.ToString() ?? "";
                EditorGUILayout.LabelField(val, GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
