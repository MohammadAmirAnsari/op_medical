using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OP.PORTAL.Data;
using OP.PORTAL.Models;

namespace OP.PORTAL.Services
{
    public interface IGeneralMasterService
    {
        Task<List<GeneralMaster>> GetAllAsync(bool onlyActive = true);
    }

    public class GeneralMasterService : IGeneralMasterService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GeneralMasterService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<GeneralMaster>> GetAllAsync(bool onlyActive = true)
        {
            using var _db = _contextFactory.CreateDbContext();
            return await _db.GeneralMasters.AsNoTracking().Where(g => onlyActive ? g.IsActive : true).
                OrderBy(x => x.Name).ToListAsync();
        }
    }
}
