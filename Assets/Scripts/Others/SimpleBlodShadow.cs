using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBlodShadow : MonoBehaviour
{
    public Transform FollowBone;
    public float BlobShadowSize = 1;
    public float DistanceScale = 2f; //阴影的大小乘数相对于与地面的距离
    public float Yoffset = 0; //阴影的偏移量
    public LayerMask GroundLayerMask;
    public bool followTerrainRotation = true;
    private float rayDist = 10f; //射线距离

    private void Update()
    {
        if (FollowBone!=null)
        {
            RaycastHit hit;
            if (Physics.Raycast(FollowBone.transform.position, Vector3.down * rayDist, out hit, rayDist, GroundLayerMask)) 
            {
                //显示blobshadow（如果碰到了东西）
                GetComponent<MeshRenderer>().enabled = true;

                //设置位置
                SetPosition(hit);

                //设置缩放
                SetScale(FollowBone.transform.position.y - hit.point.y);

                //将Blobshadow旋转设置为正常
                if (followTerrainRotation)
                {
                    SetRotation(hit.normal);
                }
            }
        }
        else
        {
            //隐藏阴影
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    /// <summary>
    /// 设置阴影位置
    /// </summary>
    /// <param name="hit"></param>
    private void SetPosition(RaycastHit hit)
    {
        transform.position = hit.point + Vector3.up * Yoffset;
    }

    /// <summary>
    /// 将阴影旋转设置为地板角度
    /// </summary>
    /// <param name="normal"></param>
    void SetRotation(Vector3 normal)
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

    /// <summary>
    /// 设置阴影大小
    /// </summary>
    /// <param name="floorDistance"></param>
    private void SetScale(float floorDistance)
    {
        float scaleMultiplier = floorDistance / DistanceScale;
        float size = BlobShadowSize + scaleMultiplier;
        transform.localScale = new Vector3(size, size, size);
    }
}
