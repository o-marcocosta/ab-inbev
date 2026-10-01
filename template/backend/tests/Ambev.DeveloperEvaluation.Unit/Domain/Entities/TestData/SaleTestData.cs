using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static CustomerRef GenerateCustomer() => new(Guid.NewGuid(), Faker.Person.FullName);

    public static BranchRef GenerateBranch() => new(Guid.NewGuid(), Faker.Company.CompanyName());

    public static ProductRef GenerateProduct() => new(Guid.NewGuid(), Faker.Commerce.ProductName());

    public static decimal GenerateUnitPrice() => Math.Round(Faker.Random.Decimal(1m, 500m), 2);

    public static Sale GenerateValidSale() =>
        Sale.Create(Faker.Date.RecentOffset().UtcDateTime, GenerateCustomer(), GenerateBranch());
}
