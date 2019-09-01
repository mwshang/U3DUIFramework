using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemPanel : SecondPanel
{ 
    public void OnTest()
    {
        UIManager.instance.OpenPanel(UIPanelType.Task, false);

        //UIManager.instance.ClearStackPanel();
        //UIManager.instance.GoHome();
    }
}
