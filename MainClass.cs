using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Learn
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Serv serv = new Serv();
            //开启服务器
            serv.Start("127.0.0.1", 1234);

            while(true)
            {
                string str = Console.ReadLine();
                switch(str)
                {
                    //输入quit退出
                    case "quit":
                        return;
                }
            }
        }
    }
}
