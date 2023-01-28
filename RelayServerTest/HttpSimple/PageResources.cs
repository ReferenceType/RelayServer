using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer
{
    internal class PageResources
    {
        public const string StatisticsPage =
@"<!DOCTYPE html>
<html>
<head>
   <body>
    <body style=""background-color:black;""><font color=""white""></font>
         <pre style=""font-size: large; color: white;"" id = ""Display""></pre>
   </body>
<script>

let hostname = window.location.hostname;
let url = ""http://""+hostname+"":20012/generalstats"";
function loadJson() 
{
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() 
  {
    if (this.readyState == 4 && this.status == 200) 
    {
        let a = xhttp.responseText;
        //let b = a.replace(/(?:\r\n|\r|\n)/g, '<br>');
        var tags = JSON.parse(this.responseText);
        console.log(tags.ResourceUsage);
        document.getElementById(""Display"").innerHTML = JSON.stringify(tags,undefined,2)
    }
  };
  xhttp.open(""GET"", url, true);
  xhttp.send();
}
loadJson();
var intervalId = window.setInterval(function(){
  loadJson()
}, 1000);

</script>
</html>"
;
        public const string TextVisualizePage = @"<!DOCTYPE html>
<html>
<head>
    <body>
        <body style=""background-color:black;""><font color=""white""></font>
        <pre style=""font-size: large; color: white;"" id = ""Display""></pre>
    </body>
<script>
let hostname = window.location.hostname;
let url = ""http://""+hostname+"":20012/text"";
function loadJson() 
{
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() 
  {
    if (this.readyState == 4 && this.status == 200) 
    {
        let a = xhttp.responseText;
        document.getElementById(""Display"").innerHTML = a;//JSON.stringify(tags,undefined,2)
    }
  };
  xhttp.open(""GET"", url, true);
  xhttp.send();
}
loadJson();
var intervalId = window.setInterval(function(){
  loadJson()
}, 1000);
</script>
</html>";
    }
}
