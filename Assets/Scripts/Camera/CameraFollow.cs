using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;//相机要跟随的目标，这是Player

    [Header("Follow Settings")]
    public float heightOffset = 5;//相机相对于目标的高度偏移
    public float distanceToTarget = 5;//相机到目标的距离(z轴)
    public float viewAngle = 10;//向下旋转
    public bool FollowZAxis;//沿z轴启用或禁用相机
    public Vector3 AdditionalOffset;//任何其他偏移

    /// <summary>
    /// 阻尼设定
    /// </summary>
    [Header("Damp Settings")]
    public float DampX = 3f;
    public float DampY = 3f;
    public float DampZ = 3f;

    [Header("View Area")]
    public float MinLeft;
    public float MaxRight;

    [Header("Wave Area Collider")]
    public bool UseWaveAreaCollider;
    public BoxCollider CurrentAreaCollider;
    public float AreaColliderViewOffset;

    private void Start()
    {
        //设置玩家作为相机的跟随目标
        if (!target)
        {
            SetPlayerAsTarget();
        }

        if (target)
        {
            Vector3 playerPos = target.transform.position;
            transform.position = new Vector3(playerPos.x, playerPos.y - heightOffset, playerPos.z + distanceToTarget);
        }
    }

    private void Update()
    {
        if (target)
        {
            //初始化数据
            float currentX = transform.position.x;
            float currentY = transform.position.y;
            float currentZ = transform.position.z;
            Vector3 playerPos = target.transform.position;

            currentX = Mathf.Lerp(currentX, playerPos.x, DampX * Time.deltaTime);
            currentY = Mathf.Lerp(currentY, playerPos.y-heightOffset, DampY * Time.deltaTime);
            if (FollowZAxis)
            {
                currentZ = Mathf.Lerp(currentZ, playerPos.z+distanceToTarget, DampZ * Time.deltaTime);
            }
            else
            {
                currentZ = distanceToTarget;
            }

            //设置相机位置
            if (CurrentAreaCollider==null)
            {
                UseWaveAreaCollider = false;
            }

            if (!UseWaveAreaCollider)
            {
                transform.position = new Vector3(Mathf.Clamp(currentX, MaxRight, MinLeft), currentY, currentZ) + AdditionalOffset;
            }
            else
            {
                transform.position = new Vector3(Mathf.Clamp(currentX, CurrentAreaCollider.transform.position.x + AreaColliderViewOffset, MinLeft), currentY, currentZ) + AdditionalOffset;
            }

            //设置相机的旋转
            transform.rotation = new Quaternion(0, 180f, viewAngle, 0);
        }
    }

    /// <summary>
    /// 设置玩家作为目标
    /// </summary>
    void SetPlayerAsTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player!=null)
        {
            target = player.transform;
        }
    }
}
