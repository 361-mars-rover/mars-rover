public interface IAIController
{
    /// <summary>
    /// Called by CarControl to update the rover.
    /// The controller is responsible for computing the desired controls and then calling IAIInput.SetControls.
    /// </summary>
    void UpdateRover();
}