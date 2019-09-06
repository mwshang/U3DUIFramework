using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPuzzleIndex : FirstPanel
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnOpenLevelMapPanel()
    {
        UIManager.instance.OpenPanel(UIPanelType.WordPuzzleLevels);
    }
}
