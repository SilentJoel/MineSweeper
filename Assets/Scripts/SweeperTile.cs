using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SweeperTile : MonoBehaviour
{
    public SweeperManager manager;

    
    public bool isBomb = false;
    public bool isOpen = false;
    public bool isMarkedWithFlag = false;
    public int numberOfCloseBombs = 0;
    public TextMesh bombText;

    public int xPosition;
    public int yPosition;

    

    public Material normalMat;
    public Material openMat;
    public Material markedMat;
    public Material bombMat;

    void Start()
    {
        bombText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        CheckTileState();
    }

    public void CheckTileState()
    {
        if(isOpen == false && manager.canPlay)
        {
            var renderer = gameObject.GetComponent<MeshRenderer>();

            if(isBomb)
            {
                renderer.material = bombMat;
                manager.GameLost(gameObject);
            }
            else
            {
                renderer.material = openMat;
                isOpen = true;
                
                numberOfCloseBombs = manager.CheckNeighboringTiles(xPosition, yPosition);

                if(numberOfCloseBombs > 0)
                {
                    bombText.text = numberOfCloseBombs.ToString();
                }
            }
        }

        manager.CheckForWin();
    }

    public void MarkTile()
    {
        var renderer = gameObject.GetComponent<MeshRenderer>();
        if(!isOpen)
        {
            if(isMarkedWithFlag)
            {
                isMarkedWithFlag = false;
                renderer.material = normalMat;
            }
            else
            {
                isMarkedWithFlag = true;
                renderer.material = markedMat;
            }        
        }
    }

    public void MarkAsBombe()
    {
        if(isBomb)
        {
            var renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.material = bombMat;
        }
    }
}
