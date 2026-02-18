#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorTools : ScriptableWizard {
    void OnWizardCreate() {
        if (被替换素材的名称 == string.Empty) {
            Debug.LogError("被替换素材的名称不能为空");
            return;
        }
        if (替换素材 == null) {
            Debug.LogError("替换素材不能为空");
            return;
        }
    
        GameObject obj = GameObject.Find("Environment");
        Exchange(obj);
    }
    private void Exchange(GameObject obj) {
        for (int i = 0; i < obj.transform.childCount; i++) {
            Transform tran = obj.transform.GetChild(i);
            if (tran.name.Contains(被替换素材的名称)) {
                GameObject random = 替换素材[UnityEngine.Random.Range(0, 替换素材.Length)];
                Mesh mesh = random.GetComponent<MeshFilter>().sharedMesh;
                Material[] materials = random.GetComponent<MeshRenderer>().sharedMaterials;

                tran.GetComponent<MeshFilter>().mesh = mesh;
                tran.GetComponent<MeshRenderer>().materials = materials;
            }
            Exchange(tran.gameObject);
        }
    }
    void OnWizardOtherButton() {
        Debug.Log("OnWizardOtherButton");
    }

    public string 被替换素材的名称 = string.Empty;
    public GameObject[] 替换素材;

    
}
#endif