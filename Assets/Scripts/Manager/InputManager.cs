using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //战斗类型
    public enum COMBATACTION
    {
        NONE,
        PUNCH,
        KICK,
        JUMP,
        DEFEND,
        WEAPONATTACK,
    };

    //判断是否为键盘输入，方便后期扩展
    public bool UseKeyboardInput;

    [Header("Keyboard keys")]
    public KeyCode Up = KeyCode.UpArrow;
    public KeyCode Down = KeyCode.DownArrow;
    public KeyCode Left = KeyCode.LeftArrow;
    public KeyCode Right = KeyCode.RightArrow;
    public KeyCode PunchKey = KeyCode.Z;
    public KeyCode KickKey = KeyCode.X;
    public KeyCode DefendKey = KeyCode.C;
    public KeyCode JumpKey = KeyCode.Space;

    [Space(10)]
    [HideInInspector]
    public Vector2 dir; 
    public static bool defendKeyDown;   //是否按下了防御键

    //角色移动的委托
    public delegate void MoveInputEventHandler(Vector2 dir);
    public static event MoveInputEventHandler onMoveInputEvent;
    //角色攻击的委托
    public delegate void CombatInputEventHandler(COMBATACTION action);
    public static event CombatInputEventHandler onCombatInputEvent;

    //TODO:显示UI部分
    private void Start()
    {

    }

    private void Update()
    {
        if (UseKeyboardInput)
        {
            KeyboardControls();
        }
    }

    //移动输入事件
    public static void MoveInputEvent(Vector2 dir)
    {
        if (onMoveInputEvent!=null)
        {
            onMoveInputEvent(dir);
        }
    }

    //战斗输入事件
    public static void CombatInputEvent(COMBATACTION combatAction)
    {
        if (onCombatInputEvent!=null)
        {
            onCombatInputEvent(combatAction);
        }
    }

    //键盘控制
    void KeyboardControls()
    {
        //角色移动
        float x = 0f, y = 0f;

        if (Input.GetKey(Up))
        {
            y =1f;
        }
        if (Input.GetKey(Down))
        {
            y = -1f;
        }
        if (Input.GetKey(Left))
        {
            x = -1f;
        }
        if (Input.GetKey(Right))
        {
            x = 1f;
        }

        dir = new Vector2(x, y);
        MoveInputEvent(dir);

        //角色战斗
        if (Input.GetKeyDown(PunchKey))
        {
            CombatInputEvent(COMBATACTION.PUNCH);
        }
        if (Input.GetKeyDown(KickKey))
        {
            CombatInputEvent(COMBATACTION.KICK);
        }
        if (Input.GetKeyDown(JumpKey))
        {
            CombatInputEvent(COMBATACTION.JUMP);
        }

        defendKeyDown = Input.GetKey(DefendKey);
    }

    //返回是否按下防御的状态
    public bool isDefendKeyDown()
    {
        return defendKeyDown;
    }
}
