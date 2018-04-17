using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeNCrawLer_0._1._0.Advanced_Settings
{
    static class FormSubmissionSettings
    {

        public static bool CanSubmitForm;
        public static string UnMatchedField;
        public static Dictionary<string, string> FieldnameAndFieldvalue = new Dictionary<string, string>(); // matched type: always regex <fieldName,fieldValue> -> <email,info@test.net>

    }
}
