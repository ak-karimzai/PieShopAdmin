using Microsoft.EntityFrameworkCore;

namespace PieShopAdmin.Models.Repositories;

public class OrderRepository : IOrderRepository
{
    private PieShopDbContext _dbContext;

    public OrderRepository(PieShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetOrderDetailsAsync(int? orderId)
    {
        if (orderId != null)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Pie)
                .OrderBy(order => order.OrderId)
                .Where(order => order.OrderId == orderId.Value).FirstOrDefaultAsync();
            return order;
        }

        return null;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.OrderDetails)!
            .ThenInclude(orderDetail => orderDetail.Pie)
            .OrderBy(order => order.OrderId)
            .ToListAsync();
    }
}