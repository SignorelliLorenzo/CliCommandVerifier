using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliCommandCreator.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using CliCommandCreator.Model;
using System.IO;
using System.Text.RegularExpressions;

namespace CliCommandCreator.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string FormattedCmd { get; set; }
        [BindProperty]
        public string BaseCmd { get; set; }
        private readonly CommandContext _context;
        private List<Command> WorkingList;
        public IndexModel(CommandContext context)
        {
            _context = context;
            WorkingList = _context.Commands.ToList();
            FormattedCmd = "";
        }

        public void OnGet()
        {

            

        }
        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(BaseCmd) || BaseCmd.Contains("*"))
            {
                return Page();
            }
            var ShortCommands = BaseCmd.Trim().ToLower().Split(new[] { '\n' });
            foreach (var ShortCommand in ShortCommands)
            {
                var result = FullCommandFinder(ShortCommand) + "\n";
                if(result.StartsWith("interface") && result.Contains("/") && (result.Split(" ")[1].StartsWith("fa") || result.Split(" ")[1].StartsWith("gi") || result.Split(" ")[1].StartsWith("se")))
                {
                    var z=result.Split(" ")[1].Where(x => int.TryParse(x.ToString(), out int value)).First();
                    var last=result.Substring(result.IndexOf(z));
                    if(result.Split(" ")[1].StartsWith("fa"))
                    {
                        result = "interface " + "fastethernet" + last;
                    }
                    else if(result.Split(" ")[1].StartsWith("gi"))
                    {
                        result = "interface " + "gigaethernet" + last;
                    }
                    else if (result.Split(" ")[1].StartsWith("se"))
                    {
                        result = "interface " + "serial" + last;
                    }
                }
                FormattedCmd = FormattedCmd + result;

            }
            return Page();
        }
        /// <summary>
        /// Restituisce la versione completa del comando
        /// </summary>
        /// <param name="ShortCommand"></param>
        /// <returns>Comando abbreviato</returns>
        private string FullCommandFinder(string ShortCommand)
        {
            var commands = ShortCommand.ToLower().Trim().Split(' ');
            string FullFormattedCommand = default;
            int PreviewCommandId = -1;
            bool CommandError = false;
            int Parameters = 0;
            string ParametersType = default;
            bool done = false;

            foreach (var command in commands)
            {
                if (!done)
                {
                    if (Parameters == 0)
                    {
                        var fullcommands = WorkingList.Where(x => x.FullCommand.StartsWith(command));
                        if (PreviewCommandId != -1)
                        {
                            fullcommands = fullcommands.Where(x =>x.BaseCommand!=null && x.BaseCommand.Id == PreviewCommandId);
                        }
                        else
                        {
                            fullcommands = fullcommands.Where(x => x.BaseCommand == null);
                        }

                        Command FoundCommand = null;
                        var OutCommand=FormattedCommand(command, CommandError, fullcommands.ToList(), commands.Count(), out FoundCommand);

                        if (OutCommand.Contains('*'))
                        {
                            if(PreviewCommandId==-1)
                            {
                                CommandError = true;
                            }
                            else if (!string.IsNullOrEmpty(WorkingList.Find(x => x.Id == PreviewCommandId).Params))
                            {
                                var tempcommandparam = WorkingList.Find(x => x.Id == PreviewCommandId).Params;
                                Parameters = tempcommandparam.Split('|').Count();
                                ParametersType = tempcommandparam;
                                OutCommand=Parameter(command, ParametersType, Parameters);
                                if(OutCommand.Contains('*'))
                                {
                                    CommandError = true;
                                }
                                Parameters--;
                            }

                            
                        }
                        else if (!CommandError && string.IsNullOrEmpty(ParametersType))
                        {
                            PreviewCommandId = FoundCommand.Id;
                            if (WorkingList.Where(x => x.BaseCommand != null && x.BaseCommand.Id == PreviewCommandId).Count() == 0 && !string.IsNullOrEmpty(WorkingList.Find(x=>x.Id==PreviewCommandId).Params))
                            {
                                Parameters = FoundCommand.Params.Split('|').Count();
                                ParametersType = FoundCommand.Params;
                            }
                            else if (WorkingList.Where(x =>x.BaseCommand!=null && x.BaseCommand.Id == PreviewCommandId).Count() == 0)
                            {
                                done = true;
                            }
                        }
                        else
                        {
                            PreviewCommandId = fullcommands.First().Id;
                        }
                        FullFormattedCommand = FullFormattedCommand + OutCommand;

                    }
                    else
                    {
                        FullFormattedCommand= FullFormattedCommand+Parameter(command, ParametersType, Parameters);

                        Parameters--;
                        if (Parameters == 0)
                        {
                            done = true;
                        }
                    }
                }
                else
                {

                    FullFormattedCommand = FullFormattedCommand + "*" + command + "*" + " ";
                }

            }

            while (Parameters != 0)
            {
                var type = ParametersType.Split('|')[ParametersType.Split('|').Count() - Parameters];
                if (type.StartsWith('?'))
                {
                    break;
                }
                else
                {
                    FullFormattedCommand = FullFormattedCommand + "*PARAM*" + " ";
                }
                Parameters--;
            }

            return FullFormattedCommand;
        }
        /// <summary>
        /// Restituice il parametro controllato
        /// </summary>
        /// <param name="BaseParameter">Parametro</param>
        /// <param name="ParametersType">parametri del comando</param>
        /// <param name="Parameters">numero parametri</param>
        /// <returns></returns>
        private string Parameter(string BaseParameter,string ParametersType, int Parameters)
        {
            var type = ParametersType.Split('|')[ParametersType.Split('|').Count() - Parameters].Replace("?", "");
            switch (type)
            {
                case "IP":
                    {
                        Regex ip = new Regex(@"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$");
                        if (ip.Match(BaseParameter.Trim()).Success)
                        {
                            return BaseParameter + " ";
                            
                        }
                        return "*" + BaseParameter + "*" + " ";
                    }
                case "MASK":
                    {
                        Regex ip = new Regex(@"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$");
                        if (ip.Match(BaseParameter.Trim()).Success)
                        {
                            return BaseParameter + " ";
                        }
                        return "*" + BaseParameter + "*" + " ";
                    }
                case "INT":
                    {
                        if (int.TryParse(BaseParameter, out int x))
                        {
                            return BaseParameter + " ";
                        }
                        return "*" + BaseParameter + "*" + " ";
                    }
                case "PARAM":
                    {
                        return BaseParameter + " ";
                    }
                default:
                    {
                        if (type.StartsWith("COMBO"))
                        {
                            if (type.Split('.').Where(x => x.StartsWith(BaseParameter)).Count() == 1 && BaseParameter != "COMBO")
                            {
                                return BaseParameter + " ";
                            }
                            return "*" + BaseParameter + "*" + " ";
                        }
                        return BaseParameter + " ";
                    }
            }
        }
        /// <summary>
        /// Restituisce il comando formattato
        /// </summary>
        /// <param name="BaseCommand">Comando</param>
        /// <param name="CommandStructureError">Se il comando è già giudicato errato</param>
        /// <param name="PossibleCommands">Comandi possibili</param>
        /// <param name="TotalWords">numero di comandi nella nel comando</param>
        /// <param name="FoundCommand">Comando trovato</param>
        /// <returns></returns>
        private string FormattedCommand(string BaseCommand, bool CommandStructureError, List<Command> PossibleCommands,int TotalWords,out Command FoundCommand)
        {
            FoundCommand = null;

            if (CommandStructureError)
            {
                if (PossibleCommands.Count() > 1)
                {
                    return "*?" + BaseCommand + "?*" + " ";
                }
                else if (PossibleCommands.Count() == 0)
                {
                    return "*" + BaseCommand + "*" + " ";
                }
                FoundCommand = PossibleCommands.First();
                return PossibleCommands.First().FullCommand + " ";

            }
            if (PossibleCommands.Count() == 1)
            {
                FoundCommand = PossibleCommands.First();
                return PossibleCommands.First().FullCommand + " ";
            }
            else if(PossibleCommands.Count() > 1)
            {
                Command best = null;
                foreach(var command in PossibleCommands)
                {
                    if(Child(command, 1).Contains(TotalWords))
                    {
                        if(best !=null)
                        {
                            return "*" + BaseCommand + "*" + " ";
                        }
                        best = command;
                    }
                }
                if(best !=null)
                {
                    FoundCommand = best;
                    return best.FullCommand + " ";
                }
                
            }
            return "*" + BaseCommand + "*" + " ";

        }
        /// <summary>
        /// Restituisce la lista dei numeri di valori per ogni figlio
        /// </summary>
        /// <param name="command">comando</param>
        /// <param name="words">numero comando</param>
        /// <returns></returns>
        private List<int> Child (Command command,int words)
        {
            var childlist = WorkingList.Where(x => x.BaseCommand!=null && x.BaseCommand.Id == command.Id);
            var result = new List<int>();
            int paramword = 0;
            if(childlist.Count()==0)
            {
                if (!string.IsNullOrEmpty(command.Params))
                {
                    result.Add(words + command.Params.Split('|').Where(x => !x.StartsWith('?')).Count());
                    foreach (var param in command.Params.Split('|'))
                    {
                        if (param.StartsWith('?'))
                        {
                            result.Add(words + paramword);
                            paramword++;
                        }

                    }
                }
                else
                {
                    result.Add(words);
                }
                return result;
            }
            foreach(var child in childlist)
            {
                result.AddRange(Child(child, words+1));
               
            }
            return result;
        }
    }

}

