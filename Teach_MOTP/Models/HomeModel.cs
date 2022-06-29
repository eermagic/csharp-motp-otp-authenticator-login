using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teach_MOTP.Models
{
    public class HomeModel
    {
        public class GenUserQRCodeIn
        {
            public string UserID { get; set; }
            public string UserPin { get; set; }
            public string UserKey { get; set; }
        }

        public class GenUserQRCodeOut
        {
            public string ErrMsg { get; set; }
            public string FileWebPath { get; set; }
        }

        public class CheckLoginIn
        {
            public string UserID { get; set; }
            public string UserPin { get; set; }
            public string UserKey { get; set; }
            public string MOTP { get; set; }
        }

        public class CheckLoginOut
        {
            public string ErrMsg { get; set; }
            public string CheckResult { get; set; }
        }

    }
}