using System;
using System.Collections.Generic;
using System.Text;

namespace TradeFlow.Modules.Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdQuery
(
    Guid ProductId,
    Guid OrganisationId

);
