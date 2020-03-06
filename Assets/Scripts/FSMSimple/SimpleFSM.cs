using UnityEngine;
using System.Collections;

namespace FSMSimple
{

    public class SimpleFSM : FSM
    {
        public enum FSMState
        {
            Patrol,
            Chase,
            Attack,
            Dead,
        }


        public float chaseDistance = 40.0f;
        public float attackDistance = 20.0f;
        public float arriveDistance = 3.0f;

        public Transform bulletSpawnPoint;
        public HealthSystem health;

        private Animator animator;

        //Current state that AI is reaching
        public FSMState curState;

        //speed of the AI
        //public float curSpeed = 120.0f;
        public float walkSpeed = 1.0f;
        public float runSpeed = 160.0f;

        //Rotation Speed
        public float curRotSpeed = 6.0f;

        //Bullet
        //public GameObject Bullet;

        //Whether the AI is destroyed or not
        private bool bDead;

        //Initialize the Finite State machine for AI
        protected override void Initialize()
        {
            curState = FSMState.Patrol;

            bDead = false;

            //Get the list of patrol points
            pointList = GameObject.FindGameObjectsWithTag("PatrolPoint");

            //Get animation
            animator = GetComponentInChildren<Animator>();
            health = GetComponent<HealthSystem>();
            print(animator);
            //Set Random destination point first
            FindNextPoint();

            //Get target(player)
            GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");

            playerTransform = objPlayer.transform;

            if (!playerTransform)
                print("找不到名为'Player'的Tag！");

        }


        protected override void FSMUpdate()
        {
            switch (curState)
            {
                case FSMState.Patrol:
                    UpdatePatrolState();
                    break;
                case FSMState.Chase:
                    UpdateChaseState();
                    break;
                case FSMState.Attack:
                    UpdateAttackState();
                    break;
                case FSMState.Dead:
                    UpdateDeadState();
                    break;
            }

            //Go to dead state when no health left
            if ( health.CurrentHp <= 0)
                curState = FSMState.Dead;
        }

        //巡逻
        protected void UpdatePatrolState()
        {
            //Find another random patrol point if current point is reached
            //check distance from player, when AI is near the player, change to chase state
            if (Vector3.Distance(transform.position, destPos) <= attackDistance)
            {
                print("Reached to the destination point, calculating the next point");
                FindNextPoint();
            }
            else if (Vector3.Distance(transform.position, playerTransform.position) <= chaseDistance)
            {
                print("Switch to chase state");
                curState = FSMState.Chase;
            }

            //计算目标方向
            Vector3 toTarget = GetDirection2D(destPos, transform.position);

            //当前物体的朝向
            Vector3 curFacing = transform.right;

            //计算物体与目标方向的点积（是否同一方向）
            float dotV = Vector3.Dot(toTarget, curFacing);

            //求出两向量的角度 （>90,方向相反， =90，垂直  <90,同向）
            float angle = Mathf.Acos(Mathf.Clamp(dotV, -1f, 1f)) * Mathf.Rad2Deg;

            if (angle <= 90)
            {
                //animator.SetTrigger("Walk");
                animator.SetFloat("MovementSpeed", 1);
            }
            else
            {
                //animator.SetTrigger("Walk");
                animator.SetFloat("MovementSpeed", -1);
            }

            transform.position = transform.position + toTarget * Time.deltaTime;
        }

        //追逐
        protected void UpdateChaseState()
        {
            //Set target position as the player position
            destPos = playerTransform.position;

            //Check the distance with player when in a range, change to attack state
            //Go back to patrol if it becomes too far
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist <= attackDistance)
            {
                curState = FSMState.Attack;
            }
            else if (dist >= chaseDistance)
            {
                curState = FSMState.Patrol;
            }

            transform.position = transform.position + (destPos- transform.position) * Time.deltaTime;

            //animator.SetFloat("MovementSpeed", 1);
            //animComponent.CrossFade("Run");

        }

        protected void UpdateAttackState()
        {
            //    Quaternion targetRotation;

            //Set target position as the player position
            destPos = playerTransform.position;

            //Check the distance with player when in a range, change to attack state
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist >= attackDistance && dist < chaseDistance)
            {
                curState = FSMState.Chase;//FSMState.Attack;
                return;
            }
            else if (dist >= chaseDistance)  //change to patrol state if too far
            {
                curState = FSMState.Patrol;
                return;
            }

            //    targetRotation = Quaternion.LookRotation(destPos - transform.position);
            //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

            //animComponent.CrossFade("StandingFire");
        }

        protected void UpdateDeadState()
        {
            //Play dead animation
            if (!bDead)
            {
                bDead = true;
                animator.SetBool("isDead", true);
            }
        }


        void onCollisionEnter(Collision collision)
        {
            //Reduce health
            if (collision.gameObject.tag == "Bullet")
            {
                //health -= collision.gameObject.GetComponent<Bullet>().damage;
            }
        }


        protected void FindNextPoint()
        {
            print("Finding next point");
            int rndIndex = Random.Range(0, pointList.Length);
            destPos = pointList[rndIndex].transform.position;
        }




        protected Vector3 GetDirection2D(Vector3 to, Vector3 from)
        {
            to = new Vector3(to.x, 0, to.z);
            from = new Vector3(from.x, 0, from.z);
            return (to - from).normalized;
        }

    }
}