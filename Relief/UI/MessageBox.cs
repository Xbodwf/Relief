using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.UI.Extensions.EasingCore;

public class MessageBox : MonoBehaviour
{
    private bool showMessageBox = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            showMessageBox = true;
        }
    }

    void OnGUI()
    {
        if (showMessageBox)
        {
            GUI.Box(new Rect(0, 0, Screen.width / 2, Screen.height / 2), "����Ҫ��ʾ���ı�");
            if (GUI.Button(new Rect(Screen.width / 4, Screen.height / 4, 100, 50), "�ر�"))
            {
                showMessageBox = false;
            }
        }
    }
}