using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Inst;
	public AudioSource[] audios;
	public List<AudioSource> audioList;
	public List<Action> actionList = new List<Action>();
	public int index;
	private void Awake()
	{
		Inst = this;
		audios = GetComponents<AudioSource>();
		GameObject.DontDestroyOnLoad(gameObject);
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
