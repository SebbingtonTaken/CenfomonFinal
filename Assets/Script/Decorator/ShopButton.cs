using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopButton : MonoBehaviour
{
    public IDecorator Decorator;
    private GameManager gameManager;
    private PlayerController playerController;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public void OnShopButtonClicked()
    {
        if (Decorator != null && gameManager != null)
        {
            playerController.ApplyDecorator(Decorator);
            gameManager.AddPurchasedDecorator(Decorator);
        }
        StartCoroutine(LoadNextSceneWithDelay()); // A�ade el retraso antes de cargar la pr�xima escena
    }

    private IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(0.5f); 
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex - 5; 
        SceneManager.LoadScene(nextSceneIndex);
    }
}
