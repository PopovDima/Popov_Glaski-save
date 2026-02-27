using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popov_Glaski_save.database
{
    public partial class Popov_GlaskiSaveEntities : DbContext
    {
        private static Popov_GlaskiSaveEntities _context;

        public static Popov_GlaskiSaveEntities GetContext()
        {
            if (_context == null)
            {
                _context = new Popov_GlaskiSaveEntities();
            }

            return _context;
        }
    }
}
