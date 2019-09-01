using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// public void OpenPanel(UIPanelType panelType, bool modal = true, object arg = null)
/// public void OpenPanel(OpenData data)
/// public void ClosePanel(UIPanelType panelType)
/// </summary>
public class UIManager
{

    private static UIManager _instance;
    public static UIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager();
            }
            return _instance;
        }
    }

    /////////////////////////////
    // 窗口堆栈
    private List<OpenData> panelStack = new List<OpenData>();
    private Dictionary<UIPanelType, UIPanelInfo> panelInfoDict = new Dictionary<UIPanelType, UIPanelInfo>();
    private List<OpenData> restoreList = new List<OpenData>();

    private Transform _canvasTransform;
    public Transform canvasTransform
    {
        get
        {
            if (_canvasTransform == null)
            {
                _canvasTransform = GameObject.Find("Canvas").transform;
            }
            return _canvasTransform;
        }
    }
    ////////////////////////////

    /// <summary>
    /// 单例
    /// </summary>
    private UIManager()
    {
        this.ParseUIPanelTypeJson();
    }
    /// <summary>
    /// 返回主界面
    /// </summary>
    public void GoHome()
    {
        int count = this.panelStack.Count;

        while (count > 0)
        {
            count--;
            OpenData data = this.panelStack[count];          

            if (data.panelType == UIPanelType.MainMenu) // 如果是主界面
            {
                data = CreatePanel(data);
                data.panel.OnResume(data);
                break;
            }
            this.panelStack.RemoveAt(count);
            if (data.panel != null)
            {
                data.tempDuration = -1;
                data.panel.OnExit(data);
                data.panel = null;
            }            
        }
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="panelType">面板类型,@see UIPanelType</param>
    /// <param name="modal"></param>
    /// <param name="arg"></param>
    public void OpenPanel(UIPanelType panelType, bool modal = true, object arg = null)
    {
        this.OpenPanel(new OpenData(panelType,modal,arg));
    }

    /// <summary>
    /// 清理第一个全屏之后的窗体 ,删除掉,释放内存 
    /// </summary>
    public void ClearStackPanel() {
        FirstPanel fistPanel = null;
        int count = this.panelStack.Count;

        while (count > 0)
        {
            count--;
            OpenData data = this.panelStack[count];

            if (fistPanel == null)
            {
                if (data.panel is FirstPanel)
                {
                    fistPanel = data.panel as FirstPanel;
                }

            }
            else
            {
                if (data.panel) {
                    data.tempDuration = 0;
                    data.panel.OnExit(data);
                    data.panel = null;
                }
            }
        }
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="data"></param>
    public void OpenPanel(OpenData data)
    {

        OpenData od = this.popData(data);//如果已经存在,删除掉


        if (this.panelStack.Count > 0 && data.modal)
        {
            this.SetStackOpenPanelModal();
        }

        if (od == null)
        {
            od = data;
        } else
        {
            od.CopyFrom(data);
        }
        data = CreatePanel(od,-1);
        //这儿不能取消
        data.panel.SetModal(false);
        //处理 层级 显示 问题
        od.panel.transform.SetAsLastSibling();
        //
        this.panelStack.Add(od);
        // 优化显示问题,全屏 面板后面 的面板不显示 
        OptimizedDisplay();
    }

    /// <summary>
    /// 关闭窗体
    /// </summary>
    /// <param name="panelType"></param>
    public void ClosePanel(UIPanelType panelType)
    {
        OpenData data = this.popData(panelType);
        if (data != null)
        {
            if (data.panel != null)
            {
                data.tempDuration = -1;
                data.panel.OnExit(data);
                data.panel = null;
            }

            if (this.panelStack.Count > 0)
            {
                data = this.panelStack[this.panelStack.Count - 1];
                data = CreatePanel(data);
                if (data.modal)
                {
                    this.SetStackOpenPanelModal(1);
                } else
                {
                    this.SetStackClosePanelModal();
                }
                data.panel.OnResume(data);

                OptimizedDisplay();
                RestoreDisplayPanel();
            }
        }
    }
    ///////////////////////////////////////////////////////////////////////
    // private

    private OpenData CreatePanel(OpenData data,float duration=0)
    {
        if (data.panel == null)
        {
            data.panel = this.loadPanel(data.panelType);
            data.tempDuration = duration;
            data.panel.OnEnter(data);
        }
        return data;
    }
    /// <summary>
    /// 关闭窗体时,显示 后续的窗体,直到遇到全屏 窗体
    /// </summary>
    private void RestoreDisplayPanel()
    {
        FirstPanel fistPanel = null;
        int count = this.panelStack.Count;
        

        restoreList.Clear();

        OpenData modalData = null;

        while (count > 0)
        {
            count--;
            OpenData data = this.panelStack[count];

            data = CreatePanel(data);

            if (!data.panel.gameObject.activeSelf)
            {
                data.panel.gameObject.SetActive(true);
            }

            restoreList.Add(data);

            if (modalData  != null)//如果前面有modal面板 了,那后面的面板 都不应该点击
            {
                data.panel.SetModal(true);
            }

            if (data.panelType == UIPanelType.MainMenu  || data.panel is FirstPanel)
            {
                break;
            } 
            if (data.modal)
            {
                modalData = data;
            }
        }

        //解決層級顯示問題
        if (restoreList.Count > 0)
        {
            for (int i = restoreList.Count - 1; i >= 0; i--)
            {

                OpenData data = restoreList[i];

                data.panel.transform.SetAsLastSibling();
            }
        }
    }

    /// <summary>
    /// 全屏之后的窗体不显示
    /// </summary>
    private void OptimizedDisplay()
    {
        FirstPanel fistPanel = null;
        int count = this.panelStack.Count;

        while (count > 0)
        {
            count--;
            OpenData data = this.panelStack[count];

            if (fistPanel == null) // 全屏窗体之前的需要显示出来
            {
                if (data.panel != null)
                {
                    if (!data.panel.gameObject.activeSelf)
                    {
                        data.panel.gameObject.SetActive(true);
                    }
                }
            }

            if (data.panelType == UIPanelType.MainMenu) // 如果 是主界面 就退出
            {
                break;
            }

            if (data.panel is FirstPanel)
            {
                if (fistPanel == null)
                {
                    fistPanel = data.panel as FirstPanel;
                } else
                {
                    if (data.panel.gameObject.activeSelf)
                    {
                        data.panel.gameObject.SetActive(false);
                    }
                    break;
                }
                
            } else if (fistPanel && data.panel != null)
            {
                // 全屏之后的窗体隐藏掉
                if (data.panel.gameObject.activeSelf)
                {
                    data.panel.gameObject.SetActive(false);
                }
            }
        }

    }

    /// <summary>
    /// 处理打开窗体时的modal
    /// </summary>
    /// <param name="countOffset"></param>
    private void SetStackOpenPanelModal(int countOffset = 0)
    {
        int count = this.panelStack.Count - countOffset;
        while (count > 0)
        {
            count--;
            OpenData data = this.panelStack[count];

            if (data.modal)
            {
                if (data.panel != null)
                {
                    data.panel.SetModal(true);
                }
                break;
            }
            else
            {
                if (data.panel)
                {
                    data.panel.SetModal(true);
                }
            }
        }
    }
    /// <summary>
    /// 关闭窗体的时候处理回退modal
    /// </summary>
    /// <param name="countOffset"></param>
    private void SetStackClosePanelModal(int countOffset = 0)
    {
        int count = this.panelStack.Count - countOffset;
        while (count > 0)
        {
            count--;
            OpenData data = this.panelStack[count];


            if (data.panel)
            {
                data.panel.SetModal(false);
            }

            if (data.modal)
            {
                break;
            }
        }
    }


    /// <summary>
    /// 查找窗口数据并返回,如果找到从堆栈中删除
    /// </summary>
    private OpenData popData(OpenData data)
    {
        return popData(data.panelType);
    }
    /// <summary>
    /// 查找窗口数据并返回,如果找到从堆栈中删除
    /// </summary>
    private OpenData popData(UIPanelType panelType)
    {
        int count = this.panelStack.Count - 1;
        while (count >= 0)
        {
            OpenData od = this.panelStack[count];
            if (od.panelType == panelType)
            {
                this.panelStack.RemoveAt(count);
                return od;
            }
            count--;
        }
        return null;
    }

    /// <summary>
    /// 加载面板
    /// </summary>
    /// <param name="panelType"></param>
    /// <returns></returns>
    private BasePanel loadPanel(UIPanelType panelType)
    {
       UIPanelInfo info = panelInfoDict.TryGet(panelType);
        if (info == null)
        {            
            Debug.LogError("PanelManager::loadPanel cant get PanelInfo by " + panelType.ToString());
            return null;
        }
        GameObject insPanel = GameObject.Instantiate(Resources.Load(info.path)) as GameObject;
        insPanel.transform.SetParent(canvasTransform,false);
        BasePanel basePanel = insPanel.GetComponent<BasePanel>();
        basePanel.panelType = panelType;
        return basePanel;
    }

    [Serializable]
    class UIPanelTypeJson
    {
        public List<UIPanelInfo> infoList;
    }

    private void ParseUIPanelTypeJson()
    {
        TextAsset ta = Resources.Load<TextAsset>("UIPanelType");
        UIPanelTypeJson list = JsonUtility.FromJson<UIPanelTypeJson>(ta.text);

        foreach(UIPanelInfo info in list.infoList)
        {
            panelInfoDict.Add(info.panelType, info);
        }
        
    }

}
