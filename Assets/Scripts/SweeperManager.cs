using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SweeperManager : MonoBehaviour
{
    public int TileCountX;
    public int TileCountY;
    public GameObject[,] Tiles;
    public List<GameObject> TileList;
    public float spacing;
    public GameObject TilePrefab;
    public int NumberOfBombs;
    public bool canPlay = true;
    public GameObject UiElements;
    public float explosionForce = 1f;
    public int tileCountSetting = 20;
    public bool doExplosion = false;
    public int bombPercent;

    public Slider TileCountSlider;
    public Text TileCountSliderText;

    public Slider BombPercentSlider;
    public Text BombPercentSliderText;

    public Toggle ExplodeCheckBox;

    public Text WinLoseText;

    bool gameOver = false;

    
    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;

        UiElements.SetActive(false);
        canPlay = true;
        tileCountSetting = Mathf.Min(tileCountSetting, 20);
        TileCountSlider.value = tileCountSetting;
        TileCountSliderText.text = "Tiles: " + tileCountSetting + "x" + tileCountSetting;
        TileCountX = tileCountSetting;
        TileCountY = tileCountSetting;

        BombPercentSlider.value = bombPercent;
        BombPercentSliderText.text = "Bombs: " + bombPercent + "%";

        ExplodeCheckBox.isOn = doExplosion;
        gameOver = false;
        GenereateField();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                SweeperTile tile = hit.transform.gameObject.GetComponent<SweeperTile>();

                if(tile != null)
                {
                    tile.MarkTile();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!gameOver)
            {                
                if(UiElements.activeSelf)
                {
                    canPlay = true;
                    UiElements.SetActive(false);
                }
                else
                {
                    canPlay = false;
                    UiElements.SetActive(true);
                }
            }
        }
    }

    public void GenereateField()
    {
        UiElements.SetActive(false);
        gameOver = false;

        NumberOfBombs = Mathf.RoundToInt((tileCountSetting * tileCountSetting) / 100 * bombPercent);
        TileCountX = tileCountSetting;
        TileCountY = tileCountSetting;

        if(NumberOfBombs <= 0)
        {
            NumberOfBombs = 1;
        }
        
        if(TileList?.Count > 0)
        {
            foreach(var item in TileList)
            {
                Destroy(item);
            }
        }

        TileList = new List<GameObject>();

        if(Tiles?.GetLength(0) > 0 && Tiles?.GetLength(1) > 0)
        {
            for (int i = 0; i < Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < Tiles.GetLength(1); j++)
                {
                    Destroy(Tiles[i, j]);
                }
            }
        }

        float startX = -((TileCountX * spacing) / 2) + spacing/2;
        float startY = -((TileCountY * spacing) / 2) + spacing/2;

        Vector3 spawnPosition = new Vector3(startX, startY,0);
        Tiles = new GameObject[TileCountX, TileCountY];

        for(int x = 0; x < TileCountX; x++)
        {
            for(int y = 0; y < TileCountY; y++)
            {
                GameObject spawnedTile = GameObject.Instantiate(TilePrefab, spawnPosition, Quaternion.identity);
                var sweeperTile = spawnedTile.GetComponent<SweeperTile>();
                sweeperTile.manager = this;
                
                sweeperTile.xPosition = x;
                sweeperTile.yPosition = y;

                Tiles[x, y] = spawnedTile;
                TileList.Add(spawnedTile);

                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y + spacing, 0);
            }

            spawnPosition = new Vector3(spawnPosition.x + spacing, startY, 0);
        }

        if(NumberOfBombs > TileList.Count)
        {
            NumberOfBombs = TileList.Count;
        }

        var bombIndexes = GenerateRandomIndexes();

        foreach(int i in bombIndexes)
        {
            var sweeperTile = TileList[i].GetComponent<SweeperTile>();
            sweeperTile.isBomb = true;
        }

        canPlay = true;
    }

    public List<int> GenerateRandomIndexes()
    {   
        List<int> randomIndexes = new List<int>();

        for(int i = 0; i < NumberOfBombs; i++)
        {
            int numToAdd = Random.Range(0,TileList.Count);
            bool generateNum = true;

            while(generateNum)
            {
                numToAdd = Random.Range(0,TileList.Count);

                if(!randomIndexes.Contains(numToAdd))
                {
                    randomIndexes.Add(numToAdd);
                    generateNum = false;
                }
            }
        }

        return randomIndexes;
    }

    public int CheckNeighboringTiles(int xPosition, int yPosition)
    {
        GameObject currentTile;
        int startX = -1;
        int startY = -1;

        int bombCount = 0;

        var neighbours = new List<SweeperTile>();

        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                try
                {
                    currentTile = Tiles[xPosition + startX + x, yPosition + startY + y];
                    SweeperTile currentSweeperTile = currentTile.GetComponent<SweeperTile>();
                    
                    if(currentSweeperTile.isOpen == false)
                    {
                        if(currentSweeperTile.isBomb)
                        {
                            bombCount++;
                        }
                        else
                        {
                            neighbours.Add(currentSweeperTile);
                        }
                    }
                }
                catch
                {

                }
            }
        }
        
        if(bombCount <= 0)
        {
            foreach(var item in neighbours)
            {
                if(item.isOpen == false)
                {
                    item.CheckTileState();
                }
            }
        }
        
        return bombCount;
    } 

    public void CheckForWin()
    {
        int countOpenTiles = 0;
        
        foreach(var item in TileList)
        {
            if(item.GetComponent<SweeperTile>().isOpen)
            {
                countOpenTiles++;
            }
        }

        if(countOpenTiles >= TileList.Count - NumberOfBombs)
        {
            UiElements.SetActive(true);
            canPlay = false;
            WinLoseText.text = "You Won";
            gameOver = true;
        }
    }

    public void GameLost(GameObject clickedTile)
    {
        WinLoseText.text = "You Lost";
        UiElements.SetActive(true);
        canPlay = false;
        gameOver = true;

        foreach(var item in TileList)
        {
            item.GetComponent<SweeperTile>().MarkAsBombe();
        }

        if(doExplosion)
        {
            ExplodeTiles(clickedTile);
        }
    }

    public void ExplodeTiles(GameObject explosionSource)
    {
        foreach(var tile in TileList)
        {
            if(tile == explosionSource)
            {
                continue;
            }

            var rb = tile.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            rb.AddTorque((tile.transform.position - explosionSource.transform.position).normalized * explosionForce, ForceMode.Impulse);
            rb.AddForce((tile.transform.position - explosionSource.transform.position).normalized * explosionForce, ForceMode.Impulse);
        }
    }

    public void TileCountChange()
    {
        tileCountSetting = (int)TileCountSlider.value;
        TileCountSliderText.text = "Tiles: " + tileCountSetting + "x" + tileCountSetting;
    }

    public void BombPersentSliderChange()
    {
        bombPercent = (int)BombPercentSlider.value;

        if(bombPercent > 100)
        {
            bombPercent = 100;
        }
        else if(bombPercent < 0)
        {
            bombPercent = 0;
        }

        BombPercentSliderText.text = "Bombs: " + bombPercent + "%";
    }

    public void ExplodeCheckBoxChange()
    {
        doExplosion = ExplodeCheckBox.isOn;
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
