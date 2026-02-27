using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popov_Glaski_save.database
{
    public partial class ProductSale
    {
        public decimal TotalCost
        {
            get
            {
                return Product.MinCostForAgent * ProductCount;
            }
        }
    }
}
