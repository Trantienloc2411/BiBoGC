using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using InventoryManagement.Domain.Entities;
using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductVariantEvents;

public class ProductVariantCreatedEvent : DomainEvent
{
    public Guid ProductId {get;}
    public Guid ProductVariantId {get;}

    public string VariantName {get;}
    
    public ProductVariantCreatedEvent 
    (
        Guid productId,
        Guid productVariantId,
        string variantName
    )
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
        VariantName = variantName;

    }
    
}
