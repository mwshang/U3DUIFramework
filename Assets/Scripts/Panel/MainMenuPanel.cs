using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPanel : BasePanel
{ 

    public void OnPushPanel(string panelTypeString)
    {
        UIPanelType panelType = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), panelTypeString);

        //UIManager.instance.PushStack(panelType);
        UIManager.instance.OpenPanel(panelType);
    }

    public void LoadWordPuzzle()
    {
        SceneManager.LoadScene("WordPuzzle");
    }


}
