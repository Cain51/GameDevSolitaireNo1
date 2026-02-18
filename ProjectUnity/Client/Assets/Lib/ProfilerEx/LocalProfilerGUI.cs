using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalProfilerGUI : MonoBehaviour
{
    string stringToEdit = "";
    private void OnGUI()
    {
        stringToEdit = GUI.TextField(new Rect(20, 130, 150, 20), stringToEdit);
        if (GUI.Button(new Rect(20, 150, 150, 50), "性能Log"))
        {
            LocalProfilerTools.LogAllFieldTable(CBus.Instance, stringToEdit);
        }
    }
}
