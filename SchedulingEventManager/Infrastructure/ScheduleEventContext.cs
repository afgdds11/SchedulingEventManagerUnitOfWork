using Microsoft.EntityFrameworkCore;
using SchedulingEventManager.Models;

namespace SchedulingEventManager.Infrastructure
{
    public class ScheduleEventContext : DbContext
    {
        public ScheduleEventContext(DbContextOptions<ScheduleEventContext> options) : base(options)
        {
        }

        public DbSet<ScheduleEvent> ScheduleEvents { get; set; }
    }
}
