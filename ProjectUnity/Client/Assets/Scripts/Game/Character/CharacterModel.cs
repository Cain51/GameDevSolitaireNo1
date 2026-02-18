using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterModel : MonoBehaviour, IPointerClickHandler
{
	public Character c;
	public int cid;

	public string[] randomAni;
	public float randomTime;
	private float curTime;
	private Animator animator;
	private void Start()
	{
		CharacterFactory cm = CBus.Instance.GetFactory(FactoryName.CharacterFactory) as CharacterFactory;
		c = cm.Produce(cid) as Character;
		curTime = randomTime;
		animator = GetComponent<Animator>();
		animator.OnOver(true, PlayNext);
	}

	public void PlayNext()
	{
		if (randomAni.Length == 0) { return; }
		if (animator == null) { return; }
		animator.Play(randomAni[Random.Range(0, randomAni.Length )], PlayNext);
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		DialogManager dm = CBus.Instance.GetManager(ManagerName.DialogManager) as DialogManager;
		dm.OnCharacterClick(c);
	}
}
