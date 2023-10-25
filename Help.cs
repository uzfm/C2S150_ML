using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace C2S150_ML
{
    class Help
    {


      static  string StringHalp;

        static public void SendHalp() {


           if (StringHalp!=null) 
           { MessageBox.Show(StringHalp); }

            StringHalp = "";

        }



        static public void WriteHalp( string data){
            StringHalp = StringHalp+ "\r" + data;
        }



        static public void Mesag (string data) {

            MessageBox.Show(data);
        }




    }
}
