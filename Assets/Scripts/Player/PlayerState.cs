using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public PLAYERSTATE currentState = PLAYERSTATE.IDLE;

    /// <summary>
    /// 设置玩家状态
    /// </summary>
    /// <param name="state"></param>
    public void SetState(PLAYERSTATE state)
    {
        currentState = state;
    }
}

public enum PLAYERSTATE
{
    IDLE,   //闲置状态
    WALK,   //行走
    JUMPING,    //跳
    LAND,   //落地
    JUMPKICK,   //跳踢
    PUNCH,//拳击
    KICK,//踢
    ATTACK, //攻击
    DEFEND,//防御
    HIT,//击中
    DEATH,//死亡
    THROW,//扔
    PICKUPITEM,//抬起物体
    KNOCKDOWN,//击倒
    KNOCKDOWNGROUNDED,//击倒接地
    GROUNDPUNCH,//重拳
    GROUNDKICK,//重腿
    GROUNDHIT,//重击
    STANDUP,//站立状态
    USEWEAPON,//使用武器
};
