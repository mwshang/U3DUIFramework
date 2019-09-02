using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
///  二级弹框(非全屏)
/// </summary>
public class SecondPanel : BasePanel
{
    private static AnimationCurve _tweenCurve;

    protected AnimationCurve tweenCurve
    {
        get
        {
            if (_tweenCurve == null)
            {
                _tweenCurve = new AnimationCurve();
                _tweenCurve.AddKey(new Keyframe(0, 0, 2, 2, 0, 0));
                _tweenCurve.AddKey(new Keyframe(0.2675635f, 1.073366f, 0.7099764f, 0.7099764f, 0.3333333f, 0.3333333f));
                _tweenCurve.AddKey(new Keyframe(0.6140905f, 0.9734462f, -0.3934441f, -0.3934441f, 0.3333333f, 0.3333333f));
                _tweenCurve.AddKey(new Keyframe(0.9936218f, 0.9977036f, 0, 0, 0, 0));
            }
            return _tweenCurve;
        }
    }

    protected override void doTween(OpenData data)
    {
        float duration = data.tempDuration;
        if (duration == -1)
        {
            duration = tweenDuration;
        }

        if (duration > 0)
        {
            this.transform.localScale = Vector3.zero;
            Tweener tween = this.transform.DOScale(1, duration).OnComplete(() =>
            {
                this.SetModal(false);//可以交互
            });
            //tween.SetEase(Ease.OutElastic);
            tween.SetEase(tweenCurve);

            /* 
             * 定义一个public AnimationCurve,在Editor中编辑之后,输出对应值
            Keyframe[] keys = curve.keys;
            foreach(Keyframe key in keys)
            {
                Debug.Log("inTangent:" + key.inTangent + "  inWeight:" + key.inWeight + "  outTangent:" + key.outTangent + "  outWeight:" + key.outWeight + "  tangentMode:" + key.tangentMode + "  time:" + key.time + "  value:" + key.value);
            }
            */
        }
        else
        {
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            this.SetModal(false);//可以交互
        }
    }
}
