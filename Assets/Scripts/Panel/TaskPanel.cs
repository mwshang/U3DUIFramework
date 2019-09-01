using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : BasePanel
{

    private Text ContentText;
    private object arg;

   private void Awake()
    {
        ContentText = this.transform.Find("ContentText").GetComponent<Text>();
    }


    public override void OnEnter(OpenData data)
    {
        base.OnEnter(data);

        arg = "Content text....";

        ContentText.text = arg.ToString();
        Debug.Log("TaskPanel OnEnter....");
    }
    private void Start()
    {
        
        Debug.Log("Task  Start....");
    }
}
