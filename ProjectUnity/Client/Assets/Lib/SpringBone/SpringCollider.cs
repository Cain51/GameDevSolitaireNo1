using UnityEngine;
using System.Collections;

namespace RG.Unity5.SpringBone
{
	public class SpringCollider : MonoBehaviour
	{
		//°ë¾¶
		public float radius = 0.5f;

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
}