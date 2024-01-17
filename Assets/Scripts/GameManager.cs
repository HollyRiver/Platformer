using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerMovement PlayerCtrl;
    public GameObject[] Stage;
    public int PlayerScore;
    public int StageIndex;
    public int Health;
    public GameObject[] UIHealth;
    public TextMeshProUGUI UIScore;
    public TextMeshProUGUI UIStage;
    public GameObject UIRetry;
    public GameObject UIClear;

    void Awake()
    {
        PlayerScore = 0;
    }

    void Update()
    {
        UIScore.text = PlayerScore.ToString();
    }

    public void NextStage() {
        if (StageIndex < Stage.Length - 1) {
            Stage[StageIndex].SetActive(false);
            StageIndex ++;
            Stage[StageIndex].SetActive(true);

            PlayerCtrl.Reposition();            
            UIStage.text = "Stage" + StageIndex;
        }

        else {
            UIClear.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerDamaged")) {
            if (Health > 1) {
                HealthDown();
                PlayerCtrl.Respown();
            }

            else {
                PlayerCtrl.Dead();
                UIRetry.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    public void HealthDown() {
        Health --;
        UIHealth[Health].SetActive(false);

        if (Health <= 0) {
            PlayerCtrl.Dead();
            UIRetry.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void RetryGame() {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }
}