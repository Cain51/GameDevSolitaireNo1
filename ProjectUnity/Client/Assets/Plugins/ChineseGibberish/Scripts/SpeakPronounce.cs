using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PinYinSpell;

public class SpeakPronounce : MonoBehaviour
{
    PronounceCore core;

    // 汉字转拼音
    public void OnConvert(string dialog)
    {
        string t = core.ConvertPinYin(dialog);
        OnSpeak(t);

	}

    // 播放拼音
    public void OnSpeak(string pinyin)
    {
        core.Speak(pinyin);
    }

    // 转换并播放
    public void ConvertAndSpeak(string dialog)
    {
		core = GetComponent<PronounceCore>();
        core.Init();
		string py = core.ConvertPinYin(dialog);
        core.Speak(py);
    }
}
