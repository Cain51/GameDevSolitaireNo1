using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterController : MonoBehaviour
{
	public static CharacterController Inst;
	public float moveSpeed = 3.5f; // 移动速度
	public NavMeshAgent agent;
	public Animator ani;
	public bool moveLock = false;
	private void Awake()
	{
		Inst = this;
	}
	void Start()
	{
		ani = GetComponent<Animator>();
		agent = GetComponent<NavMeshAgent>();
		agent.speed = moveSpeed;
	}

	void Update()
	{
		if (moveLock == true) { return; }
		MoveWithWASD();
	}

	void MoveWithWASD()
	{
		// 获取输入
		float horizontal = Input.GetAxis("Horizontal"); // A和D键
		float vertical = Input.GetAxis("Vertical"); // W和S键
		if (horizontal != 0 || vertical != 0)
		{
			ani.Play("move");
			Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

			if (direction.magnitude >= 0.1f)
			{
				// 计算目标位置
				Vector3 targetPosition = transform.position + direction * moveSpeed * Time.deltaTime;

				// 设置 NavMeshAgent 的目标位置
				agent.SetDestination(targetPosition);
			}
		}
		else {
			ani.Play("idle");
		}
		
	}
}
