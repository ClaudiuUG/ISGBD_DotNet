using DataTanker;
using DataTanker.Settings;
using KeyValueDatabaseApi.Commands;
using KeyValueDatabaseApi.Exceptions;
using KeyValueDatabaseApi.Http.Requests;
using KeyValueDatabaseApi.Parsers;
using KeyValueDatabaseApi.Validators;
using System;
using System.Web.Http;

namespace KeyValueDatabaseApi.Controllers
{
    public class KeyValueDatabaseHttpController : ApiController
    {

        [Route("api")]
        public string Post([FromBody]ExecuteCommandRequest commandRequest)
        {

            var command = commandRequest.Command.ToLower();

            try
            {
                ICommandValidator commandValidator = new CommandValidator();
                commandValidator.ValidateCommand(command);
            }
            catch (InvalidCommandException e)
            {
                return $"{e.GetType()} occured. Message: {e.Message}";
            }

            try
            {
                ICommandParser commandParser = new CommandParser();
                if (commandParser.TryParse(command, out var parsedCommand))
                {
                    if (parsedCommand.GetType().Equals(typeof(SelectCommand)))
                    {
                        SelectCommand selectCommand = (SelectCommand)parsedCommand;
                        return selectCommand.ExecuteSelect();
                    }
                    else
                    {
                        parsedCommand.Execute();
                    }
                }
                else
                {
                    return $"Could not parse command: {command}";
                }
                return "SUCCESS";
            }
            catch (Exception exception)
            {
                return $"Failure with exception {exception}";
            }
        }
    }
}