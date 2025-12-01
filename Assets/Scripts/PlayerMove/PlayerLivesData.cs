using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLivesData", menuName = "Game/Player Lives Data")]
public class PlayerLivesData : ScriptableObject
{
    [System.Serializable]
    public class PlayerLifeInfo
    {
        public int playerIndex;
        public int lives = 3;
        public Vector2 spawnPosition;
    }

    public PlayerLifeInfo[] players = new PlayerLifeInfo[4];

    public void InitializePlayers(Vector2[] spawnPositions)
    {
        players = new PlayerLifeInfo[spawnPositions.Length];
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            players[i] = new PlayerLifeInfo
            {
                playerIndex = i,
                lives = 3,
                spawnPosition = spawnPositions[i]
            };
        }
    }

    public void ResetPlayer(int playerIndex, Vector2 spawnPosition)
    {
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            players[playerIndex].lives = 3;
            players[playerIndex].spawnPosition = spawnPosition;
        }
    }

    public bool LoseLife(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            players[playerIndex].lives--;
            return players[playerIndex].lives > 0;
        }
        return false;
    }

    public int GetLives(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            return players[playerIndex].lives;
        }
        return 0;
    }

    public Vector2 GetSpawnPosition(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < players.Length)
        {
            return players[playerIndex].spawnPosition;
        }
        return Vector2.zero;
    }
}