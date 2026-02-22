using DBUtility;
using System.Data;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DataCenterCsvEditor : EditorWindow
{
    private string csvPath = "";
    private DataTable csvTable;
    private Vector2 scrollPos;
    private bool isDirty = false;
    private Vector2 mapScrollPos;

    [MenuItem("Tools/DataCenter CSV编辑器")]
    public static void ShowWindow()
    {
        GetWindow<DataCenterCsvEditor>("CSV编辑器");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV文件路径", EditorStyles.boldLabel);
        csvPath = EditorGUILayout.TextField(csvPath);

        if (GUILayout.Button("选择CSV文件"))
        {
            string path = EditorUtility.OpenFilePanel("选择CSV", Application.streamingAssetsPath + DataCenter.dataPath, "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvPath = path;
                LoadCsv();
            }
        }

        if (csvTable != null)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.LabelField("CSV内容", EditorStyles.boldLabel);

            // 显示表头
            EditorGUILayout.BeginHorizontal();
            foreach (DataColumn col in csvTable.Columns)
                EditorGUILayout.LabelField(col.ColumnName, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // 显示数据行
            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < csvTable.Columns.Count; j++)
                {
                    string val = csvTable.Rows[i][j].ToString();
                    string newVal = EditorGUILayout.TextField(val, GUILayout.Width(100));
                    if (newVal != val)
                    {
                        csvTable.Rows[i][j] = newVal;
                        isDirty = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("保存并覆盖CSV文件") && isDirty)
            {
                SaveCsv();
                isDirty = false;
                EditorUtility.DisplayDialog("保存成功", "CSV文件已覆盖保存", "确定");
            }
        }

        // 集成CBus地图数据调试
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("地图数据调试（CBus）", EditorStyles.boldLabel);
        if (Application.isPlaying)
        {
            var mapFactory = CBus.Instance.GetFactory("MapFactory") as MapFactory;
            if (mapFactory != null)
            {
                var allMaps = mapFactory.GetAllCA();
                if (allMaps != null && allMaps.Length > 0)
                {
                    mapScrollPos = EditorGUILayout.BeginScrollView(mapScrollPos, GUILayout.Height(200));
                    // 表头
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("id", GUILayout.Width(60));
                    EditorGUILayout.LabelField("name", GUILayout.Width(120));
                    EditorGUILayout.LabelField("scene", GUILayout.Width(120));
                    EditorGUILayout.LabelField("unlockday", GUILayout.Width(80));
                    EditorGUILayout.LabelField("opentime", GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();

                    // 数据行
                    foreach (var ca in allMaps)
                    {
                        var map = ca as MapCA;
                        if (map == null) continue;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(map.id.ToString(), GUILayout.Width(60));
                        EditorGUILayout.LabelField(map.name, GUILayout.Width(120));
                        EditorGUILayout.LabelField(map.scene, GUILayout.Width(120));
                        EditorGUILayout.LabelField(map.unlockday.ToString(), GUILayout.Width(80));
                        EditorGUILayout.LabelField(string.Join(",", map.opentime), GUILayout.Width(120));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.LabelField("未找到任何地图数据。");
                }
            }
            else
            {
                EditorGUILayout.LabelField("MapFactory未初始化或未注册。");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("地图数据调试需在游戏运行时使用。", MessageType.Info);
        }
    }

    void LoadCsv()
    {
        csvTable = CsvHelper.OpenCSV(csvPath);
        isDirty = false;
    }

    void SaveCsv()
    {
        StringBuilder sb = new StringBuilder();
        // 写表头
        for (int i = 0; i < csvTable.Columns.Count; i++)
        {
            sb.Append(csvTable.Columns[i].ColumnName);
            if (i < csvTable.Columns.Count - 1) sb.Append(",");
        }
        sb.AppendLine();
        // 写数据
        for (int i = 0; i < csvTable.Rows.Count; i++)
        {
            for (int j = 0; j < csvTable.Columns.Count; j++)
            {
                sb.Append(csvTable.Rows[i][j].ToString());
                if (j < csvTable.Columns.Count - 1) sb.Append(",");
            }
            sb.AppendLine();
        }
        File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
    }
}
