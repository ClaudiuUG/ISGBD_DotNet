using System;
using System.Collections.Generic;
using System.Web.Http;
using KeyValueDatabaseApi.Exceptions;
using KeyValueDatabaseApi.Http.Requests;
using KeyValueDatabaseApi.Parsers;
using KeyValueDatabaseApi.Validators;

namespace KeyValueDatabaseApi.Controllers
{
    public class KeyValueDatabaseHttpController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new [] { "value1", "value2" };
        }


        // POST api/<controller>
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
                    parsedCommand.Execute();
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