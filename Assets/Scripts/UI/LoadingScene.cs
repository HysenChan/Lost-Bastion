using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    private float time;
    public Image image;

    private AsyncOperation async;


    //读取场景的进度，它的取值范围在0 - 1 之间。
    int progress = 0;

    private void Start()
    {
        //第一关的场景名称
        Global.loadName = "Level1";
        StartCoroutine(Loading());
    }

    private void Update()
    {
        //在这里计算读取的进度，
        //progress 的取值范围在0.1 - 1之间， 但是它不会等于1
        //也就是说progress可能是0.9的时候就直接进入新场景了
        //所以在写进度条的时候需要注意一下。
        //为了计算百分比 所以直接乘以100即可
        progress = (int)(async.progress * 100);

        //进度条
        image.fillAmount = progress;
        image.GetComponentInChildren<Text>().text = "100";
    }

    IEnumerator Loading()
    {
        async = SceneManager.LoadSceneAsync(Global.loadName);
        yield return async;
    }
}
