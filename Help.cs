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
      static  ToolStripStatusLabel ConsolMesg = new ToolStripStatusLabel();

      static  string StringHalp;

        static public void Set_Halp() {
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


  //      static public  void WriteLineInstal(ToolStripStatusLabel Consol)
  //      {
  //          ConsolMesg = Consol;


  //      }

  //        static public void WriteLine(string Consol){
        
  //          ConsolMesg.Text = Consol;
  //}



   }
}
