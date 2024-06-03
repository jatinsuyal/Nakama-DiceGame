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
    [SerializeField] private int port = 7351;
    [SerializeField] private string serverKey = "defaultkey";
    [SerializeField] Button rollBtn;

    private IClient client;
    private ISession session;
    private ISocket socket;
    private IMatch currentMatch;

    public Animator diceAnimator; // Animator attached to the dice
    public bool isRolling = false;

    async void Start()
    {
        client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
        session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);
        socket = client.NewSocket();
        await socket.ConnectAsync(session, true);

        socket.ReceivedMatchmakerMatched += OnReceivedMatchmakerMatched;
        socket.ReceivedMatchState += OnReceivedMatchState;

        Debug.Log(session);
        Debug.Log(socket);
    }

    void Update()
    {
        // Copying actions to a separate list to execute
        List<Action> actionsToExecute = new List<Action>(actionsToExecuteOnMainThread);
        actionsToExecuteOnMainThread.Clear();
        foreach (var action in actionsToExecute)
        {
            action();
        }
    }

    public async void FindMatch()
    {
        Debug.Log("Finding match");
        var matchmakingTicket = await socket.AddMatchmakerAsync("*", 2, 2);
    }

    private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matchmakerMatched)
    {
        currentMatch = await socket.JoinMatchAsync(matchmakerMatched);
        Debug.Log("Joined Match ID: " + currentMatch.Id);
    }

    public void RollDice()
    {
        if (!isRolling && currentMatch != null)
        {
            isRolling = true;
            int diceResult = UnityEngine.Random.Range(1, 7); // Simulate the dice roll result
            StartCoroutine(PlayDiceRollAnimation(diceResult));
            SendDiceResult(diceResult);
        }
    }

    IEnumerator PlayDiceRollAnimation(int result)
    {
        rollBtn.interactable = false;
        diceAnimator.SetInteger("result", result);
        yield return new WaitForSeconds(1.5f); // Assume the animation takes 2 seconds
        diceAnimator.SetInteger("result", 0); // Reset the result to prevent retriggering
        isRolling = false; // Reset rolling state
        rollBtn.interactable = true;
        // SendDiceResult(result);
    }

    private void SendDiceResult(int result)
    {
        var state = System.Text.Encoding.UTF8.GetBytes(result.ToString());
        socket.SendMatchStateAsync(currentMatch.Id, 0, state);
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        var result = int.Parse(System.Text.Encoding.UTF8.GetString(matchState.State));
        QueueMainThreadAction(() => StartCoroutine(PlayDiceRollAnimation(result)));
    }

    private void QueueMainThreadAction(Action action)
    {
        actionsToExecuteOnMainThread.Add(action);
    }
}
