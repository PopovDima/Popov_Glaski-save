using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Popov_Glaski_save.database
{
    public partial class Agent
    {
        public string ClearPhone
        {
            get
            {
                return new string(Phone.Where(char.IsDigit).ToArray());
            }
        }

        public string LogoPath
        {
            get
            {
                if (string.IsNullOrEmpty(Logo))
                {
                    return null;
                }

                return new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "images", Logo.TrimStart('/'))).AbsoluteUri;
            }
        }

        public int TotalSalesUnits
        {
            get
            {
                int totalSales = 0;

                DateTime startDate = DateTime.Now.AddDays(-365);

                foreach (ProductSale productSale in ProductSale)
                {
                    if (productSale.SaleDate >= startDate)
                    {
                        totalSales += productSale.ProductCount;
                    }
                }

                return totalSales;
            }
        }

        public decimal TotalSalesAmount
        {
            get
            {
                return ProductSale.Sum(ps => ps.TotalCost);
            }
        }

        public int Discount
        {
            get
            {
                if (TotalSalesAmount < 10000)
                    return 0;
                else if (TotalSalesAmount < 50000)
                    return 5;
                else if (TotalSalesAmount < 150000)
                    return 10;
                else if (TotalSalesAmount < 500000)
                    return 20;
                else
                    return 25;
            }
        }

        public Brush BackgroundDiscount
        {
            get
            {
                if (Discount >= 25)
                {
                    return Brushes.LightGreen;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
