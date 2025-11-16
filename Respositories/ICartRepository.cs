public interface ICartRepository
{
    Task<List<CartItem>> GetCartAsync(string sessionId, string? userId);
    Task SaveCartAsync(string sessionId, string? userId, List<CartItem> items);
    Task ClearCartAsync(string sessionId, string? userId);
}