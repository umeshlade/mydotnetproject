using System.Security.Claims;
using CarvedRockFitness.Models;
using CarvedRockFitness.Services;

public class ShoppingCartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CartEventService _cartEventService;

    private string? uniqueId { get; set; }

    private string? identityProvider { get; set; }

    public ShoppingCartService(ICartRepository cartRepository, IHttpContextAccessor httpContextAccessor, CartEventService cartEventService)
    {
        _cartRepository = cartRepository;
        _httpContextAccessor = httpContextAccessor;
        _cartEventService = cartEventService;
    }

    private string GetSessionId() =>
        _httpContextAccessor.HttpContext?.Session.Id ?? Guid.NewGuid().ToString();

    private string? GetUserId() {
        uniqueId = _httpContextAccessor.HttpContext?.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"];
        identityProvider = _httpContextAccessor.HttpContext?.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"];
        return (uniqueId != null && identityProvider != null) ? $"{identityProvider}{uniqueId}" : null;
    }

    public async Task<List<CartItem>> GetCartAsync()
    {
        var sessionId = GetSessionId();
        var userId = GetUserId();
        Console.WriteLine($"Session ID: {sessionId}");
        return await _cartRepository.GetCartAsync(sessionId, userId);
    }

    public async Task AddToCartAsync(Product product, int quantity)
    {
        var sessionId = GetSessionId();
        var userId = GetUserId();
        Console.WriteLine($"Session ID: {sessionId}");
        var cart = await _cartRepository.GetCartAsync(sessionId, userId);

        var item = cart.FirstOrDefault(x => x.ProductId == product.Id);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                UserId = userId ?? sessionId,
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow
            });
        }

        await _cartRepository.SaveCartAsync(sessionId, userId, cart);
        await _cartEventService.NotifyCartUpdatedAsync();
    }

    public async Task RemoveFromCartAsync(int productId)
    {
        var sessionId = GetSessionId();
        var userId = GetUserId();
        var cart = await _cartRepository.GetCartAsync(sessionId, userId);

        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            cart.Remove(item);
            await _cartRepository.SaveCartAsync(sessionId, userId, cart);
            await _cartEventService.NotifyCartUpdatedAsync();
        }
    }

    public async Task ClearCartAsync()
    {
        var sessionId = GetSessionId();
        var userId = GetUserId();
        await _cartRepository.ClearCartAsync(sessionId, userId);
        await _cartEventService.NotifyCartUpdatedAsync();
    }
}