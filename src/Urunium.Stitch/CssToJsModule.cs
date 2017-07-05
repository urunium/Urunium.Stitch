using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urunium.Stitch
{
    public class CssToJsModule
    {
        public string Build(string content, string moduleId)
        {
            content = Newtonsoft.Json.JsonConvert.SerializeObject($"{content}");
            return $@"var styleNode = document.querySelector('style[data-module=""{moduleId}""]'); if(!styleNode){{ styleNode = document.createElement('STYLE'); styleNode.type = 'text/css'; styleNode.dataset.module = '{moduleId}'; document.head.appendChild(styleNode); }}  styleNode.innerHtml = '';  var styleText = document.createTextNode({content}); styleNode.appendChild(styleText);";
        }
    }
}
