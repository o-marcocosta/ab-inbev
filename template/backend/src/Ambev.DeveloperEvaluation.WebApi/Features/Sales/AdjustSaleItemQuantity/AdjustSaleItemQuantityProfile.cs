using Ambev.DeveloperEvaluation.Application.Sales.AdjustSaleItemQuantity;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.AdjustSaleItemQuantity;

public class AdjustSaleItemQuantityProfile : Profile
{
    public AdjustSaleItemQuantityProfile()
    {
        CreateMap<AdjustSaleItemQuantityRequest, AdjustSaleItemQuantityCommand>()
            .ForMember(dest => dest.SaleId, opt => opt.Ignore())
            .ForMember(dest => dest.ItemId, opt => opt.Ignore());
    }
}
