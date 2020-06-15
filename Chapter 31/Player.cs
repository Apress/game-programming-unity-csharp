using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    private enum Mode
    {
        Build,
        Play
    }

    private Mode mode = Mode.Build;
    
    [Header("References")]
    public Transform trans;
    public Transform spawnPoint;
    public Transform leakPoint;
    
    [Tooltip("Reference to the sell button lock panel GameObject.")]
    public GameObject sellButtonLockPanel;

    [Header("X Bounds")]
    public float minimumX = -70;
    public float maximumX = 70;

    [Header("Y Bounds")]
    public float minimumY = 18;
    public float maximumY = 80;

    [Header("Z Bounds")]
    public float minimumZ = -130;
    public float maximumZ = 70;

    [Header("Movement")]
    [Tooltip("Distance traveled per second with the arrow keys.")]
    public float arrowKeySpeed = 80;

    [Tooltip("Multiplier for mouse drag movement.  A higher value will result in the camera moving a greater distance when the mouse is moved.")]
    public float mouseDragSensitivity = 2.8f;

    [Tooltip("Amount of smoothing applied to camera movement.  Should be a value between 0 and 1.")]
    [Range(0,.99f)]
    public float movementSmoothing = .75f;

    private Vector3 targetPosition;

    [Header("Scrolling")]
    [Tooltip("Amount of Y distance the camera moves per mouse scroll increment.")]
    public float scrollSensitivity = 1.6f;

    [Header("Build Mode")]
    [Tooltip("Current gold for the player.  Set this to however much gold the player should start with.")]
    public int gold = 50;

    [Tooltip("Layer mask for highlighter raycasting.  Should include the layer of the stage.")]
    public LayerMask stageLayerMask;

    [Tooltip("Reference to the Transform of the Highlighter GameObject.")]
    public Transform highlighter;

    [Tooltip("Reference to the Tower Selling Panel.")]
    public RectTransform towerSellingPanel;

    [Tooltip("Reference to the Text component of the Refund Text in the Tower Selling Panel.")]
    public Text sellRefundText;

    [Tooltip("Reference to the Text component of the current gold text in the bottom-left corner of the UI.")]
    public Text currentGoldText;

    [Tooltip("The color to apply to the selected build button.")]
    public Color selectedBuildButtonColor = new Color(.2f,.8f,.2f);

    //Mouse position at the last frame.
    private Vector3 lastMousePosition;

    //Current gold the last time we checked.
    private int goldLastFrame;

    //True if the cursor is over the stage right now, false if not.
    private bool cursorIsOverStage = false;

    //Reference to the Tower prefab selected by the build button.
    private Tower towerPrefabToBuild = null;

    //Reference to the currently selected build button Image component.
    private Image selectedBuildButtonImage = null;

    //Currently selected Tower instance, if any.
    private Tower selectedTower = null;

    private Dictionary<Vector3, Tower> towers = new Dictionary<Vector3, Tower>();

    //Play Mode:
    [Header("Play Mode")]
    [Tooltip("Reference to the Build Button Panel to deactivate it when play mode starts.")]
    public GameObject buildButtonPanel;

    [Tooltip("Reference to the Game Lost Panel.")]
    public GameObject gameLostPanel;

    [Tooltip("Reference to the Text component for the info text in the Game Lost Panel.")]
    public Text gameLostPanelInfoText;

    [Tooltip("Reference to the Play Button GameObject to deactivate it in play mode.")]
    public GameObject playButton;

    [Tooltip("Reference to the Enemy Holder Transform.")]
    public Transform enemyHolder;

    [Tooltip("Reference to the ground enemy prefab.")]
    public Enemy groundEnemyPrefab;

    [Tooltip("Reference to the flying enemy prefab.")]
    public Enemy flyingEnemyPrefab;

    [Tooltip("Time in seconds between each enemy spawning.")]
    public float enemySpawnRate = .35f;

    [Tooltip("Determines how often flying enemy levels occur.  For example if this is set to 4, every 4th level is a flying level.")]
    public int flyingLevelInterval = 4;

    [Tooltip("Number of enemies spawned each level.")]
    public int enemiesPerLevel = 15;

    [Tooltip("Gold given to the player at the end of each level.")]
    public int goldRewardPerLevel = 12;

    //The current level.
    public static int level = 1;

    //Number of enemies spawned so far for this level.
    private int enemiesSpawnedThisLevel = 0;

    //Player's number of remaining lives; once it hits 0, the game is over:
    public static int remainingLives = 40;

    //Methods:
    void ArrowKeyMovement()
    {
        //If up arrow is held,
        if (Input.GetKey(KeyCode.UpArrow))
        {
            //...add to target Z position:
            targetPosition.z += arrowKeySpeed * Time.deltaTime;
        }
        //Otherwise, if down arrow is held,
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //...subtract from target Z position:
            targetPosition.z -= arrowKeySpeed * Time.deltaTime;
        }

        //If right arrow is held,
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //..add to target X position:
            targetPosition.x += arrowKeySpeed * Time.deltaTime;
        }
        //Otherwise, if left arrow is held,
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            //...subtract from target X position:
            targetPosition.x -= arrowKeySpeed * Time.deltaTime;
        }
    }

    void MouseDragMovement()
    {
        //If the right mouse button is held,
        if (Input.GetMouseButton(1))
        {
            //Get the movement amount this frame:
            Vector3 movement = new Vector3(-Input.GetAxis("Mouse X"),0,-Input.GetAxis("Mouse Y")) * mouseDragSensitivity;

            //If there is any movement,
            if (movement != Vector3.zero)
            {
                //...apply it to the targetPosition:
                targetPosition += movement;
            }
        }
    }

    void Zooming()
    {
        //Get the scroll delta Y value and flip it:
        float scrollDelta = -Input.mouseScrollDelta.y;

        //If there was any delta,
        if (scrollDelta != 0)
        {
            //...apply it to the Y position:
            targetPosition.y += scrollDelta * scrollSensitivity;
        }
    }

    void MoveTowardsTarget()
    {
        //Clamp the target position to the bounds variables:
        targetPosition.x = Mathf.Clamp(targetPosition.x,minimumX,maximumX);
        targetPosition.y = Mathf.Clamp(targetPosition.y,minimumY,maximumY);
        targetPosition.z = Mathf.Clamp(targetPosition.z,minimumZ,maximumZ);

        //Move if we aren't already at the target position:
        if (trans.position != targetPosition)
        {
            trans.position = Vector3.Lerp(trans.position,targetPosition,1 - movementSmoothing);
        }
    }

    void PositionHighlighter()
    {
        //If the mouse position this frame is different than last frame:
        if (Input.mousePosition != lastMousePosition)
        {
            //Get a ray at the mouse position, shooting out of the camera:
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit; //Information on what was hit will be stored here

            //Cast the ray and check if it hit anything, using our layer mask:
            if (Physics.Raycast(ray,out hit,Mathf.Infinity,stageLayerMask.value))
            {
                //If it did hit something, use hit.point to get the location it hit:
                Vector3 point = hit.point;
                
                //Round the X and Z values to multiples of 10:
                point.x = Mathf.Round(hit.point.x * .1f) * 10;
                point.z = Mathf.Round(hit.point.z * .1f) * 10;

                //Clamp Z between -80 and 80 to prevent sticking over the edge of the stage:
                point.z = Mathf.Clamp(point.z,-80,80);
                
                //Ensure Y is always 0:
                point.y = .2f;

                //Make sure the highlighter is active (visible) and set its position:
                highlighter.position = point;
                highlighter.gameObject.SetActive(true);
                cursorIsOverStage = true;
            }
            else //If the ray didn't hit anything,
            {
                //... mark cursorIsOverStage as false:
                cursorIsOverStage = false;

                //Deactivate the highlighter GameObject so it no longer shows:
                highlighter.gameObject.SetActive(false);
            }
        }

        //Make sure we keep track of the mouse position this frame:
        lastMousePosition = Input.mousePosition;
    }

    void OnStageClicked()
    {
        //If a build button is selected:
        if (towerPrefabToBuild != null)
        {
            //If there is no tower in that slot and we have enough gold to build the selected tower:
            if (!towers.ContainsKey(highlighter.position) && gold >= towerPrefabToBuild.goldCost)
            {
                BuildTower(towerPrefabToBuild,highlighter.position);
            }
        }
        //If no build button is selected:
        else
        {
            //Check if a tower is at the current highlighter position:
            if (towers.ContainsKey(highlighter.position))
            {
                //Set the selected tower to this one:
                selectedTower = towers[highlighter.position];

                //Update the refund text: 
                sellRefundText.text = "for " + Mathf.CeilToInt(selectedTower.goldCost * selectedTower.refundFactor) + " gold";

                //Make sure the sell tower UI panel is active so it shows:
                towerSellingPanel.gameObject.SetActive(true);
            }
        }
    }

    void BuildTower(Tower prefab,Vector3 position)
    {
        //Instantiate the tower at the given location and place it in the Dictionary:
        towers[position] = Instantiate<Tower>(prefab,position,Quaternion.identity);
        
        //Decrease player gold:
        gold -= towerPrefabToBuild.goldCost;

        //Update the path through the maze:
        UpdateEnemyPath();
    }

    void SellTower(Tower tower)
    {
        //Since it's not going to exist in a bit, deselect the tower:
        DeselectTower();

        //Refund the player:
        gold += Mathf.CeilToInt(tower.goldCost * tower.refundFactor);

        //Remove the tower from the dictionary using its position:
        towers.Remove(tower.transform.position);

        //Destroy the tower GameObject:
        Destroy(tower.gameObject);

        //Refresh pathfinding:
        UpdateEnemyPath();
    }

    public void OnSellTowerButtonClicked()
    {
        //If there is a selected tower,
        if (selectedTower != null)
            //Sell it:
            SellTower(selectedTower);
    }

    void PositionSellPanel()
    {
        //If there is a selected tower:
        if (selectedTower != null)
        {
            //Convert tower world position, moved forward by 8 units, to screen space:
            var screenPosition = Camera.main.WorldToScreenPoint(selectedTower.transform.position + Vector3.forward * 8);

            //Apply the position to the tower selling panel:
            towerSellingPanel.position = screenPosition;
        }
    }

    void UpdateCurrentGold()
    {
        //If the gold has changed since last frame:
        if (gold != goldLastFrame)
            //Update the text to match:
            currentGoldText.text = gold + " gold";
        
        //Keep track of the gold value each frame:
        goldLastFrame = gold;
    }

    public void DeselectTower()
    {
        //Null selected tower and hide the sell tower panel:
        selectedTower = null;
        towerSellingPanel.gameObject.SetActive(false);
    }

    void DeselectBuildButton()
    {
        //Null the tower prefab to build, if there is one:
        towerPrefabToBuild = null;

        //Reset the color of the selected build button, if there is one:
        if (selectedBuildButtonImage != null)
        {
            selectedBuildButtonImage.color = Color.white;
            selectedBuildButtonImage = null;
        }
    }

    void UpdateEnemyPath()
    {
        Invoke("PerformPathfinding",.1f);
    }

    void BuildModeLogic()
    {
        PositionHighlighter();

        PositionSellPanel();

        UpdateCurrentGold();
        
        //If the left mouse button is clicked while the cursor is over the stage:
        if (cursorIsOverStage && Input.GetMouseButtonDown(0))
        {
            OnStageClicked();
        }

        //If Escape is pressed:
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTower();

            DeselectBuildButton();
        }
    }

    public void OnBuildButtonClicked(Tower associatedTower)
    {
        //Set the prefab to build:
        towerPrefabToBuild = associatedTower;

        //Clear selected tower (if any):
        DeselectTower();
    }

    public void SetSelectedBuildButton(Image clickedButtonImage)
    {
        //Keep a reference to the Button that was clicked:
        selectedBuildButtonImage = clickedButtonImage;

        //Set the color of the clicked button:
        clickedButtonImage.color = selectedBuildButtonColor;
    }

    void PerformPathfinding()
    {
        //Pathfind from spawn point to leak point, storing the result in GroundEnemy.path:
        NavMesh.CalculatePath(spawnPoint.position, leakPoint.position, NavMesh.AllAreas, GroundEnemy.path);
        
        if (GroundEnemy.path.status == NavMeshPathStatus.PathComplete)
        {
            //If the path was successfully found, make sure the lock panel is inactive:
            sellButtonLockPanel.SetActive(false);
        }
        else //If the path is blocked,
        {
            //Activate the lock panel:
            sellButtonLockPanel.SetActive(true);
        }
    }
    
    void SpawnEnemy()
    {   
        Enemy enemy = null;
        
        //If this is a flying level
        if (level % flyingLevelInterval == 0)
        {
            enemy = Instantiate(flyingEnemyPrefab,spawnPoint.position + (Vector3.up * 18),Quaternion.LookRotation(Vector3.back));
        }
        else //If it's a ground level
        {
            enemy = Instantiate(groundEnemyPrefab,spawnPoint.position,Quaternion.LookRotation(Vector3.back));
        }

        //Parent enemy to the enemy holder:
        enemy.trans.SetParent(enemyHolder);

        //Count that we spawned the enemy:
        enemiesSpawnedThisLevel += 1;

        //Stop invoking if we've spawned all enemies:
        if (enemiesSpawnedThisLevel >= enemiesPerLevel)
        {
            CancelInvoke("SpawnEnemy");
        }
    }

    public void PlayModeLogic()
    {
        //If no enemies are left and all enemies have already spawned
        if (enemyHolder.childCount == 0 && enemiesSpawnedThisLevel >= enemiesPerLevel)
        {
            //Return to build mode if we haven't lost yet:
            if (remainingLives > 0)
                GoToBuildMode();
            else
            {
                //Update game lost panel text with information:
                gameLostPanelInfoText.text = "You had " + remainingLives + " lives by the end and made it to level " + level + ".";
                
                //Activate the game lost panel:
                gameLostPanel.SetActive(true);
            }
        }
    }

    void GoToPlayMode()
    {
        mode = Mode.Play;
        
        //Deactivate build button panel and play button:
        buildButtonPanel.SetActive(false);
        playButton.SetActive(false);

        //Deactivate highlighter:
        highlighter.gameObject.SetActive(false);
    }

    void GoToBuildMode()
    {
        mode = Mode.Build;

        //Activate build button panel and play button:
        buildButtonPanel.SetActive(true);
        playButton.SetActive(true);

        //Reset enemies spawned:
        enemiesSpawnedThisLevel = 0;
        
        //Increase level:
        level += 1;
        gold += goldRewardPerLevel;
    }

    public void StartLevel()
    {
        //Switch to play mode:
        GoToPlayMode();

        //Repeatedly invoke SpawnEnemy:
        InvokeRepeating("SpawnEnemy",.5f,enemySpawnRate);
    }

    //Unity events:
    void Start()
    {
        targetPosition = trans.position;
        GroundEnemy.path = new NavMeshPath();
        UpdateEnemyPath();
    }
    
    void Update()
    {
        ArrowKeyMovement();

        MouseDragMovement();

        Zooming();

        MoveTowardsTarget();

        //Run build mode logic if we're in build mode:
        if (mode == Mode.Build)
            BuildModeLogic();
        else
            PlayModeLogic();
    }
}
