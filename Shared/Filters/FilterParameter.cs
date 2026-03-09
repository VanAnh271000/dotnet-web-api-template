namespace Shared.QueryParameter
{
    public class FilterParameter
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = "=="; 
        public object? Value { get; set; }
        public string LogicalOperator { get; set; } = "and";
    }

    public enum FilterOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        In,
        NotIn
    }
}
