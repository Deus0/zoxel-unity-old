using Unity.Entities;

namespace Zoxel
{
    /// <summary>
    /// Handles Game in NewGameState (move to game systems)
    ///     - First disable all cameras but first player
    ///     - Then spawn UI for Game Generation
    ///     - Shows World Map and a generate button
    ///     - also a confirm and back button
    ///     [   m   a   p             ]
    ///     [back] [generate] [confirm]
    ///     - Once confirm - save that seed in a game data
    ///     - use SaveSystem to save Game folder
    ///     - UI Flow:
    ///         - NewGameUI
    ///         - New Character UI (show character in middle - class choice to be anchored on right)
    ///         - Race Choice anchored on left next
    ///         - Custom character UI - chose between different heads - chests - etc (body parts)
    ///         - then chose a starting Location
    ///         - Chose name for character and it will fade out screen and fade in to character (load it into new world)
    /// </summary>
    [DisableAutoCreation]
    public class NewGameSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {

        }
    }
}
