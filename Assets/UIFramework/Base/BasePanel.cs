using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public delegate void TweenOnComplete();

public class BasePanel : MonoBehaviour
{

    public UIPanelType panelType;

    protected CanvasGroup _canvasGroup;
    protected float tweenDuration = 0.3f;

    public CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
        
    }
    /// <summary>
    /// 是否主界面
    /// </summary>
    /// <returns></returns>
    public bool IsMainMenu()
    {
        return this.panelType == UIPanelType.MainMenu;
    }

    public virtual void SetModal(bool modal)
    {
        CanvasGroup cg = this.canvasGroup;
        if (cg != null)
        {
            cg.blocksRaycasts = !modal;
        } else
        {
            Debug.LogWarning("BasePanel::SetModal --> cant get a CanvasGroup Component!!!!!");
        }
    }

    public virtual void OnEnter(OpenData data)
    {
        this.doTween(data);
    }

    protected  virtual void doTween(OpenData  data)
    {
        CanvasGroup cg = this.canvasGroup;
        if (cg)
        {
            float duration = data.tempDuration;
            if (duration == -1)
            {
                duration = this.tweenDuration;
            }

            if (duration > 0)
            {
                cg.alpha = 0;
                cg.DOFade(1, tweenDuration).OnComplete(() => {
                    this.SetModal(false);//可以交互
                });
            }
            else
            {
                cg.alpha = 1;
                this.SetModal(false);//可以交互
            }

        }
        else
        {
            Debug.LogWarning("BasePanel::OnEnter --> cant get a CanvasGroup Component!!!!!");
        }
    }

    /// <summary>
    /// (未使用)
    /// </summary>
    /// <param name="data"></param>
    public virtual void OnPause(OpenData data)
    {
        if (data != null)
        {
            this.SetModal(data.modal);
        }
        else
        {
            this.SetModal(true);
        }
    }
    public virtual void OnResume(OpenData data)
    {
        this.SetModal(false);
    }
    public virtual void OnExit(OpenData  data)
    {
        CanvasGroup cg = this.canvasGroup;
        float duration = data.tempDuration;
        if (duration == -1)
        {
            duration = this.tweenDuration;
        }

        if (cg && duration > 0)
        {
            
            cg.DOFade(0, tweenDuration).OnComplete(()=> {            
                Destroy(this.gameObject);
            });
        } else
        {
            Destroy(this.gameObject);
        }
    }

    public virtual void OnClosePanel()
    {
        UIManager.instance.ClosePanel(this.panelType);
    }
}
