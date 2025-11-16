namespace CarvedRockFitness.Services;
public class CartEventService
{
    public event Func<Task>? CartUpdated;

    public async Task NotifyCartUpdatedAsync()
    {
        if (CartUpdated != null)
        {
            await CartUpdated.Invoke();
        }
    }
}