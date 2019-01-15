using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMM0
{
    public static class Hlp
    {
        public static void Indexes()
        {
            //if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "??_Fld").FirstOrDefault() != null)
            //    Db.SQL("DROP INDEX ??_Fld ON ??");

            // CC:Client

            // PP:Project
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "PP_CC").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX PP_CC ON PP (CC)");

            // HH:HesapPlani
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "HH_PP").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX HH_PP ON HH (PP)");
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "HH_Prn").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX HH_Prn ON HH (Prn)");

            // FF:Fis
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_PP").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX FF_PP ON FF (PP)");
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_HH").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX FF_HH ON FF (HH)");
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_Trh").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX FF_Trh ON FF (Trh)");

        }
    }
}
