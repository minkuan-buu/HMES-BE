﻿using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.ProductRepositories
{
    public class ProductRepositories : GenericRepositories<Product>, IProductRepositories
    {
        public ProductRepositories(HmesContext context) : base(context)
        {
        }
    }
}
