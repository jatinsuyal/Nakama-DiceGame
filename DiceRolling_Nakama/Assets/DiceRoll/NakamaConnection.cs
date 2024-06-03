using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using static UnityEngine.Networking.UnityWebRequest;
using UnityEngine.UI;

public class NakamaConnection : MonoBehaviour
{
    private List<Action> actionsToExecuteOnMainThread = new List<Action>();

    [SerializeField] private string scheme = "http";
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 7350;
    [SerializeField] private string serverKey = "defaultkey";
    [SerializeField] private GameObject gameObjectToDeactivate;  // Reference to the GameObject that will be deactivated
    [SerializeField] private Button rollBtn;

    private IClient client;
    private ISession session;
    private ISocket socket;
    private IMatch currentMatch;

    public Animator diceAnimator;  // Animator attached to a dice object
    public bool isRolling = false;

    async void Start()
    {
        // Initialize the Nakama client and session
        client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
        session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);

        // Establish socket connection
        socket = client.NewSocket();
        await socket.ConnectAsync(session);

        // Event listeners for matchmaking and match state
        socket.ReceivedMatchmakerMatched += OnReceivedMatchmakerMatched;
        socket.ReceivedMatchState += OnReceivedMatchState;

        Debug.Log("Session: " + session);
        Debug.Log("Socket connected");
    }

    void Update()
    {
        // Execute actions in the main thread that have been queued from other threads
        List<Action> actionsToExecute = new List<Action>(actionsToExecuteOnMainThread);
        actionsToExecuteOnMainThread.Clear(); // Correct method call to clear the list
        foreach (var action in actionsToExecute)
        {
            action();
        }
    }

    public async void FindMatch()
    {
        // Begin matchmaking process
        Debug.Log("Finding match...");
        var matchmakingTicket = await socket.AddMatchmakerAsync("*", 2, 2);
    }

    private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matchmakerMatched)
    {
        // Join the match received from matchmaking
        currentMatch = await socket.JoinMatchAsync(matchmakerMatched);
        Debug.Log("Joined Match ID: " + currentMatch.Id);

        // Queue deactivation of the GameObject when a match is successfully found
        QueueMainThreadAction(() => {
            if (gameObjectToDeactivate != null)
            {
                gameObjectToDeactivate.SetActive(false);
            }
        });
    }

    public void RollDice()
    {
        // Simulate a dice roll if not currently rolling and a match is ongoing
        if (!isRolling && currentMatch != null)
        {
            isRolling = true;
            int diceResult = UnityEngine.Random.Range(1, 7);  // Generate a random dice result
            StartCoroutine(PlayDiceRollAnimation(diceResult));
            SendDiceResult(diceResult);
        }
    }

    IEnumerator PlayDiceRollAnimation(int result)
    {
        // Handle dice roll animation
        rollBtn.interactable = false;
        diceAnimator.SetInteger("result", result);
        yield return new WaitForSeconds(1.5f);  // Duration for the dice roll animation
        diceAnimator.SetInteger("result", 0);  // Reset the dice result
        isRolling = false;
        rollBtn.interactable = true;
    }

    private void SendDiceResult(int result)
    {
        // Send the dice roll result to other players in the match
        var state = System.Text.Encoding.UTF8.GetBytes(result.ToString());
        socket.SendMatchStateAsync(currentMatch.Id, 0, state);
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        // Decode the match state and execute actions if needed
        var result = int.Parse(System.Text.Encoding.UTF8.GetString(matchState.State));
        QueueMainThreadAction(() => StartCoroutine(PlayDiceRollAnimation(result)));
    }

    private void QueueMainThreadAction(Action action)
    {
        // Queue an action to be executed on the main thread
        actionsToExecuteOnMainThread.Add(action);
    }
}
