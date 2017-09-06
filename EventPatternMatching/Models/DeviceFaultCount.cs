using System;
using Microsoft.EntityFrameworkCore;

namespace EventPatternMatching.Models
{
    /// <summary>
    /// this class is used for storing All faout that ocurred per a device
    /// </summary>
    public class EventPatternMatching
    {
		public int ID { get; set; }
       public string DeviceId
        {
            get;
            set;
        }
        public int Count
        {
            get;
            set;
        }
    }

	public class EventPatternMatchingContext : DbContext
	{
		public EventPatternMatchingContext(DbContextOptions<EventPatternMatchingContext> options)
			: base(options)
		{
		}
        public EventPatternMatchingContext(){
            
        }
		public DbSet<EventPatternMatching> EventPatternMatching { get; set; }
	}
}
