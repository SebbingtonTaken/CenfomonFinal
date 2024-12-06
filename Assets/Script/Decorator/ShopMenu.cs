using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;

    public class ShopMenu : MonoBehaviour
    {
        public static ShopMenu instance;
        [SerializeField] public Shop shop;
        public PlayerController playerController;
        public static bool GameIsPaused = false;
        public GameObject pauseMenuUI;
        public ControladorPuntos controladorPuntos;
        public FlyWeight flyWeight;
        public IFlyWeightFactory iflyWeightFactory;
        
        void Start()
        {
        controladorPuntos = FindObjectOfType<ControladorPuntos>();
        pauseMenuUI.SetActive(false);
            iflyWeightFactory = new IFlyWeightFactory();


        }


        void Update()
        {
        
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (GameIsPaused)
                {
                    Input.GetKeyDown(KeyCode.B);
                    Resume();
                }
                else
                {
                    Pause();

                }
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;
        }

        void Pause()
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsPaused = true;
        }


        public void Zapaz()
        {
        FlyWeight flyweight = shop.GetItemFlyweight("Zapaz");
        if (HasEnoughPoints(flyweight.itemPrice))
        {
            DeductPoints(flyweight.itemPrice);
            IDecorator decorator = new SpeedBoostDecorator(3f);
            playerController.ApplyDecorator(decorator);
        }
        else
            {
           
            }

            Resume();
        }

        private bool HasEnoughPoints(int amount)
        {
            return controladorPuntos.CantidadPuntos >= amount;
        }

        private void DeductPoints(int amount)
        {
            controladorPuntos.CantidadPuntos -= amount;
        }
     
    }
