using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineItem : MonoBehaviour
{
    public Text lbl_index;
    public GameObject btn_over;
    public Transform tran_content;

    public void OverDay() {
        btn_over.SetActive(false);
    }
}
