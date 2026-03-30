using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data;
using Microsoft.EntityFrameworkCore;

public class BossStateRepository
{
    private readonly GameDbContext _context;

    public BossStateRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(DateTime date, string key)
    {
        _context.BossSpawnStates.Add(new BossSpawnState
        {
            Date = date.Date,
            Key = key
        });

        await _context.SaveChangesAsync();
    }

    public async Task<HashSet<string>> LoadForDateAsync(DateTime date)
    {
        var list = await _context.BossSpawnStates
            .Where(x => x.Date == date.Date)
            .Select(x => x.Key)
            .ToListAsync();

        return list.ToHashSet();
    }

    public async Task ClearOldAsync(DateTime date)
    {
        var old = _context.BossSpawnStates
            .Where(x => x.Date < date.Date);

        _context.BossSpawnStates.RemoveRange(old);
        await _context.SaveChangesAsync();
    }
}