using Cyb_lab.Data;
using Cyb_lab.Models;
using Microsoft.EntityFrameworkCore;

namespace Cyb_lab.Services;

public class EventLogsService
{
	private readonly AppDBContext _dbContext;
	private readonly DbSet<EventEntry> _logs;

	public EventLogsService(AppDBContext dbContext)
	{
		_dbContext = dbContext;
		_logs = _dbContext.Set<EventEntry>();
	}

	public IQueryable<EventEntry> GetLogs()
	{
		return _logs.AsQueryable();
	}

	public void AddEntry(EventEntry entry)
	{
		_logs.Add(entry);
		_dbContext.SaveChanges();
	}
}
