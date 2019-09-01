using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
///  二级弹框(非全屏)
/// </summary>
public class SecondPanel : BasePanel
{
    protected override void doTween(OpenData data)
    {
        float duration = data.tempDuration;
        if (duration == -1)
        {
            duration = 1f;
        }

        if (duration > 0)
        {
            this.transform.localScale = Vector3.zero;
            Tweener tween = this.transform.DOScale(1, duration).OnComplete(() =>
            {
                this.SetModal(false);//可以交互
            });
            tween.SetEase(Ease.OutElastic);
            
        }
        else
        {
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            this.SetModal(false);//可以交互
        }
    }
}
