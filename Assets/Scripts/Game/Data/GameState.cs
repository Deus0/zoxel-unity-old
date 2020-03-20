

namespace Zoxel
{
    
    [System.Serializable]
    public enum GameState
    {
        // First Part
        LoadingStartScreen,
        StartScreen,
        MainMenu,
        OptionsScreen,

        // Saved World
        SaveGamesScreen,
        //LoadSaveWorld,
        //LoadingSaveWorld,
        //SpawnSaveCharacters,

        // Select Character
        CharacterSelectScreen,
        //LoadPlayers,

        // Play Game
        InGame,
        GameUI,
        PauseScreen,

        // respawning
        RespawnScreen,
        RespawnCharacter,

        // return to main menu
        ClearingGame,

        LoadCharacter
    }
}