using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{

    [Serializable]
    struct PinConfiguration {
        public string name;
        public int pinCount;
        public GameObject pinPrefab;
    }

    [Serializable]
    struct BallSizes {
        public string name;
        public float size;
    }

    [SerializeField] private PinConfiguration[] pinConfigurations;
    [SerializeField] private BallSizes[] ballSizes;

    [Header("Game Settings")]
    [SerializeField, NaughtyAttributes.ReadOnly] private int maxRounds = 10;
    [SerializeField, NaughtyAttributes.ReadOnly] private int pinConfiguration = 0;
    [SerializeField, NaughtyAttributes.ReadOnly] private float ballSize = 0;

    [Header("Main Menu UI")]
    [SerializeField] private UIDocument menuUI;
    [SerializeField] private VisualTreeAsset playerFieldVisualTreeAsset;
    [SerializeField] private VisualTreeAsset mainMenuVisualTreeAsset;
    [SerializeField] private VisualTreeAsset helpMenuVisualTreeAsset;
    [SerializeField] private VisualTreeAsset gameOverMenuVisualTreeAsset;

    [Header("Scoreboard")]
    [SerializeField] private Camera scoreboardCamera;
    private ScoreboardManager scoreboardManager;

    [Header("Game Objects")]
    [SerializeField] private GameObject player;

    // Private fields
    private bool isPlaying = false;
    private List<Player> players = new List<Player>();
    private int round = 1;

    private Pin[] pins;
    private Ball ball;
    private CloseupCamera closeupCamera;
    private Transform pinLocation;
    private int currentPlayer = 0;

    private IEnumerator EndThrow() {
        // Wait for 2 seconds after the ball has entered the trigger
        yield return new WaitForSeconds(2);

        closeupCamera.DisableCamera();

        // Activate the scoreboard camera
        scoreboardCamera.gameObject.SetActive(true);
        
        // Wait one second before updating the score
        yield return new WaitForSeconds(1);

        // Get the number of fallen pins
        int fallenPins = GetNumberOfFallenPins();

        // Check if it is the last round
        bool lastRound = round == maxRounds;

        // Get the maximum number of pins that can fall in one throw
        int pinCount = pinConfigurations[pinConfiguration].pinCount;

        // Get the player and his latest score field
        Player player = players[currentPlayer];
        Score score = player.scores[player.scores.Count - 1];

        // Disable all fallen pins
        foreach (Pin pin in pins) {
            if(pin.HasFallen) {
                pin.gameObject.SetActive(false);
            }
        }

        // Check if it is the first throw
        if(score.firstThrow == -1) {
            // Set the first throw score
            score.firstThrow = fallenPins;

            // Check if all pins have fallen
            if(fallenPins == pinCount) {
                // If it is the last round, reset the pins otherwise end the turn
                if(lastRound) {
                    ResetPins();
                } else {
                    EndTurn();
                }
            }
        // Check if it is the second throw
        } else if (score.secondThrow == -1) {
            // Set the second throw score
            score.secondThrow = fallenPins;

            // Check if the player has scored a strike or a spare
            if(score.firstThrow + score.secondThrow >= pinCount) {
                // Check if it is the last round
                if(lastRound) {
                    ResetPins();
                // If it is not the last round, end the turn
                } else {
                    EndTurn();
                }
            // If the player has not scored a spare or a strike, end the turn
            } else {
                EndTurn();
            }

        // If both the first and second throws have been set, set the bonus throw score (only applicable for the last round)
        } else {
            // Set the bonus throw score
            score.bonusThrow = fallenPins;
            // End the turn
            EndTurn();
        }

        // Update the player's score fields
        UpdateScores(player);

        // Update the scoreboard UI
        scoreboardManager.UpdateScoreboardUI(players, currentPlayer);

        // Wait for 2 seconds before deactivating the scoreboard camera
        yield return new WaitForSeconds(2);

        // If the round is one more than max rounds, the game is over
        if(round > maxRounds) {
            OpenGameOverMenu();
            EndGame();
        }

        // Deactivate the scoreboard camera
        scoreboardCamera.gameObject.SetActive(false);

        // Reset the ball
        ball.Reset();

    }

    private void EndTurn() {

        // If it is not the last round, add a new score for the next turn
        if(round < maxRounds) {
            AddNewScore(players[currentPlayer]);
        }

        // Switch to the next player
        currentPlayer++;
        // Mod the current player index by the number of players to loop back to the first player
        currentPlayer %= players.Count;

        // If the current player is the first player, increment the round
        if(currentPlayer == 0) {
            round++;
        }

        // Reset the pins
        ResetPins();
    }

    private int GetNumberOfFallenPins() {
        int fallenPins = 0;

        // Count the number of fallen pins
        foreach (Pin pin in pins) {
            // Only count the pin if it is active and has fallen over
            if(pin.gameObject.activeSelf && pin.HasFallen) {
                fallenPins++;
            }
        }

        // Return the number of fallen pins
        return fallenPins;
    }

    private void ResetPins() {
        // Reset all pins
        foreach (Pin pin in pins) {
            pin.Reset();
        }
    }

    private void AddNewScore(Player player) {
        // Add a new score field to the given player
        player.scores.Add(new Score() {
            firstThrow = -1,
            secondThrow = -1,
            totalScore = -1,
            bonusThrow = -1
        });
    }

    void UpdateScores(Player player) {

        // Get the maximum number of pins that can fall in one throw
        int maxFallenPins = pinConfigurations[pinConfiguration].pinCount;

        // Loop through all scores and calculate the total score for each frame
        for(int i=0; i < player.scores.Count; i++) {

            // Get a reference to the current score
            Score score = player.scores[i];
            // Get the previous score total or 0 if it is the first frame
            int previousScore = i > 0 ? player.scores[i - 1].totalScore : 0;

            // If there is no first throw, break the loop
            if(score.firstThrow == -1) {
                break;
            }

            // Special case for the last frame
            if(i == maxRounds - 1) {

                // If the second throw has not been set, break the loop
                if(score.secondThrow == -1) {
                    break;
                }

                // If the player has not scored a strike or a spare on the second throw, calculate the total score for the last frame 
                if(score.firstThrow + score.secondThrow < maxFallenPins) {
                    score.totalScore = previousScore + score.firstThrow + score.secondThrow;
                    break;
                }

                // If the player has scored a strike or a spare on the second throw, check for the bonus throw
                if(score.bonusThrow == -1) {
                    break;
                }

                // Calculate the total score for the last frame if the bonus throw has been set
                score.totalScore = previousScore + score.firstThrow + score.secondThrow + score.bonusThrow;
                break;
            }

            // Check if the first throw is a strike
            if(score.firstThrow == maxFallenPins) {

                // Get the next score
                Score nextScore = i + 1 < player.scores.Count ? player.scores[i + 1] : null;

                // If the next score is null, break the loop
                if(nextScore == null) {
                    break;
                }

                // If the first throw of the next score has not been set, break the loop
                if(nextScore.firstThrow == -1) {
                    break;
                }
                
                // If the next throw is also a strike, check for the next next score
                if(nextScore.firstThrow == maxFallenPins) {

                    // Special case for the before last frame
                    if(i == (maxRounds - 2)) {
                        // If the last frame's second throw has not been set, break the loop
                        if(nextScore.secondThrow == -1) {
                            break;
                        }
                        // Calculate the total score for the before last frame
                        score.totalScore = previousScore + score.firstThrow + nextScore.firstThrow + nextScore.secondThrow;
                        continue;
                    }

                    // Get the next next score
                    Score nextNextScore = i + 2 < player.scores.Count ? player.scores[i + 2] : null;

                    // If the next next score is null, break the loop
                    if(nextNextScore == null) {
                        break;
                    }

                    // If the first throw of the next next score has not been set, break the loop
                    if(nextNextScore.firstThrow == -1) {
                        break;
                    }

                    // Calculate the total score for the current frame
                    score.totalScore = previousScore + score.firstThrow + nextScore.firstThrow + nextNextScore.firstThrow;
                } else {
                    // If the next throw is not a strike, check for the second throw, break the loop if it has not been set
                    if(nextScore.secondThrow == -1) {
                        break;
                    }
                    // Calculate the total score for the current frame 
                    score.totalScore = previousScore + score.firstThrow + nextScore.firstThrow + nextScore.secondThrow;
                }
            // Check if the score is a spare
            } else if (score.firstThrow + score.secondThrow == maxFallenPins) {

                // Get the next score
                Score nextScore = i + 1 < player.scores.Count ? player.scores[i + 1] : null;

                // If the next score is null, break the loop
                if(nextScore == null) {
                    break;
                }

                // If the first throw of the next score has not been set, break the loop
                if(nextScore.firstThrow == -1) {
                    break;
                }

                // Calculate the total score for the current frame
                score.totalScore = previousScore + score.firstThrow + score.secondThrow + nextScore.firstThrow;

            } else {
                // If the score is not a spare or a strike, check for the second throw

                // If the second throw has not been set, break the loop
                if(score.secondThrow == -1) {
                    break;
                }

                // Calculate the total score for the current frame
                score.totalScore = previousScore + score.firstThrow + score.secondThrow;
            }

        }

    }

    void ChangePinConfiguration(int value) {
        pinConfiguration = value;
    }

    void ChangeBallSize(int value) {
        ballSize = value;
    }

    void ChangeNumberOfRounds(float rounds) {
        maxRounds = Mathf.Clamp((int)rounds, 1, 10);
    }

    void StartGame() {

        isPlaying = true;
        round = 1;
        currentPlayer = 0;

        foreach(Player p in players) {
            p.scores.Clear();
        }

        GameObject pinsInstance = Instantiate(pinConfigurations[pinConfiguration].pinPrefab, pinLocation.position, pinLocation.rotation, pinLocation);
        pins = pinsInstance.transform.GetComponentsInChildren<Pin>().Where(pin => pin.gameObject.CompareTag("Pin")).ToArray();

        // Add initial scores for all players
        foreach (Player player in players) {
            AddNewScore(player);
        }
        // Update the scoreboard UI
        scoreboardManager.Configure(maxRounds, pinConfigurations[pinConfiguration].pinCount);
        scoreboardManager.UpdateScoreboardUI(players, currentPlayer);

        float size = ballSizes[(int)ballSize].size;

        ball.transform.localScale = new Vector3(size, size, size);

    }

    void EndGame() {

        isPlaying = false;
        players.Clear();
        foreach (Transform t in pinLocation) {
            Destroy(t.gameObject);
        }
        pins = new Pin[0];

    }

    void ExitGame() {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void OpenGameOverMenu() {

        menuUI.visualTreeAsset = gameOverMenuVisualTreeAsset;

        VisualElement root = menuUI.rootVisualElement;

        Player winner = players.OrderByDescending(player => player.scores.Last().totalScore).First();

        Label winnerLabel = root.Q<Label>("winner");
        Button playAgainButton = root.Q<Button>("playAgainButton");

        winnerLabel.text = $"The winner is {winner.name} with a score of {winner.scores.Last().totalScore}";

        playAgainButton.clicked += () => {
            OpenMainMenu();
        };

    }

    void OpenHelpMenu() {

        menuUI.visualTreeAsset = helpMenuVisualTreeAsset;

        VisualElement root = menuUI.rootVisualElement;

        Button backButton = root.Q<Button>("backButton");

        backButton.clicked += () => {
            OpenMainMenu();
        };
    }

    void OpenMainMenu() {

        menuUI.visualTreeAsset = mainMenuVisualTreeAsset;

        VisualElement root = menuUI.rootVisualElement;

        DropdownField pinConfigurationDropdown = root.Q<DropdownField>("pinConfigurationDropdown");
        DropdownField ballSizeDropdown = root.Q<DropdownField>("ballSizeDropdown");
        SliderInt roundSlider = root.Q<SliderInt>("roundSlider");
        Button addPlayerButton = root.Q<Button>("addPlayerButton");
        Button startButton = root.Q<Button>("startGameButton");
        Button helpButton = root.Q<Button>("helpButton");
        Button exitButton = root.Q<Button>("exitGameButton");
        VisualElement playerListView = root.Q<VisualElement>("playerListView");

        // Fill the dropdown values
        pinConfigurationDropdown.choices = pinConfigurations.Select(config => config.name).ToList();
        ballSizeDropdown.choices = ballSizes.Select(size => size.name).ToList();

        // Add event listeners to the UI elements
        pinConfigurationDropdown.RegisterValueChangedCallback(e => ChangePinConfiguration(pinConfigurationDropdown.index));
        ballSizeDropdown.RegisterValueChangedCallback(e => ChangeBallSize(ballSizeDropdown.index));
        roundSlider.RegisterValueChangedCallback(e => ChangeNumberOfRounds(roundSlider.value));
        startButton.clicked += StartGame;
        exitButton.clicked += ExitGame;
        helpButton.clicked += () => {
            OpenHelpMenu();
        };

        // Set default values
        roundSlider.value = 10;
        pinConfigurationDropdown.index = 0;
        ballSizeDropdown.index = 0;

        addPlayerButton.clicked += () => {
            Player player = new Player();
            players.Add(player);

            VisualElement playerField = playerFieldVisualTreeAsset.CloneTree();
            playerField.Q<TextField>().value = player.name;
            playerField.Q<TextField>().RegisterValueChangedCallback(e => {
                player.name = e.newValue;
                CheckPlayerNames(startButton);
            });

            playerField.Q<Button>().clicked += () => {
                players.Remove(player);
                playerListView.Remove(playerField);

                if(players.Count < 6) {
                    addPlayerButton.SetEnabled(true);
                }

                CheckPlayerNames(startButton);
            };

            playerListView.Add(playerField);

            if(players.Count >= 6) {
                addPlayerButton.SetEnabled(false);
            }

            CheckPlayerNames(startButton);

        };

        startButton.SetEnabled(false);
    }

    void CheckPlayerNames(Button startButton) {

        if(players.Count == 0) {
            startButton.SetEnabled(false);
            return;
        }

        foreach(Player p in players) {

            if(p.name.Trim() == "") {
                startButton.SetEnabled(false);
                return;
            }

        }

        startButton.SetEnabled(true);

    }

    void Awake() {
        // Find all the gameObjects
        ball = GameObject.FindGameObjectWithTag("Player").GetComponent<Ball>();
        closeupCamera = FindObjectOfType<CloseupCamera>();
        pinLocation = GameObject.FindGameObjectWithTag("PinLocation").transform;
        scoreboardManager = FindObjectOfType<ScoreboardManager>();

        // Deactivate the scoreboard camera
        scoreboardCamera.gameObject.SetActive(false);

    }

    void Start() {
        OpenMainMenu();
    }

    void Update() {

        if(Input.GetKeyDown(KeyCode.F5)) {
            StartCoroutine(EndThrow());
        }

        menuUI.gameObject.SetActive(!isPlaying);
        player.SetActive(isPlaying);
    }

    void OnTriggerEnter(Collider other) {
        // Check if the ball has entered the trigger (ground) and start the end throw coroutine
        if (other.CompareTag("Player")) {
            StartCoroutine(EndThrow());
        }
    }

    public void SetMaxRounds(float rounds) {
        maxRounds = Math.Clamp((int)rounds, 1, 10);
    }

    public void SetPinConfiguration(int configuration) {
        pinConfiguration = Math.Clamp(configuration, 0, pinConfigurations.Length - 1);
    }

    public void SetBallSize(double size) {
        ballSize = Math.Clamp((float)size, 0.5f, 1.5f);
    }

}
