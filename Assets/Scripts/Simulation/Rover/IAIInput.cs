/*
JIKAEL
An interface used to control the rover movement (throttle + steer)
*/
public interface IAIInput
{
    void SetControls(float throttle, float steer);
}
