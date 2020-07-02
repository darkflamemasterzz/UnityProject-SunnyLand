using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    private int CherryCount;
    private int GemCount;

    public Text CherryNumText;

    public delegate void MessageHandler();

    public event MessageHandler PlayerCollectCherry;


    private void Start()
    {
        // 组测事件订阅者
        PlayerCollectCherry += new MessageHandler(addCherryCount);
    }

    public void awakePCCEvent()
    {
        // 触发PlayerCollectCherry 事件
        PlayerCollectCherry();
    }

    // cherry+1
    void addCherryCount()
    {
        CherryCount++;
        CherryNumText.text = CherryCount.ToString();
    }
}
