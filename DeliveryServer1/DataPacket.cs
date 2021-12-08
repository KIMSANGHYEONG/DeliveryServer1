using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServer1
{
    [Serializable]
    struct DataPacket
    {
        public string Id;
        public string Phone;
        public string Address;
        public string Menu;
        public string Memo;
        
    }
}
