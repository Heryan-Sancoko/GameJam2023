using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroManager : MonoBehaviour
{
    public static HeroManager instance;

    public PlayerInput shovelInput;
    public HeroBehaviour shovelBehaviour;
    public PlayerInput swordInput;
    public HeroBehaviour swordBehaviour;
    [SerializeField]
    private HeroBehaviour.heroType selectedHero;
    public HeroBehaviour.heroType SelectedHero
    {
        get 
        { 
            return selectedHero; 
        }
        set 
        {
            selectedHero = value;
        }
    }

    public void SwapCharacters()
    {
        switch (selectedHero)
        {
            case HeroBehaviour.heroType.shovel:
                shovelBehaviour.enabled = false;
                shovelInput.enabled = false;
                //shovelBehaviour.gameObject.SetActive(false);
                swordBehaviour.enabled = true;
                swordInput.enabled = true;
                //swordBehaviour.gameObject.SetActive(true);
                selectedHero = HeroBehaviour.heroType.sword;
                break;
            case HeroBehaviour.heroType.sword:
                swordBehaviour.enabled = false;
                swordInput.enabled = false;
                //swordBehaviour.gameObject.SetActive(false);
                shovelBehaviour.enabled = true;
                shovelInput.enabled = true;
                //shovelBehaviour.gameObject.SetActive(true);
                selectedHero = HeroBehaviour.heroType.shovel;
                break;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
