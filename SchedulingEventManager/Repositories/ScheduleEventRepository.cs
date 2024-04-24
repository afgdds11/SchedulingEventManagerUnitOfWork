using Microsoft.EntityFrameworkCore;
using SchedulingEventManager.Infrastructure;
using SchedulingEventManager.Models;
using System.Linq.Expressions;

namespace SchedulingEventManager.Contract
{
    public class ScheduleEventRepository : IScheduleEventRepository
    {
        private readonly ScheduleEventContext _context;

        public ScheduleEventRepository(ScheduleEventContext context)
        {
            _context = context;
        }

        public async Task Create(ScheduleEvent entity)
        {
            _context.ScheduleEvents.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Delete(int id)
        {
            var scheduleEvent = await _context.ScheduleEvents.FindAsync(id);
            if (scheduleEvent == null)
                return false;

            _context.ScheduleEvents.Remove(scheduleEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ScheduleEvent>> FindAll()
        {
            return await _context.ScheduleEvents.ToListAsync();
        }

        public async Task<IEnumerable<ScheduleEvent>> FindByCondition(Expression<Func<ScheduleEvent, bool>> expression)
        {
            return await _context.ScheduleEvents.Where(expression).ToListAsync();
        }

        public async Task Update(ScheduleEvent entity)
        {
            _context.ScheduleEvents.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
