using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalyzerHelper
{
    public class AnalyzerManager
    {
        public string[] getResourceName()
        {
            Utility utility = new Utility();
            return utility.FindResources();
        }

        /// <summary>
        ///获取对象实例
        /// </summary>
        /// <param name="networkAnalyzerType">仪表型号</param>
        /// <returns></returns>
        public bool GetInstance(string networkAnalyzerName,ref INetworkAnalyzer networkAnalyzer)
        {

            NetworkAnalyzerType networkAnalyzerType = new NetworkAnalyzerType();
            Utility utility = new Utility();
            string errorMsg = string.Empty;
            if(utility.OpenResource(networkAnalyzerName, ref errorMsg))
            {
                string AnalyzerType = utility.WriteAndReadString("*IDN?");

                if(AnalyzerType.Contains("Keysight Technologies,E5071C"))
                {
                    networkAnalyzerType = NetworkAnalyzerType.Agilent_E5071C;
                }
                utility.CloseResource();
            }
            else
            {
                Console.WriteLine(errorMsg);
            }
            
            switch (networkAnalyzerType)
            {
                case NetworkAnalyzerType.Agilent_E5071C:
                    networkAnalyzer = new Agilent_E5071C();
                    return true;
                default:
                    return false;

            }
        }


    }
}
