using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliCommandCreator.Model;
using Microsoft.EntityFrameworkCore;

namespace CliCommandCreator.Context
{
    public class CommandContext:DbContext
    {
        public CommandContext(DbContextOptions<CommandContext> options):base(options)
        {

        }
        public DbSet<Command> Commands { get; set; }
    }
}
