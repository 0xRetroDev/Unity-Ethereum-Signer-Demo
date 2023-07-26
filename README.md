<h1 align="center">Unity Ethereum Signer Demo</h1>

<p align="center">
  <b>Unity Ethereum Wallet Generation and Transaction Signer</b>
  <br>
  <i>Unlock the true potential of your Unity game with invisible Ethereum wallet generation and transaction signing.</i>
</p>

<p align="center">
  <img src="https://user-images.githubusercontent.com/97366705/231331134-fb9d64c2-f3b8-404b-9cae-12124f9bbfe5.png" width=600>
  <br><br>
<b>Unity Ethereum Signer API:</b> https://github.com/nftpixels/Unity-Ethereum-Signer-API
</p>

## Table of Contents

- [Introduction](#introduction)
- [Installation](#installation)
- [Usage](#usage)
- [License](#license)
- [Author](#author)

## Introduction

In this very simple Unity demo we look at how we can create a new signer (wallet) for our player when he starts the game, and invoke a smart contract method when he collects coins. We'll be using the [Unity Ethereum Signer API](https://github.com/nftpixels/Unity-Ethereum-Signer-API). Pleae make sure that the server is running and ready to receive requests before proceeding.
<br>


## Installation

1. **Clone this repository to your local machine.**

```bash

git clone https://github.com/nftpixels/Unity-Ethereum-Signer-Demo.git
cd Unity-Ethereum-Signer-Demo
```
<br>

2. **Open the project in Unity**
   
Add the project to your Unity Hub by clicking on the **dropdown arrow** and then **add project from disk** to locate the project folder.
<br>
<br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/cfe0c008-e26f-4030-adf7-e7c0064468bc)


3. **Ensure the API server is running:**

Double check your node API server is live and listening to _https://localhost:3000_ (This can be changed on the server and Unity to work with any hosted server)
<br>
<br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/04024bca-2185-4d33-b3a9-daaed6426d80)

<br>
<br>

4. **Play the CreateWallet Scene**

The demo is configured to work with any localhost server listening on port 3000 for educational purposes. If your server is running and you play the **CreateWallet** scene, you should be able to click on **Play** to generate a new wallet and send Gas to it using an external API configured for Gas distribution. 
<br>
<br>
You can expect to see the below printed in the editor console: 
<br>
<br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/23304c0c-c068-4de7-a356-fc8089e2ac34)
<br><br>
Upon switching to the new scene and collecting the coins with the character, you should see the below logged to the console, as we invoke a request to the server to sign our specified smart contract method:
<br><br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/c622829f-1b42-40d3-a9be-a0c2542c67c3)
<br><br>

5. **Check the server logs**

The server will debug everything for you as the requests come in. We make use of a queue to help ensure all requests get executed in a sequential manner. This prevents nonce issues among other things. The below is what you can expect from the server console for all the incoming requests:
<br><br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/305ad3f0-3ea9-4eb1-ae5a-3ffd23da7bb3)
<br><br>

It's worth noting that the server will check the **playerId** when a new wallet creation request comes in. If the **playerId** already has a matching entry and signer, the server will respond with an error message that will be received within the unity client. You can use this to validate any required game logic that requires you to check if a player already has a signer. In this demo, we simply load the next scene once we know that the player has an existing signer wallet:
<br><br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/f7e3c53b-b940-4287-b98f-36381024e674)
<br><br>
![image](https://github.com/nftpixels/Unity-Ethereum-Signer-Demo/assets/97366705/fa37383a-557f-43c2-b76d-a8d11cc7901c)
<br><br>






## Usage

**Generate Wallet:**

Endpoint: POST **_/generateWallet_**

Generate a new Ethereum wallet for each player at the beginning of the game. Players will receive a unique wallet address to interact with the blockchain. In the **CreateWallet.cs** script we pass the player GUID as the **playerId**. You can however pass any data as a player ID, or even use session tokens.

**Unity Example:**

```c#
    private void Start()
    {
        // If the connecting player doesn't have a playerId, we generate a new one and store it the playerPrefs to be reused (This is optional and you can remove it if you prefer to generate a brand new signer for every session)
        if (!PlayerPrefs.HasKey("playerID"))
        {
            Debug.Log("No ID Found, Generating a new one");
            string newPlayerID = Guid.NewGuid().ToString();

            // Generate a unique playerId for the player and store it in playerPrefs
            PlayerPrefs.SetString("playerID", newPlayerID);
            playerId = newPlayerID;
        }
        else
        {
            playerId = PlayerPrefs.GetString("playerID");
        }

        // Invoke our signer method
        GenerateSigner();
    }


    // Invoke this method to generate a new signer and assign it to the playerID
    public async Task GenerateSigner()
    {
        Debug.Log("Your PlayerID: " + playerId);

        // Create the JSON payload with playerId
        var requestData = new RequestData
        {
            playerId = playerId
        };
        var json = JsonUtility.ToJson(requestData);
        var contentBytes = Encoding.UTF8.GetBytes(json);

        // Call our API endpoint ./generateWallet
        using (var request = UnityWebRequest.PostWwwForm(apiBaseUrl + "/generateWallet", ""))
        {
            // Set the request content type
            request.SetRequestHeader("Content-Type", "application/json");

            // Set the request data
            request.uploadHandler = new UploadHandlerRaw(contentBytes);

            // Send the request
            await request.SendWebRequest();

            Debug.Log("Generating Wallet for: " + playerId);

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseJson = request.downloadHandler.text;
                var responseData = JsonUtility.FromJson<GenerateWalletResponse>(responseJson);
                Debug.Log("Wallet generated for player " + playerId + " - Address: " + responseData.address);

                // Adding our wallet address variable
                walletAddress = responseData.address;

                // Provide gas to the created wallet (You'll need to configure your own gas distribution API for the below)
                // SKALE network does this best as their gas token has no value, so you can mine transactions and distribute gas for free.
                StartCoroutine(SendGas("https://corsproxy.io/?https://example-gas-api.onrender.com/claim/" + walletAddress));
            }
            else
            {
                {
                    // Check if there's an error in the response data
                    var responseJson = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(responseJson))
                    {
                        // Deserialize the JSON error message from the response
                        var errorData = JsonUtility.FromJson<ErrorResponse>(responseJson);
                        Debug.LogError("Error generating wallet: " + errorData.message);

                        // The only reason we'd get this error is to let us know we already have a signer, which means we can load the level
                        SceneManager.LoadScene("GameScene");
                    }
                    else
                    {
                        // If there's no response data or error message, display the generic error
                        Debug.LogError("Error generating wallet: " + request.result);
                    }
                }
            }
        }
    }
```
<br>


**Sign Transaction:**

Endpoint: POST **_/signTransaction_**

Call the TokenCollected method on the example contract for a player when they collect tokens in the game. This will initiate a transaction using their generated wallet.
This is just an **example** method and can be replaced with any method and endpoint. _eg. /mintNFT could invoke a mint function on the node server and use parameters passed from the Unity client._

<br>
<br>

**Unity Example:**

```c#
    // Queue our request to the API server
    public IEnumerator MakeAPIRequest()
    {
        Debug.Log("Adding transaction request to server queue");

        using (var client = new HttpClient())
        {
            var requestData = new RequestData
            {
                // Pass the player ID generated from the wallet API
                playerId = playerId
            };
            var json = JsonUtility.ToJson(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call our API endpoint ./signTransaction - This can be any method and endpoint however
            var request = client.PostAsync(CreateWallet.instance.apiBaseUrl + "/signTransaction", content);
            yield return new WaitUntil(() => request.IsCompleted);

            if (request.Exception != null)
            {
                Debug.LogError("Error calling method: " + request.Exception.Message);
            }
            else if (request.Result.IsSuccessStatusCode)
            {
                Debug.Log("Transaction successfully mined for " + playerId);
            }
            else
            {
                Debug.LogError("Error calling method: " + request.Result.ReasonPhrase);
            }
        }
    }
```
<br>
<br>

## License
This project is licensed under the MIT License.

## Author
* Author: Reinhardt Weyers <br>
* Email: weyers70@gmail.com <br>
* GitHub: [github.com/yourusername](https://github.com/nftpixels) <br>
* LinkedIn: https://www.linkedin.com/in/reinhardtweyers/ <br>
