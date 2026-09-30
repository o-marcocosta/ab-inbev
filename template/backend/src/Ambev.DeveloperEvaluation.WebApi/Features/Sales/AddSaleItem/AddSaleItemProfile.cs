using Ambev.DeveloperEvaluation.Application.Sales.AddSaleItem;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddSaleItem;

public class AddSaleItemProfile : Profile
{
    public AddSaleItemProfile()
    {
        CreateMap<AddSaleItemRequest, AddSaleItemCommand>()
            .ForMember(dest => dest.SaleId, opt => opt.Ignore());
    }
}
