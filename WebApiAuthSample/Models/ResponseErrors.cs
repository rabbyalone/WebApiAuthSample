using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebApiAuthSample.Models
{
    public class ResponseErrors
    {
        public string Message { get; set; }
        public Dictionary<string,string[]> ModelState { get; set; }

        public override string ToString()
        {
            var str = new StringBuilder(Message);
            str.Append("<ul>");
            foreach (var s in ModelState.SelectMany(err => err.Value))
            {
                str.AppendFormat("<li>{0}</li>", s);
            }
            str.Append("</ul>");

            return str.ToString();
        }
    }
}