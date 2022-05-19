using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliCommandCreator.Context;
using CliCommandCreator.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CliCommandCreator.Data
{
    public class SeedData
    {
        private static void PopulateDatabase(string file, CommandContext _context)
        {
            StreamReader mod = new StreamReader(@"Database\mod");
            var h = new FileInfo(file).LastWriteTime.ToString();
            if (mod.ReadLine().Trim().ToLower() == h)
            {
                mod.Close();
               
                return;

            }
            mod.Close();
            var Nuovo = File.Create(@"Database\mod");
            var Update = new StreamWriter(Nuovo);
            Update.WriteLine(h);
            Update.Close();
            StreamReader miofile = new StreamReader(file);
            _context.Commands.RemoveRange(_context.Commands);
            _context.SaveChanges();
            string[] line = null;
            Command Base = null;
            var ModelList = _context.Commands.ToList();
            var ToAddCommands = new List<Command>();
            int id = 1; 
            if (ModelList.Count()!=0)
            {
                id=ModelList.Max(y => y.Id) + 1;
            }
            
            while (!miofile.EndOfStream)
            {
                Base = null;
                line = miofile.ReadLine().Trim().Split(' ');
               
                    foreach (var info in line)
                    {
                        if (ModelList.Where(x => x.FullCommand == info.Trim().ToLower() && x.BaseCommand == Base).Count() == 0 && !string.IsNullOrEmpty(info))
                        {
                            if (info.StartsWith("*") && info.EndsWith("*"))
                            {
                                if (string.IsNullOrEmpty(Base.Params))
                                {
                                    ToAddCommands.Where(x => x.Id == Base.Id).FirstOrDefault().Params = info.Replace('*', ' ').Trim();
                                }
                            }
                            else
                            {

                                var model = new Command() { Id = id, FullCommand = info, BaseCommand = Base, };
                                ModelList.Add(model);
                                ToAddCommands.Add(model);
                                Base = model;
                                id++;
                            }


                        }
                        else if(!string.IsNullOrEmpty(info))
                        {
                            Base = ModelList.Where(x => x.FullCommand == info.Trim().ToLower() && x.BaseCommand == Base).First();
                        }


                    }
                


            }
         
       
            foreach(var test in ToAddCommands)
            {

                    _context.Commands.Add(test);
                    _context.SaveChanges();
              
              
            }
           //ModelList.Clear();
            
            
            miofile.Close();
            
        }
        public static void Initialize(IServiceProvider ServiceProvider)
        {
            using (CommandContext context = new CommandContext(ServiceProvider.GetRequiredService<DbContextOptions<CommandContext>>()))
            {

                PopulateDatabase("Commands.txt", context);
            }
        }
    }
}
