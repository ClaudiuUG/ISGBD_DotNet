using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public enum SetFunction
    {
        Union,
        Intersect
    }

    public class SelectUnionOrIntersectCommand : ICommand
    {
        public SelectUnionOrIntersectCommand(string firstTable, string secondTable, SetFunction setFunction)
        {
            FirstTable = firstTable;
            SecondTable = secondTable;
            SetFunction = setFunction;
        }

        public string FirstTable { get; set; }
        public string SecondTable { get; set; }
        public SetFunction SetFunction { get; set; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (SetFunction == SetFunction.Union)
            {
                return dbContext.SelectUnion(FirstTable, SecondTable);
            }
            return dbContext.SelectUnion(FirstTable, SecondTable);
        }
    }
}