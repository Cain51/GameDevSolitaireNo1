using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Inst;
	public AudioSource[] audios;
	public List<AudioSource> audioList;
    private AudioSource bgmSource; // 专用的 BGM 音源
	public List<Action> actionList = new List<Action>();
	public int index;
	private void Awake()
	{
		Inst = this;
		audios = GetComponents<AudioSource>();
        
        // 初始化 BGM 音源
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

		GameObject.DontDestroyOnLoad(gameObject);
	}

    // 播放背景音乐
    public void PlayBGM(string name, float volume = 0.2f)
    {
        AudioClip clip = Resources.Load<AudioClip>(name);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] 找不到 BGM 资源: {name}");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.Play();
        Debug.Log($"[AudioManager] 正在播放 BGM: {name}");
    }

    // 停止背景音乐
    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    // 设置 BGM 音量
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = volume;
    }

	public void Play(AudioClip audioClip, Action a = null)
	{
		if (audioClip == null) { return; }

        AudioSource audio = audios[index];
		if (audioList.Contains(audio))
		{
			int index = audioList.IndexOf(audio);
			audioList.RemoveAt(index);
			Action ac = actionList[index];
			ac.Invoke();
			actionList.RemoveAt(index);
		}

		audio.clip = audioClip;
		audio.Play();
		if (a != null)
		{
			audioList.Add(audio);
			actionList.Add(a);
		}
		index = (index + 1) % audios.Length;
	}
	public void Play(string name, Action a = null)
	{
		AudioClip audioClip = Resources.Load<AudioClip>(name);
		if (audioClip == null) { return; }

        AudioSource audio = audios[index];
		if (audioList.Contains(audio))
		{
			int index = audioList.IndexOf(audio);
			audioList.RemoveAt(index);
			Action ac = actionList[index];
			ac.Invoke();
			actionList.RemoveAt(index);
		}

		audio.clip = audioClip;
		audio.Play();
		if (a != null) {
			audioList.Add(audio);
			actionList.Add(a);
		}
		index = (index + 1) % audios.Length;
	}
	private void Update()
	{
		int cnt = audioList.Count;
		for (int i = cnt - 1; i >= 0; i--)
		{
			if (audioList[i].clip == null) { continue; }
			if (audioList[i].isPlaying == false)
			{
				audioList[i].clip = null;
				Action ac = actionList[i];
				ac.Invoke();
				audioList.RemoveAt(i);
				actionList.RemoveAt(i);
			}
		}
	}
}
