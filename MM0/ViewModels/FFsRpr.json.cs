using DBMM0;
using Starcounter;

namespace MM0.ViewModels
{
    partial class FFsRpr : Json
    {
        protected override void OnData()
        {
            base.OnData();

            var recs = Db.SQL<FF>("select r from FF r");
            FFs.Data = recs;

            decimal GlrTop = 0, GdrTop = 0;
            foreach (var ff in recs)
            {
                GlrTop += ff.Glr;
                GdrTop += ff.Gdr;
            }
            GlrTopX = $"{GlrTop:#,#.##;-#,#.##;#}";
            GdrTopX = $"{GdrTop:#,#.##;-#,#.##;#}";
        }

    }
}
