#if UNITY_EDITOR
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
        var tableMap = DataCenter.data[tableName];

        if (tableMap == null || tableMap.Count == 0)
        {
            EditorGUILayout.HelpBox("该表目前没有数据或尚未加载。", MessageType.Info);
            return;
        }

        // 显示表格内容
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        // 获取第一行来确定列信息
        DataRow firstRow = null;
        foreach (var key in tableMap.Keys) 
        { 
            firstRow = tableMap[key]; 
            break; 
        }

        if (firstRow != null)
        {
            // 表头
            EditorGUILayout.BeginHorizontal();
            foreach (DataColumn col in firstRow.Table.Columns)
            {
                EditorGUILayout.LabelField(col.ColumnName, EditorStyles.boldLabel, GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();

            // 数据行
            foreach (var key in tableMap.Keys)
            {
                DataRow row = tableMap[key];
                EditorGUILayout.BeginHorizontal();
                foreach (var item in row.ItemArray)
                {
                    EditorGUILayout.TextField(item.ToString(), GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif
