namespace Ambev.DeveloperEvaluation.Domain.Common.Querying;

public sealed record Filter(string Field, FilterOperator Operator, string Value);
