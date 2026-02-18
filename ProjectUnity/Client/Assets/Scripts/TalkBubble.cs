 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkBubble : MonoBehaviour
{
    public SpriteRenderer talk;
    public GameObject target;
    protected ResManager _resManager;
    private string _path;
    public void SetRes(string res)
    {
        _resManager.UnloadRes(_path);
        talk.sprite = _resManager.GetRes<Sprite>(res);
        _path = res;
        transform.position = target.transform.position;
        gameObject.SetActive(true);
    }
    public void Init()
    {
        _resManager = CBus.Instance.GetManager("ResManager") as ResManager;

    }
    private Vector3 offset = new Vector3(0, 0, -1);
    private void Update()
    {
        if (target == null) { return; }
        transform.position = target.transform.position + offset;
    }
    public void UnActive()
    {
        _resManager.UnloadRes(_path);
        gameObject.SetActive(false);
    }
}
