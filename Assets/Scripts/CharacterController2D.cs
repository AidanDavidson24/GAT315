using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
	[SerializeField] Animator animator;
	[SerializeField] SpriteRenderer spriteRenderer;
	[SerializeField] float speed;
	[SerializeField] float jumpHeight;
	[SerializeField] float doubleJumpHeight;
	[SerializeField, Range(1, 5)] float fallRateMultiplier;
	[SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;
	[SerializeField] float groundRadius;
	[Header("Attack")]
	[SerializeField] Transform attackTransform;
	[SerializeField] float attackRadius;

	Rigidbody2D rb;
	Vector2 velocity = Vector2.zero;
	bool faceRight = true;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		// check if on ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask) != null;

		// get direction input
		Vector2 direction = Vector2.zero;
		direction.x = Input.GetAxis("Horizontal");

		velocity.x = direction.x * speed;
		
		// set velocity
		if (onGround)
		{

			if(velocity.y < 0) velocity.y = 0;
			if(Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
				StartCoroutine(DoubleJump());
				animator.SetTrigger("Jump");
			}

			if(Input.GetMouseButtonDown(0))
			{
				animator.SetTrigger("Attack");
			}
		}

		// adjust gravity for jump
		float gravityMultiplier = 1;
		if (!onGround && velocity.y < 0) gravityMultiplier = fallRateMultiplier;
		if (!onGround && velocity.y > 0 && !Input.GetButton("Jump")) gravityMultiplier = lowJumpRateMultiplier;

		velocity.y += Physics.gravity.y * Time.deltaTime;

		// move character
		rb.velocity = velocity;

		//rotate character to face direction of movement
		if (velocity.x > 0 && !faceRight) Flip();
		if (velocity.x < 0 && faceRight) Flip();

		// update animator
		animator.SetFloat("Speed", Mathf.Abs(velocity.x));
		animator.SetBool("Fall", !onGround && velocity.y < -0.1f);
		
	}
		
	IEnumerator DoubleJump()
	{
		// wait a little after the jump to allow a double jump
		yield return new WaitForSeconds(0.01f);
		// allow a double jump while moving up
		while(velocity.y > 0)
		{
			// if "jump" pressed add jump velocity
			if(Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
				break;
			}
			yield return null;
		}
	}

	private void Flip()
	{
		faceRight = !faceRight;
		spriteRenderer.flipX = !faceRight;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);
	}

	private void CheackAttack()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius);
		foreach(Collider2D collider in colliders)
		{
			if (collider.gameObject == gameObject) continue;

            if (collider.gameObject.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.Damage(10);
            }
        }
	}
}
