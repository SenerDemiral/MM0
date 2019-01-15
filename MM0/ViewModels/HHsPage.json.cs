using DBMM0;
using Starcounter;

namespace MM0.ViewModels
{
    partial class HHsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            HHs.Data = DBMM0.HH.View("1234");  //Db.SQL<HH>("select r from HH r");
        }
    }
}
