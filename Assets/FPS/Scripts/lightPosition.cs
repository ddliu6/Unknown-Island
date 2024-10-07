using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class lightPosition : MonoBehaviour
{
    public static lightPosition instance;
    public GameObject inGameMenu;
    public bool isnightvalue;
    private float _currentTime;
    public Light environmentlight;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        _currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.y >= 0) isnightvalue = false;
        else isnightvalue = true;

        if(isnightvalue)
        {
            environmentlight.gameObject.SetActive(false);
        }
        else
        {
            environmentlight.gameObject.SetActive(true);
        }

        if (_currentTime >= (2 * Mathf.PI))
        {
            _currentTime -= 2 * Mathf.PI;
        }
        if(!inGameMenu.activeSelf)
            _currentTime += (2 * Mathf.PI / 43200);

        this.transform.position = new Vector3(Mathf.Cos(_currentTime) * (-300), Mathf.Sin(_currentTime) * 300, -10);
    }
}
