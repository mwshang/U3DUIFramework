using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //UIManager.instance.PushStack(UIPanelType.MainMenu);
        UIManager.instance.OpenPanel(UIPanelType.MainMenu);
    }
}
