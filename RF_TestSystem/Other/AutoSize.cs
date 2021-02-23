using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public class AutoSizea
    {
        public static void SetTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ";" + con.Height + ";"
                    + con.Left + ";" + con.Top + ";" + con.Font.Size;
                if (con.Controls.Count > 0)
                {
                    SetTag(con);
                }
            }
        }

        public static void SetControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
            {
                //获取控件的 Tag 属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    string[] myTag = con.Tag.ToString().Split(new char[] { ';' });
                    //根据窗体缩放的比例来确定控件的值
                    con.Width = Convert.ToInt32(System.Convert.ToSingle(myTag[0]) * newx);//宽
                    con.Height = Convert.ToInt32(System.Convert.ToSingle(myTag[1]) * newy);//搞
                    con.Left = Convert.ToInt32(System.Convert.ToSingle(myTag[2]) * newx);//左边距
                    con.Top = Convert.ToInt32(System.Convert.ToSingle(myTag[3]) * newy);//顶边距
                    Single currentSize = System.Convert.ToSingle(myTag[4]) * newy;//字体大小
                    con.Font = new System.Drawing.Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    if (con.Controls.Count > 0)
                    {
                        SetControls(newx, newy, con);
                    }
                }
            }
        }
    }
}

