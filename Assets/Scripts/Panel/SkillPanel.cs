using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPanel : SecondPanel
{ 
    public void OnTest()
    {
        UIManager.instance.OpenPanel(UIPanelType.Shop,false);
    }
}
