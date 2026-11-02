using UnityEngine;


public class TowerManager : MonoBehaviour
{
    public static TowerManager instance; //only one copy of it in the entire game.

    private void Awake()
    {
        instance = this;
    }

    public BaseTower activeTower; //this is reference of the tower that the player is trying to place

    public Transform indicator; //the holographic/ghost preview that follows the players cursor
    public bool isPlacing;

    public LayerMask whatIsPlacement, whatIsObstacle; // to declare which layers are good for towers to be built on, and the layer that prevents it

    public float topSafePercent = 15f; //this will be editted ltr on to fit edlysia and shin's UI layout but this basically prevents the tower from being placed on accident if the player clicks the UI (while having the tower on select to build)

    [HideInInspector]
    public BaseTower selectedTower; //the tower selected for build

    public GameObject selectedTowerEffect; //VFX to show that this tower is being selected

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isPlacing)
        {
            indicator.position = GetGridPosition(); //this method is basically how the tower moves with the cursor

            RaycastHit hit;
            if (Input.mousePosition.y > Screen.height * (1f - (topSafePercent / 100f))) //disables the placement when the cursor goes to the UI
            {
                indicator.gameObject.SetActive(false);
            }
            else if (Physics.Raycast(indicator.position + new Vector3(0f, -2f, 0f), Vector3.up, out hit, 10f, whatIsObstacle)) //disables placement if raycast detects obstacle layers
            {
                indicator.gameObject.SetActive(false); //might change it to show as a red holographic instead and SFX if player tries to build onto the pathway
            }
            else
            {
                indicator.gameObject.SetActive(true);

                UIManager.instance.notEnoughMoneyWarning.SetActive(MoneyManager.instance.currentMoney < activeTower.cost); //if the player does not have enough currency, this will be shown

                if (Input.GetMouseButtonDown(0))
                {
                    if (MoneyManager.instance.SpendMoney(activeTower.cost))
                    {

                        isPlacing = false;

                        Instantiate(activeTower, indicator.position, activeTower.transform.rotation);

                        indicator.gameObject.SetActive(false);

                        UIManager.instance.notEnoughMoneyWarning.SetActive(false);

                        AudioManager.instance.PlaySFX(8); //instantiates tower down and removes all the indicators
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    isPlacing = false;

                    indicator.gameObject.SetActive(false);

                    UIManager.instance.notEnoughMoneyWarning.SetActive(false); //if player right clicks, the tower selection will also disappear so they can in a sense, "escape" from building it
                }
            }
        }
    }
    public void StartTowerPlacement(BaseTower towerToPlace)
    {
        activeTower = towerToPlace;

        isPlacing = true;

        Destroy(indicator.gameObject); //removes the placeholder sphere for the actual model view to place the tower
        BaseTower placeTower = Instantiate(activeTower);
        placeTower.enabled = false;
        placeTower.GetComponent<Collider>().enabled = false; //disables thw tower script to stop it from scanning for enemies.
        indicator = placeTower.transform;

        placeTower.rangeModel.SetActive(true); //to showcase the range radius of a tower unit before placing.
        placeTower.rangeModel.transform.localScale = new Vector3(placeTower.range, 1f, placeTower.range);
    }

    public Vector3 GetGridPosition()
    {
        Vector3 location = Vector3.zero; //sets a default return value of 0 for the xyz values.


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //creates a raycast that spawns from the camera, going through the player's cursor
        Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red); //this is just for debugging purposes, only visible in scene view.

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200f, whatIsPlacement)) //checks for colliders within 200units of the cursor, and the code is only called with the detected layer is the "whatIsPlacement" layermask
        {
            location = hit.point; //stores the position of where the raycast hit
        }

        location.y = 0f; //fixate the position to y=0, the ground level

        return location;
    }

    public void MoveTowerSelectionEffect()
    {
        if (selectedTower != null)
        {
            selectedTowerEffect.transform.position = selectedTower.transform.position;
            selectedTowerEffect.SetActive(true); //VFX
        }
    }
}


