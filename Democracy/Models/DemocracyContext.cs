using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class DemocracyContext : DbContext
    {
        public DemocracyContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<State> States { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Voting> Votings { get; set; }

        public DbSet<User> Users { get; set; }
    }


}