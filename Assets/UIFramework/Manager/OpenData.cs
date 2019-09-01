using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenData
{
    public UIPanelType panelType;
    public object arg;
    public bool modal;
    public BasePanel panel;

    //窗体缓动时长，用它来控制窗体是否需要缓动
    //大于0时需要 缓动，由UIManager内部控制 
    public float tempDuration  = 0;

    public OpenData(UIPanelType panelType, bool modal = true, object arg = null)
    {
        this.panelType = panelType;
        this.modal = modal;
        this.arg = arg;
    }

    public void CopyFrom(OpenData data)
    {
        // 注意 这里不copy panel
        this.panelType = data.panelType;
        this.arg = data.arg;
        this.modal = data.modal;
    }
}
