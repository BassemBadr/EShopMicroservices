using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Protos;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

public class DiscountService
    (DiscountContext dbcontext, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbcontext.Coupons.FirstOrDefaultAsync(c => c.ProductName == request.ProductName);
        if (coupon == null)
            coupon = new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };

        logger.LogInformation("Discount is retrieved for ProductName:{productName}, Amount :{amount}", coupon.ProductName, coupon.Amount);

        var couponModel = coupon.Adapt<CouponModel>();
        return couponModel;
    }

    public override Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        return base.CreateDiscount(request, context);
    }

    public override Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        return base.DeleteDiscount(request, context);
    }

    public override Task<CouponModel> UpdateDiscount(UpdateDiscountrequest request, ServerCallContext context)
    {
        return base.UpdateDiscount(request, context);
    }
}
