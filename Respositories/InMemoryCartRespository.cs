public class InMemoryCartRepository : ICartRepository
{
    private readonly Dictionary<string, List<CartItem>> _carts = new();

    public Task<List<CartItem>> GetCartAsync(string sessionId, string? userId)
    {
        string key = userId ?? sessionId;
        if (!_carts.ContainsKey(key))
        {
            _carts[key] = new List<CartItem>();
        }
        return Task.FromResult(_carts[key]);
    }

    public Task SaveCartAsync(string sessionId, string? userId, List<CartItem> items)
    {
        string key = userId ?? sessionId;
        _carts[key] = items;
        return Task.CompletedTask;
    }

    public Task ClearCartAsync(string sessionId, string? userId)
    {
        string key = userId ?? sessionId;
        _carts.Remove(key);
        return Task.CompletedTask;
    }
}