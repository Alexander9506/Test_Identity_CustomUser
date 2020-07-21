using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test_CustomUserManagement.Models
{
    public class EFContext : DbContext
    {
        public DbSet<FileContainer> FileContainers { get; set; }
        public EFContext(DbContextOptions<EFContext> options) : base(options)
        {
        }
    }
}
