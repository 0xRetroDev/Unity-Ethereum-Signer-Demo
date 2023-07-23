using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;
using static CreateWallet;

public class SendTransaction : MonoBehaviour
{
    // Singleton to invoke our API requests 
    public static SendTransaction instance;

    private void Start()
    {
        // Singleton
        instance = this;    
    }

    // This script is an example of where we could create a name space or class for sending different API requests to the server.
    // In this example we'll look at sending a request

    /// <summary>
    ///  The Solidity method we're calling for the below is as follows:
    ///  
    ///  uint256 public tokenCount; 
    ///   
    ///  function TokenCollected() external {
    ///  tokenCount++;
    ///  }
    /// 
    /// 
    /// </summary>


    // Method to call when the signTransaction event should be triggered (e.g., when a player collects a token in the game)
    public void PlayerCollectedToken()
    {
        // Send a request to the server to invoke the TokenCollected contract method for the player
        StartCoroutine(MakeAPIRequest());
    }

    // Queue our request to the API server
    public IEnumerator MakeAPIRequest()
    {
        Debug.Log("Adding transaction request to server queue");

        using (var client = new HttpClient())
        {
            var requestData = new RequestData
            {
                playerId = CreateWallet.instance.playerId
            };
            var json = JsonUtility.ToJson(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call our API endpoint ./signTransaction
            var request = client.PostAsync(CreateWallet.instance.apiBaseUrl + "/signTransaction", content);
            yield return new WaitUntil(() => request.IsCompleted);

            if (request.Exception != null)
            {
                Debug.LogError("Error calling method: " + request.Exception.Message);
            }
            else if (request.Result.IsSuccessStatusCode)
            {
                Debug.Log("Transaction successfully mined for " + CreateWallet.instance.playerId);
            }
            else
            {
                Debug.LogError("Error calling method: " + request.Result.ReasonPhrase);
            }
        }
    }

}
