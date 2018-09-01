using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningCards
{
    internal class Model
    {
        public string Content { get; set; }
        public string Location { get; set; }
        public Uri URL { get; set; }
        public bool LTR { get; set; }
        public string ImageLocation { get; set; }
    }
}
