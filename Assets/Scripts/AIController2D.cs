using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIController2D : MonoBehaviour, IDamagable
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
	[Header("AI")]
	[SerializeField] Transform[] waypoints;
	[SerializeField] float rayDistance = 1;
	[SerializeField] string enemyTag;
	[SerializeField] LayerMask raycastLayerMask;

	public float health = 100;

	Rigidbody2D rb;

	Vector2 velocity = Vector2.zero;
	bool faceRight = true;
	Transform targetWaypoint = null;
	GameObject playerGameObject = null;

	enum State
	{
		IDLE,
		PATROL,
		CHASE,
		ATTACK
	}

	State state = State.IDLE;
	float stateTimer = 1;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		// update AI
		CheckEnemySeen();

		Vector2 direction = Vector2.zero;
		switch (state)
		{
			case State.IDLE:
				{
					// if it has not seen the player it will not go into chase state if they are seen they will chase the player
					if (playerGameObject != null) state = State.CHASE;
					stateTimer -= Time.deltaTime;
					if (stateTimer <= 0)
					{
						// when the timer goes to zero it will set a new waypoint and patrol that section
						SetNewWaypointTarget();
						state = State.PATROL;
					}
					break;
				}
			case State.PATROL:
				{
					// if it has not seen the player it will not go into chase state if they are seen they will chase the player
					if (playerGameObject != null) state = State.CHASE;
					direction.x = Mathf.Sign(targetWaypoint.position.x - transform.position.x);
					float dx = Mathf.Abs(transform.position.x - targetWaypoint.position.x);
					if (dx <= 0.25f)
					{
						// goes to the idle state and sets the state timer to 1 
						state = State.IDLE;
						stateTimer = 1;
					}
					break;
				}
			case State.CHASE:
                {
					// if it does not see the player then is stays in the idle state
                    if (playerGameObject == null)
                    {
                        state = State.IDLE;
                        stateTimer = 1;
                        break;
                    }
					// finds the position of the player if found switch to attack until not seen anymore
                    float dx = Mathf.Abs(playerGameObject.transform.position.x - transform.position.x);
                    if (dx <= 1f)
                    {
						// sets the animator attack when the player is seen
                        state = State.ATTACK;
                        animator.SetTrigger("Attack");
                    }
                    else
                    {
                        direction.x = Mathf.Sign(playerGameObject.transform.position.x - transform.position.x);
                    }
					break;
                }
            case State.ATTACK:
				{
					// gets the current state of the enemy and or sees if it is in transition
                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
                    {
						// sets the animator to chase
                        state = State.CHASE;
                    }
					break;
                }
			default:
				break;
		}

		// check if on ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask) != null;

		velocity.x = direction.x * speed;
		
		// set velocity
		if (onGround)
		{

			if(velocity.y < 0) velocity.y = 0;
			if(Input.GetButtonDown("Jump"))
			{
				//velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
				//StartCoroutine(DoubleJump());
				//animator.SetTrigger("Jump");
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
				//velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
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

	private void SetNewWaypointTarget()
	{
		Transform waypoint = null;
		while(waypoint == targetWaypoint || !waypoint)
		{
			waypoint = waypoints[Random.Range(0, waypoints.Length)];
		}
		targetWaypoint = waypoint;
	}

    private void CheckEnemySeen()
    {
        playerGameObject = null;
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left), rayDistance, raycastLayerMask);
        if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag(enemyTag))
        {
            playerGameObject = raycastHit.collider.gameObject;
            Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance, Color.red);
        }
    }

	public void Damage(int damage)
	{
		health -= damage;
		print(health);
	}
}
