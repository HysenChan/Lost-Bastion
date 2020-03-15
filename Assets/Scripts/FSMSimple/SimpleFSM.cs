using UnityEngine;
using System.Collections;
using System.Threading;

//仅用来巡逻
public class SimpleFSM : MonoBehaviour
{
    private Vector3[] positions;//巡逻点集合
    private int posIndex = 0;//当前巡逻的目的点编号
    public float turnRate = 1f;//巡逻变向间隔
    private float timer;//计时器

    public float PatrolDistance = 1f;//巡逻半径范围

    [HideInInspector]
    public bool isPatrol;//是否开始巡逻
    void Start()
    {
        //第一次初始化巡逻点
        positions = new Vector3[4];
        InitializePatrolPoint();


    }

    void Update()
    {
        //开始随机巡逻
        if (isPatrol)
        {
            timer += Time.deltaTime;
            if (timer >= turnRate)
            {
                posIndex = Random.Range(0, 4);
                timer = 0;
            }
            Patrol();
        }
        else
        {
            //每次脱离巡逻状态，都要以原地为基准点初始化巡逻点
            InitializePatrolPoint();
        }
    }

    //初始化巡逻点
    void InitializePatrolPoint()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = this.transform.localPosition;
        }

        positions[0].x += PatrolDistance;
        positions[0].z -= PatrolDistance;

        positions[1].x -= PatrolDistance;
        positions[1].z -= PatrolDistance;

        positions[2].x -= PatrolDistance;
        positions[2].z += PatrolDistance;

        positions[3].x += PatrolDistance;
        positions[3].z += PatrolDistance;
    }

    //巡逻
    void Patrol()
    {
        //移动
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, positions[posIndex], Time.deltaTime);

        //判断目的点和当前的位置关系，决定朝向
        if (positions[posIndex].x >= transform.localPosition.x)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}