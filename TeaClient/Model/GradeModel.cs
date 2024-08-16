using System;
using System.Collections.Generic;
using System.Text;
using TeaClient.ViewModel;

namespace TeaClient.Model
{
   public class GradeModel
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; }
    }

    public class GradeList
    {
        public List<GradeModel> GradeDetails { get; set; }
    }

    public class LocalGradeSaveModel
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
