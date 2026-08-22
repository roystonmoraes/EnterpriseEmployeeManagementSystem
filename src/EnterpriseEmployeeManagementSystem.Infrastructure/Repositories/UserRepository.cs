using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using EnterpriseEmployeeManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.FirstOrDefaultAsync(
            u => u.Username == username,
            cancellationToken
        );
    }

    public async Task<User?> GetByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Users.FirstOrDefaultAsync(
            u => u.RefreshToken == refreshToken,
            cancellationToken
        );
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
