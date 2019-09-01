using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnapsackPanel : BasePanel
{ 

    public void OnItemClick()
    {
        UIManager.instance.OpenPanel(UIPanelType.Skill,false);
    }

    public override void SetModal(bool modal)
    {
        base.SetModal(modal);
    }

}
