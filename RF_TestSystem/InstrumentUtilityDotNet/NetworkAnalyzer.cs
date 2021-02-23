using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstrumentUtilityDotNet.NetworkAnalyzerManager
{
    public  class NetworkAnalyzer
    {

        internal NetworkAnalyzer()
        {

        }
        /// <summary>
        ///获取对象实例
        /// </summary>
        /// <param name="networkAnalyzerType">仪表型号</param>
        /// <returns></returns>
        public static INetworkAnalyzer GetInstance(NetworkAnalyzerType  networkAnalyzerType)
        {
            switch (networkAnalyzerType)
            {
                case NetworkAnalyzerType.Agilent_E5071C:
                    return new Agilent_Technologies_E5071C();
                default:
                    return null;
               
            }
        }
    }
}
