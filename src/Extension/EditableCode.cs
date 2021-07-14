using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public class EditableCode
    {
        public string Language { get; }
        public string Code { get; }

        public EditableCode(string language, string code)
        {
            Language = language;
            Code = code;
        }

    }
}
