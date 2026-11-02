using UnityEngine;


public class MoneyManager : MonoBehaviour
{
    public static MoneyManager instance; //notes and changes to be updated

    private void Awake()
    {
        instance = this;
    }

    public int currentMoney;

    // Start is called before the first frame update
    void Start()
    {
        UIManager.instance.goldText.text = currentMoney.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GiveMoney(int amountToGive)
    {
        currentMoney += amountToGive;

        UIManager.instance.goldText.text = currentMoney.ToString();
    }

    public bool SpendMoney(int amountToSpend)
    {
        bool canSpend = false;

        if (amountToSpend <= currentMoney)
        {
            canSpend = true;

            Debug.Log("Spent " + amountToSpend);
            currentMoney -= amountToSpend;
        }

        UIManager.instance.goldText.text = currentMoney.ToString();

        return canSpend;
    }
}
