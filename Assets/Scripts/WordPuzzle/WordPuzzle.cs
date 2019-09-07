using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordPuzzle : MonoBehaviour
{
    public Transform tileItem;
    public ScrollRect scrollRect;

    // Start is called before the first frame update
    void Start()
    {
        UIManager.instance.OpenPanel(UIPanelType.WordPuzzleIndex);
    }

    // Update is called once per frame
    void Update()
    {

        //Vector3   p = tileItem.parent.parent.InverseTransformPoint(tileItem.position);
        //RectTransform itemRT = tileItem.transform as RectTransform;
        //float itemMidY = p.y - itemRT.sizeDelta.y * 0.5f;
        //RectTransform srRect = scrollRect.transform as RectTransform;
        
        //float midY = -srRect.sizeDelta.y * 0.5f;
        //float s = itemMidY - midY;
        //Debug.Log(s);
    }
}
