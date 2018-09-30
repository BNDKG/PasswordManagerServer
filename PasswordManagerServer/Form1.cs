using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using System.IO;
//aes加密
using System.Security.Cryptography;
//序列化
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

//和android设备的序列化json

using System.Web.Script.Serialization;

namespace PasswordManagerServer
{
    public partial class Form1 : Form
    {
        static string OriPath;
        static string UsersDataPath;
        static string CurUserPath;

        //总体密码保存结构体
        //static PassWordStruct[] bbb;
        static PassWordDic bbbb;

        enum ClientType
        {
            Csharp,
            Android,
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Console.Write("Waiting for a connection... ");

            richTextBox1.Text = richTextBox1.Text.Insert(0, "Waiting for a connection... ");

            MultStart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Endflag = false;
        }
        //创建 负责监听的线程，并传入监听方法
        private void MultStart()
        {
            Thread threadWatch = new Thread(StartListerning);
            threadWatch.IsBackground = true;//设置为后台线程
            threadWatch.Start();//开启线程
        }


        bool Endflag;
        private void StartListerning()
        {
            /*
             * 通信协议(收)
             * 第一次 2位 <json>固定长度 14 byte
             * 0客户端标识 3位数字 \ 1第二次通信长度
             * 第二次 4位
             * 0逻辑代号 确认用户需要哪项功能 具体功能如下 
             * 0 测试连通性 1 同步所有密码 2 上传本地密码 3 创建用户 4 修改key或删除用户密码等\
             * 1用户名 \ 2用户的KEY \3(创建用户名时需要的管理员权限密码)
             * 第三次(见后详细)
             * 
             */

            /*
             * 通信协议(用户上传本地密码)
             * 第一次 4位 固定长度 256 byte
             * 0待定 \ 1接下来一号数组长度 \ 2二号数组长度 \ 3三号数组长度
             * 第二次
             * 一号数组(name)
             * 第三次
             * 二号数组(psw)aes加密后
             * 第四次
             * 三号数组(info)其他信息，目前为保存名称
             */
            /* 通信协议(出现重复覆盖)
             * 通信协议(发)
             * 第一次 固定长度
             * 0 是否成功以及是否有覆盖 \ 1 接下来信息的长度
             * 第二次 所有将要覆盖的名字
             * 名字数组
             

             * 通信协议(收)
             * 第一次 固定长度
             * 0 是否全部覆盖或取消或自定义 \ 1 接下来信息的长度
             * 第二次 自定义覆盖
             * 是否覆盖的数组
             */


            /*
             * 通信协议(创建用户)
             * 第一次 2位 固定长度 256 byte
             * 0待定 \ 1接下来数组长度
             * 第二次
             * 0用户名 \1 KEY
             */



            /*
             * 通信协议(修改或删除用户)
             * 第一次 2位 固定长度 256 byte
             * 0逻辑代号 具体功能如下
             * 0待定 1删除某个密码 2修改KEY\
             * 1接下来数组长度
             * 第二次 (待定)
             * 0待定 \1 待定
             * 第二次(删除某个密码 不删除bak)
             * 0密码存储名
             * 第二次(修改key)
             * 0 原来的key 1 新的key
             */

            /*----------------------------------------------------------------------------------------*/


            /*
             * 通信协议(发客户端同步下载详细)c#
             * 第一次 4位 固定长度 256 byte
             * 0是否允许同步下载 \ 1接下来一号数组长度 \ 2二号数组长度 \ 3三号数组长度
             * 第二次
             * 一号数组(name)
             * 第三次
             * 二号数组(psw)aes加密后
             * 第四次
             * 三号数组(info)其他信息，目前为保存名称
             */

            /*
             * 通信协议(发客户端同步下载详细)android
             * 第一次 4位 <json>固定长度 28 byte （这里可能不够提升到6位）
             * 0是否允许同步下载 \ 1接下来一号数组长度 4位 \ 2二号数组长度 4位\ 3三号数组长度 4位
             * 第二次
             * 一号数组(name)
             * 第三次
             * 二号数组(psw)aes加密后
             * 第四次
             * 三号数组(info)其他信息，目前为保存名称
             */

            /*
             * 通信协议(发客户端允许上传详细)c#
             * 第一次 2位 固定长度 xx byte （）
             * 0是否允许上传 \ 1剩余可用密码个数 4位 
             */

            /*
             * 通信协议(发允许新建用户)c#
             * 第一次 2位 固定长度 xx byte （）
             * 0是否允许新增用户 \ 1待定 
             */

            /*
             * 通信协议(发允许用户修改)c#
             * 第一次 2位 固定长度 xx byte （）
             * 0是否允许修改 \ 1待定 
             */

            /*
             * 通信协议(发服务器资料修改成功)c#
             * 第一次 2位 固定长度 xx byte （）
             * 0修改是否成功 \ 1待定 
             */


            /*----------------------------------------------------------------------------------------*/
            TcpListener server = null;
            try
            {
                Int32 port = Convert.ToInt32(textBox1.Text);
                string ipadd = textBox2.Text;

                IPAddress localAddr = IPAddress.Parse(ipadd);

                server = new TcpListener(localAddr, port);
                server.Start();

                //设置退出flag
                Endflag = true;

                //开始监听主循环
                while (Endflag)
                {
                    try
                    {
                        if (!server.Pending())
                        {
                            //为了避免每次都被tcpListener.AcceptTcpClient()阻塞线程，添加了此判断，
                            //当没有连接请求时，什么也不做，有了请求再执行到tcpListener.AcceptTcpClient()
                            //加入延迟降低cpu功耗
                            Thread.Sleep(100);
                        }
                        else
                        {

                            TcpClient client = server.AcceptTcpClient();
                            //用于测试TESTZDELFLAG
                            Console.WriteLine("Connected!");

                            // Get a stream object for reading and writing
                            NetworkStream stream = client.GetStream();                         

                            // 第一次接收 json 目前数据大小位14byte
                            Byte[] bytes = new Byte[14];

                            stream.Read(bytes, 0, bytes.Length);

                            // 将传来字符串转换为字符串数组,todo添加长度限制防止崩溃
                            string[] firstget = EncryptionClass.BytesToStringArr(bytes);

                            //判断客户端
                            ClientType clientType;
                            if (GetClientType(firstget) == 1)
                            {
                                clientType = ClientType.Csharp;
                            }
                            else if(GetClientType(firstget) == 2)
                            {
                                clientType = ClientType.Android;
                            }
                            else
                            {
                                clientType = ClientType.Csharp;
                            }
                            //接下来信息的长度
                            int MsgLen = Convert.ToInt32(firstget[1]);
                            //返回消息buffer
                            byte[] buffer;

                            //选择不同客户端逻辑
                            switch (clientType)
                            {
                                case ClientType.Csharp:

                                    Byte[] bytes2 = new Byte[256];

                                    stream.Read(bytes2, 0, bytes2.Length);

                                    string[] csharplogicget = (string[])EncryptionClass.BytesToObject(bytes2);


                                    
                                    int k = Convert.ToInt32(csharplogicget[0]);

                                    String CurUserName = csharplogicget[1];
                                    String CurUserPsw = csharplogicget[2];
                                    string cursuperadmin= csharplogicget[3];

                                    int checkresult=-1;
                                    checkresult = PSWDataBaseClass.UserCheck(CurUserName, CurUserPsw);
                                    switch (k)
                                    {
                                        case 0:
                                            //测试服务器连通

                                            //直接返回1表示服务器连接正常
                                            buffer = Errorreply(1);
                                            //返回2表示服务器忙
                                            stream.Write(buffer, 0, buffer.Length);
                                            break;
                                        case 1:
                                            //同步所有密码
                                          
                                            //验证用户名密码
                                            if (checkresult == 1){
                                                //处理请求并回复
                                                byte[][] backmsgs = PSWDataBaseClass.downloadlogic(csharplogicget);

                                                foreach (var singlemsg in backmsgs)
                                                {
                                                    int len = singlemsg.Length;

                                                    stream.Write(singlemsg, 0, len);

                                                    //这里考虑下要不要wait
                                                }
                                            }
                                            else if(checkresult == 2)
                                            {
                                                Errorreply(2);
                                            }
                                            else
                                            {
                                                Errorreply(3);
                                            }

                                            break;
                                        case 2:
                                            //用户上传本地密码

                                            //验证用户名密码
                                            if (checkresult == 1)
                                            {
                                                //返回同意用户上传密码

                                                //直接返回1表示同意用户上传密码
                                                buffer = Errorreply(1);
                                                stream.Write(buffer, 0, buffer.Length);

                                                //接收用户上传的密码
                                                Byte[] buffer2 = new Byte[256];
                                                stream.Read(buffer2, 0, buffer2.Length);
                                                string[] uploadfirst = (string[])EncryptionClass.BytesToObject(buffer2);

                                                int lenb = Convert.ToInt32(uploadfirst[1]);
                                                int lenc = Convert.ToInt32(uploadfirst[2]);
                                                int lend = Convert.ToInt32(uploadfirst[3]);

                                                Byte[] data2 = new Byte[lenb];
                                                Byte[] data3 = new Byte[lenc];
                                                Byte[] data4 = new Byte[lend];

                                                stream.Read(data2, 0, data2.Length);
                                                stream.Read(data3, 0, data3.Length);
                                                stream.Read(data4, 0, data4.Length);

                                                string[] receive2 = (string[])EncryptionClass.BytesToObject(data2);
                                                string[] receive3 = (string[])EncryptionClass.BytesToObject(data3);
                                                string[] receive4 = (string[])EncryptionClass.BytesToObject(data4);

                                                //返回是否成功或者失败或者有重复，如果有重复则发送重复名字
                                                List<string> RepetName;                                              
                                                //判断是否有重复
                                                if(PSWDataBaseClass.UploadUserMain(receive2, receive3, receive4, out RepetName) != 1)
                                                {
                                                    //如果有重复继续
                                                    //(如果有重复)接收返回的修改的数组，将数组对应的密码改为新密码

                                                }

                                                //返回成功

                                                buffer = Errorreply(1);
                                                stream.Write(buffer, 0, buffer.Length);



                                            }
                                            else if (checkresult == 2)
                                            {
                                                //用户名不存在
                                                buffer = Errorreply(2);
                                                stream.Write(buffer, 0, buffer.Length);
                                            }
                                            else
                                            {
                                                //用户名密码错误
                                                buffer = Errorreply(3);
                                                stream.Write(buffer, 0, buffer.Length);
                                            }

                                            break;
                                        case 3:
                                            //创建用户逻辑

                                            //创建目录
                                            int curerror=PSWDataBaseClass.CreateUserMain(cursuperadmin, CurUserName, CurUserPsw);

                                            //返回结果
                                            //直接返回1表示服务器连接正常
                                            buffer = Errorreply(curerror);
                                            //返回2表示服务器忙
                                            stream.Write(buffer, 0, buffer.Length);

                                            break;
                                        case 4:
                                            //修改或删除key
                                            //验证用户名密码
                                            if (checkresult == 1)
                                            {
                                                //返回同意用户修改key

                                                //读取将新拿来的key保存到服务器中

                                                //返回修改成功

                                            }
                                            else if (checkresult == 2)
                                            {
                                                //用户名不存在
                                                Errorreply(2);
                                            }
                                            else
                                            {
                                                //用户名密码错误
                                                Errorreply(3);
                                            }


                                            break;
                                        default:
                                            //无效的指令
                                            break;

                                    }
                                    break;
                                case ClientType.Android:

                                    Byte[] bytes3 = new Byte[MsgLen];
                                    stream.Read(bytes3, 0, bytes3.Length);

                                    string[] get2 = EncryptionClass.BytesToStringArr(bytes3);

                                    byte[][] backmsgs2 = backmsglogic2(get2);

                                    foreach (var singlemsg in backmsgs2)
                                    {
                                        int len = singlemsg.Length;

                                        stream.Write(singlemsg, 0, len);

                                        //这里考虑下要不要wait
                                    }


                                    break;
                                default:
                                    break;
                            }


                            // 关闭socket连接
                            client.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: {0}", ex);
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: {0}", ex);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\n成功关闭服务器");

        }

        private void test111()
        {
            string[] test = { "1", "BNDKG" ,"kkk"};

            string[] test2 = { "2", "BNDKG", "kkk" ,"京东商城","zbsqqyx","1qw23er45t"};

            backmsglogic(test);

        }
        private int GetClientType(string[] firstget)
        {
            int typechoice;
            string logictype = firstget[0];
            try
            {
                typechoice = Convert.ToInt32(logictype);
            }
            catch
            {
                typechoice = 0;
                //有空了再搞异常处理防止恶意破坏
            }

            return typechoice;

        }


        private byte[][] backmsglogic(string[] firstget)
        {

            byte[][] backmsgs= new byte[1][];

            backmsgs[0] = new byte[] { 0x00 };


            string logictype = firstget[0];
            int typechoice;
            try
            {
                typechoice = Convert.ToInt32(logictype);
            }
            catch
            {
                typechoice = 0;
                //有空了再搞异常处理防止恶意破坏
            }

            if (typechoice == 1)
            {
                //同步逻辑

                //读取用户名
                string nameupload = firstget[1];
                string pswload = firstget[2];

                try
                {
                    //判断用户名密码是否正确
                    if (userjudge(nameupload, pswload))   //密码账号正确
                    {
                        //从文件夹中读取对应数据
                        PSWDataBaseClass.CurUserinit(nameupload);
                        //string bakpath = CurUserPath + "bak\\";
                        string sourcepath = CurUserPath + "source\\psw";

                        PassWordDic curbbbb = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);

                        //结构转换分布发送
                        backmsgs = new byte[4][];
                        int row = backmsgs.Length;

                        //string[] databackname = { "Fuck back the superuniverse", "sdd" };
                        string[] databackname=new string[curbbbb.numberofpassword];
                        string[] databackpsw = new string[curbbbb.numberofpassword];
                        string[] databackinfo = new string[curbbbb.numberofpassword];

                        for (int iii=0;iii< curbbbb.numberofpassword; iii++)
                        {
                            databackname[iii] = curbbbb.MYpasswordList[iii].name;
                            databackpsw[iii] = curbbbb.MYpasswordList[iii].password;
                            databackinfo[iii] = curbbbb.MYpasswordList[iii].info;
                        }

                        /*
                        foreach (var singlepsw in curbbbb.MYpasswords)
                        {
                            databackname
                        }
                        */
                       
                        byte[] msgb = EncryptionClass.ObjectToBytes(databackname);
                        byte[] msgc = EncryptionClass.ObjectToBytes(databackpsw);
                        byte[] msgd = EncryptionClass.ObjectToBytes(databackinfo);

                        string blen = (msgb.Length).ToString();
                        string clen = (msgc.Length).ToString();
                        string dlen = (msgd.Length).ToString();

                        string[] userinfo = { "用户信息", blen, clen,dlen };
                        byte[] msga = EncryptionClass.ObjectToBytes(userinfo);

                        backmsgs[0] = msga;
                        backmsgs[1] = msgb;
                        backmsgs[2] = msgc;
                        backmsgs[3] = msgd;
                        /*
                        for (int ii = 0; ii < row; ii++)
                        {
                            backmsgs[ii] = new byte[];
                        }
                        */

                    }
                    else    //密码账号错误
                    {
                        //账号密码错误返回逻辑
                    }
                }
                catch
                {
                    //如果出问题了
                    int dddses = 3;
                }

            }
            else if (typechoice == 2)
            {
                //单个上传
                //读取用户名
                string nameupload = firstget[1];
                string pswload = firstget[2];

                try
                {
                    //判断用户名密码是否正确
                    if (userjudge(nameupload, pswload))   //密码账号正确
                    {
                        //上传的文件名3 用户名4 密码5
                        string uploadstructfilename = firstget[3];
                        string uploadstructusername = firstget[4];
                        string uploadstructpsw = firstget[5];

                        //从文件夹中读取对应数据
                        PSWDataBaseClass.CurUserinit(nameupload);
                        //string bakpath = CurUserPath + "bak\\";
                        string sourcepath = CurUserPath + "source\\psw";

                        PassWordDic curbbbb = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);

                        //结构转换分布发送
                        backmsgs = new byte[1][];

                        string[] userinfo = { "上传成功", "当前存储的信息等" };


                        for (int iii = 0; iii < curbbbb.numberofpassword; iii++)
                        {
                            if (curbbbb.otherinfo== uploadstructfilename)
                            {
                                userinfo[0] = "重复的密码";
                                break;
                            }
                            //databackname[iii] = curbbbb.MYpasswords[iii].name;
                            //databackpsw[iii] = curbbbb.MYpasswords[iii].password;

                        }
                        if(userinfo[0] == "上传成功")
                        {
                            
                            //更新服务器中的bak和source
                        }
                        /*
                        foreach (var singlepsw in curbbbb.MYpasswords)
                        {
                            databackname
                        }
                        */
                        

                        byte[] msga = EncryptionClass.ObjectToBytes(userinfo);


                        backmsgs[0] = msga;


                    }
                    else    //密码账号错误
                    {
                        //账号密码错误返回逻辑
                    }
                }
                catch
                {
                    //如果出问题了
                    int dddses = 3;
                }
            }

            //先测试一下
            /*
            foreach (var singlemsg in backmsgs)
            {
                string[] gggg= (string[])EncryptionClass.BytesToObject(singlemsg);

                int ddddss = 3;
                //这里考虑下要不要wait
            }
            */


            //Console.WriteLine("Received: {0}", data);

            // Process the data sent by the client.
            //data = data.ToUpper();

            //string[] databack = { "Fuck back the superuniverse", "sdd" };







            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(databack);
            //byte[] msg = EncryptionClass.ObjectToBytes(databack);
            //string databack2 = "Fuck Fuck back the superuniverse";
            //byte[] msg2 = System.Text.Encoding.ASCII.GetBytes(databack2);

            return backmsgs;
        }

        private byte[] Errorreply(int error)
        {
            string errorstring = error.ToString();
            //todo 附带一个服务器时间
            string time = "";
            string[] userinfo = { errorstring, "11:11:11" };
            byte[] msga = EncryptionClass.ObjectToBytes(userinfo);
            return msga;
        }
        private byte[][] backmsglogic2(string[] firstget)
        {

            byte[][] backmsgs = new byte[1][];

            backmsgs[0] = new byte[] { 0x00 };


            string logictype = firstget[0];
            int typechoice;
            try
            {
                typechoice = Convert.ToInt32(logictype);
            }
            catch
            {
                typechoice = 0;
                //有空了再搞异常处理防止恶意破坏
            }

            if (typechoice == 1)
            {
                //同步逻辑

                //读取用户名
                string nameupload = firstget[1];
                string pswload = firstget[2];

                try
                {
                    //判断用户名密码是否正确
                    if (userjudge(nameupload, pswload))   //密码账号正确
                    {
                        //从文件夹中读取对应数据
                        PSWDataBaseClass.CurUserinit(nameupload);
                        //string bakpath = CurUserPath + "bak\\";
                        string sourcepath = CurUserPath + "source\\psw";

                        PassWordDic curbbbb = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);

                        //结构转换分布发送
                        backmsgs = new byte[4][];
                        int row = backmsgs.Length;

                        //string[] databackname = { "Fuck back the superuniverse", "sdd" };
                        string[] databackname = new string[curbbbb.numberofpassword];
                        string[] databackpsw = new string[curbbbb.numberofpassword];
                        string[] databackinfo = new string[curbbbb.numberofpassword];

                        for (int iii = 0; iii < curbbbb.numberofpassword; iii++)
                        {
                            databackname[iii] = curbbbb.MYpasswordList[iii].name;
                            databackpsw[iii] = curbbbb.MYpasswordList[iii].password;
                            databackinfo[iii] = curbbbb.MYpasswordList[iii].info;
                        }

                        /*
                        foreach (var singlepsw in curbbbb.MYpasswords)
                        {
                            databackname
                        }
                        */
                        
                        byte[] msgb = EncryptionClass.StringArrToBytes(databackname);
                        byte[] msgc = EncryptionClass.StringArrToBytes(databackpsw);
                        byte[] msgd = EncryptionClass.StringArrToBytes(databackinfo);

                        string blen = String.Format("{0:D4}", msgb.Length);
                        string clen = String.Format("{0:D4}", msgc.Length);
                        string dlen = String.Format("{0:D4}", msgd.Length);
                        //string blen = (msgb.Length).ToString();
                        //string clen = (msgc.Length).ToString();
                        //string dlen = (msgd.Length).ToString();

                        string[] userinfo = { "002", blen, clen, dlen };
                        byte[] msga = EncryptionClass.StringArrToBytes(userinfo);

                        backmsgs[0] = msga;
                        backmsgs[1] = msgb;
                        backmsgs[2] = msgc;
                        backmsgs[3] = msgd;
                        /*
                        for (int ii = 0; ii < row; ii++)
                        {
                            backmsgs[ii] = new byte[];
                        }
                        */

                    }
                    else    //密码账号错误
                    {
                        //账号密码错误返回逻辑
                    }
                }
                catch
                {
                    //如果出问题了
                    int dddses = 3;
                }

            }
            else if (typechoice == 2)
            {
                //单个上传
                //读取用户名
                string nameupload = firstget[1];
                string pswload = firstget[2];

                try
                {
                    //判断用户名密码是否正确
                    if (userjudge(nameupload, pswload))   //密码账号正确
                    {
                        //上传的文件名3 用户名4 密码5
                        string uploadstructfilename = firstget[3];
                        string uploadstructusername = firstget[4];
                        string uploadstructpsw = firstget[5];

                        //从文件夹中读取对应数据
                        PSWDataBaseClass.CurUserinit(nameupload);
                        //string bakpath = CurUserPath + "bak\\";
                        string sourcepath = CurUserPath + "source\\psw";

                        PassWordDic curbbbb = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);

                        //结构转换分布发送
                        backmsgs = new byte[1][];

                        string[] userinfo = { "上传成功", "当前存储的信息等" };


                        for (int iii = 0; iii < curbbbb.numberofpassword; iii++)
                        {
                            if (curbbbb.otherinfo == uploadstructfilename)
                            {
                                userinfo[0] = "重复的密码";
                                break;
                            }
                            //databackname[iii] = curbbbb.MYpasswords[iii].name;
                            //databackpsw[iii] = curbbbb.MYpasswords[iii].password;

                        }
                        if (userinfo[0] == "上传成功")
                        {

                            //更新服务器中的bak和source
                        }
                        /*
                        foreach (var singlepsw in curbbbb.MYpasswords)
                        {
                            databackname
                        }
                        */


                        byte[] msga = EncryptionClass.ObjectToBytes(userinfo);


                        backmsgs[0] = msga;


                    }
                    else    //密码账号错误
                    {
                        //账号密码错误返回逻辑
                    }
                }
                catch
                {
                    //如果出问题了
                    int dddses = 3;
                }
            }

            //先测试一下
            /*
            foreach (var singlemsg in backmsgs)
            {
                string[] gggg= (string[])EncryptionClass.BytesToObject(singlemsg);

                int ddddss = 3;
                //这里考虑下要不要wait
            }
            */


            //Console.WriteLine("Received: {0}", data);

            // Process the data sent by the client.
            //data = data.ToUpper();

            //string[] databack = { "Fuck back the superuniverse", "sdd" };







            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(databack);
            //byte[] msg = EncryptionClass.ObjectToBytes(databack);
            //string databack2 = "Fuck Fuck back the superuniverse";
            //byte[] msg2 = System.Text.Encoding.ASCII.GetBytes(databack2);

            return backmsgs;
        }

        private bool userjudge(string nameupload, string pswload)
        {
            //todo这里查找所有文件夹寻找是否有此名字并从文件夹下读取对应密码，如果正确则上传对应密码


            return true;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            structsave("test1");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            structload("test1");
        }

        private void structsave(string structsavepath)
        {
            /*
            PassWordStruct[] aaa = new PassWordStruct[3];
            aaa[1].NO = 10;
            aaa[1].name = "fdsfs";
            aaa[1].password = "fsdsfesssss";
            aaa[2].NO = 14;
            aaa[2].name = "aaaaaa";
            aaa[2].password = "fsdsfesssss";
            aaa[0].NO = 17;
            aaa[0].name = "vdvvvvs";
            aaa[0].password = "fdddfesssss";
            */


            List<PassWordStruct> aac = new List<PassWordStruct>();
            PassWordStruct add1 = new PassWordStruct(1);
            aac.Add(add1);



            PassWordDic aaaa;
            aaaa.info = "";
            aaaa.password = "";
            aaaa.otherinfo = "";
            aaaa.numberofpassword = aac.Count();
            //aaaa.MYpasswords = aaa;
            aaaa.MYpasswordList = aac;

            EncryptionClass.SavePassword(aaaa, structsavepath);
        }
        private void structload(string structloadpath)
        {
            //bbb = (PassWordStruct[])EncryptionClass.LoadPassword("test1");

            bbbb = (PassWordDic)EncryptionClass.LoadPassword(structloadpath);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OriPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            UsersDataPath = OriPath + "UsersData\\";

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string path = OriPath;

            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            PSWDataBaseClass.CurUserinit("BNDKG7");
            PSWDataBaseClass.bakOnLoad();

            PSWDataBaseClass.saveandchange();
        }


        private void button7_Click(object sender, EventArgs e)
        {

        }


        private void button8_Click(object sender, EventArgs e)
        {
            PSWDataBaseClass.CreateUserMain("Superbndkg","REXNI","aaabbbcccdddeeefff");


        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            int result = PSWDataBaseClass.UserCheck("REXNI", "aaabbbcccdddeeefff");


            int fsdffe = 4;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            //从bak读取并保存到新结构体中
        }

        private void button12_Click(object sender, EventArgs e)
        {
            test111();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //string[] kkk = { "fuck the superuniverse", "fuck again", "fuck again and again" };

            //List<string> kkk = new List<string> { "fuck the superuniverse", "fuck again", "fuck again and again" };
            string[] kkk = { "aa", "abc", "ks" };

            string get =EncryptionClass.ToJSON(kkk);

            //string sss = "easonjim";

            byte [] changebyte =Encoding.UTF8.GetBytes(get);

            string nfefe=Encoding.UTF8.GetString(changebyte);

            string[] lll =(string[])EncryptionClass.FromJSON<string[]>(nfefe);

            int lemecc = 1;
        }

        private void button14_Click(object sender, EventArgs e)
        {

        }
    }
    [Serializable]
    public struct PassWordStruct 
    {
        //密码结构
        public int NO;
        public string key;
        public string name;
        public string password;
        public string info;

        public PassWordStruct(int type)
        {
            NO = type;
            key = "";
            name = "";
            password = "";
            info = "";
        }

    }
    [Serializable]
    public struct PassWordDic  
    {
        //密码字典结构
        public string info;
        public string password;
        public string otherinfo;
        public int numberofpassword;
        //public PassWordStruct[] MYpasswords;
        public List<PassWordStruct> MYpasswordList;


    }


    public class EncryptionClass
    {
        public static object LoadPassword(string filename)            //序列化读取文件(从服务器)
        {
            object password = new object();
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                password = bf.Deserialize(fs);
                fs.Close();
            }
            catch (Exception)
            {
                //MessageBox.Show("读取失败！！！");
            }

            return password;
        }
        public static object LoadPasswordPure(string filename)            //序列化读取文件(从bak)
        {
            PassWordStruct aaa = new PassWordStruct(9);
            aaa.password = _LoadPasswordPure(filename);

            return aaa;
        }
        public static string _LoadPasswordPure(string filename)            //序列化读取文件
        {
            string password = "";
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                password = sr.ReadLine();

                sr.Close();
                fs.Close();
            }
            catch (Exception)
            {
                //MessageBox.Show("备份读取失败！！！");
            }

            return password;
        }

        public static void SavePassword(object password, string filename)   //序列化保存(服务器版)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, password);
                fs.Close();
                //MessageBox.Show("服务器保存成功！");
            }
            catch (Exception)
            {
                //MessageBox.Show("服务器保存失败！");
            }
        }

        /// <summary> 
        /// 将一个object对象序列化，返回一个byte[]         
        /// </summary> 
        /// <param name="obj">能序列化的对象</param>         
        /// <returns></returns> 
        public static byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter(); formatter.Serialize(ms, obj); return ms.GetBuffer();
            }
        }

        /// <summary> 
        /// 将一个序列化后的byte[]数组还原         
        /// </summary>
        /// <param name="Bytes"></param>         
        /// <returns></returns> 
        public static object BytesToObject(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                IFormatter formatter = new BinaryFormatter(); return formatter.Deserialize(ms);
            }
        }
        public static string[] BytesToStringArr(byte[] Bytes)
        {
            string nfefe = Encoding.UTF8.GetString(Bytes);
            return FromJSON<string[]>(nfefe);
        }
        public static byte[] StringArrToBytes(string[] StringArr)
        {
            string get = EncryptionClass.ToJSON(StringArr);
            return Encoding.UTF8.GetBytes(get);
        }

        /// <summary>
        /// 内存对象转换为json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJSON(object obj)
        {
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer json = new JavaScriptSerializer();
            json.Serialize(obj, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Json字符串转内存对象
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FromJSON<T>(string jsonString)
        {
            JavaScriptSerializer json = new JavaScriptSerializer();
            return json.Deserialize<T>(jsonString);
        }


        public static string sha256(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            byte[] hash = SHA256Managed.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }
            return builder.ToString();
        }

    }


    public class PSWDataBaseClass
    {

        static String CurUserPath;
        static String OriPath;
        static String UsersDataPath;
        static String pswpath;
        static String bakpath;

        public static void CurUserinit(string username)
        {
            OriPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            UsersDataPath = OriPath + "UsersData\\";

            //这里改多用户名逻辑
            //string username = "BNDKG";

            CurUserPath = UsersDataPath + username + "\\";

            pswpath = CurUserPath + "source\\";
            bakpath = CurUserPath + "bak\\";
        }

        public static byte[][] downloadlogic(string[] firstget)
        {
            byte[][] backmsgs = new byte[1][];
            backmsgs[0] = new byte[] { 0x00 };

            //同步逻辑

            //读取用户名
            string nameupload = firstget[1];
            string pswload = firstget[2];

            try
            {
                //判断用户名密码是否正确
                if (true)   //密码账号正确
                {
                    //从文件夹中读取对应数据
                    CurUserinit(nameupload);
                    //string bakpath = CurUserPath + "bak\\";
                    string sourcepath = CurUserPath + "source\\psw";

                    PassWordDic curbbbb = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);

                    //结构转换分布发送
                    backmsgs = new byte[4][];
                    int row = backmsgs.Length;
                    int numofpsw=curbbbb.MYpasswordList.Count();

                    //string[] databackname = { "Fuck back the superuniverse", "sdd" };
                    string[] databackname = new string[numofpsw];
                    string[] databackpsw = new string[numofpsw];
                    string[] databackinfo = new string[numofpsw];

                    for (int iii = 0; iii < numofpsw; iii++)
                    {
                        databackname[iii] = curbbbb.MYpasswordList[iii].name;
                        databackpsw[iii] = curbbbb.MYpasswordList[iii].password;
                        databackinfo[iii] = curbbbb.MYpasswordList[iii].info;
                    }

                    /*
                    foreach (var singlepsw in curbbbb.MYpasswords)
                    {
                        databackname
                    }
                    */

                    byte[] msgb = EncryptionClass.ObjectToBytes(databackname);
                    byte[] msgc = EncryptionClass.ObjectToBytes(databackpsw);
                    byte[] msgd = EncryptionClass.ObjectToBytes(databackinfo);

                    string blen = (msgb.Length).ToString();
                    string clen = (msgc.Length).ToString();
                    string dlen = (msgd.Length).ToString();

                    string[] userinfo = { "1", blen, clen, dlen };
                    byte[] msga = EncryptionClass.ObjectToBytes(userinfo);

                    backmsgs[0] = msga;
                    backmsgs[1] = msgb;
                    backmsgs[2] = msgc;
                    backmsgs[3] = msgd;
                    /*
                    for (int ii = 0; ii < row; ii++)
                    {
                        backmsgs[ii] = new byte[];
                    }
                    */

                }
                else    //密码账号错误
                {
                    //账号密码错误返回逻辑
                }
            }
            catch
            {
                //如果出问题了
                int dddses = 3;
            }
            return backmsgs;
        }


        static List<string> bakPWDPathList = new List<string>();
        static List<string> bakPWDNameList = new List<string>();

        public static void bakOnLoad()
        {

            //获取指定文件夹的所有文件
            string[] paths = Directory.GetFiles(bakpath);
            foreach (var item in paths)
            {
                //获取文件后缀名
                string extension = Path.GetExtension(item).ToLower();
                if (extension == ".bak")
                {

                    string savename = Path.GetFileNameWithoutExtension(item).ToLower();
                    bakPWDPathList.Add(item);
                    bakPWDNameList.Add(savename);
                }
            }

        }

        public static void saveandchange()
        {
            

            List<PassWordStruct> pswlist = new List<PassWordStruct>();
            int i = 0;
            foreach (var item in bakPWDPathList)
            {

                PassWordStruct curpswstuct = (PassWordStruct)EncryptionClass.LoadPasswordPure(item);
                curpswstuct.info = bakPWDNameList[i];
                pswlist.Add(curpswstuct);

                i++;

            }

            PassWordDic aaaa;
            aaaa.info = "";
            aaaa.password = "";
            aaaa.otherinfo = "";
            aaaa.numberofpassword = pswlist.Count();
            aaaa.MYpasswordList = pswlist;

            EncryptionClass.SavePassword(aaaa, "sdfafdd");
        }

        private static PassWordDic UserInit(String Name, String key, String otherinfo = "")
        {
            string pswget = EncryptionClass.sha256(key);
            PassWordDic ddd = new PassWordDic();
            ddd.info = Name;
            ddd.password = pswget;
            ddd.otherinfo = otherinfo;
            ddd.numberofpassword = 0;
            ddd.MYpasswordList = new List<PassWordStruct>();
            return ddd;
        }
        public static int CreateUserMain(String superpsw, String username, String key)
        {
            //错误代码10
            //判断超级管理员密码
            if(superpsw!= "Superbndkg")
            {
                return 2;
            }

            CurUserinit(username);
            string sourcepath = pswpath + "psw";
            //读取用户名是否已在服务器中有存储且是否结构完整 错误代码2
            if (File.Exists(sourcepath))
            {
                //用户名已存在无法新建
                return 3;
            }
            
            System.IO.Directory.CreateDirectory(pswpath);
            System.IO.Directory.CreateDirectory(bakpath);

            //新建用户配置结构体
            PassWordDic curPWD = UserInit(username, key);
            //保存用户配置结构体到指定路径(错误代码5)
            EncryptionClass.SavePassword(curPWD, sourcepath);
            //这里还有一些防错todo等用户很多了再说

            return 1;
        }

        private int DeleteUserMain()
        {
            //删除整个账户

            //是否增加密码 错误代码3

            //是否修改key

            //是否修改已保存的密码 错误代码4

            //是否删除某个密码 错误代码5

            return 1;
        }
        public static int UploadUserMain(string[] names,string[] keys, string[] infos, out List<string>repeats)
        {
            //错误代码20
            repeats = new List<string>() ;

            //打开本地内容
            string sourcepath = pswpath + "psw";
            PassWordDic curcccc = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);
            //读取本地密码到一个list中

            //循环用户上传的密码如果在list中存在检查密码是否一致，不一致将此条记录到重复list中
            int i = 0;
            foreach (var item in infos)
            {
                if (item=="")
                {
                    //检测路径名是否合法
                    i++;
                    continue;
                }
                if (curcccc.info.Contains(item))
                {
                    int index = curcccc.info.IndexOf(item);

                    int sdfsf = 9;
                }
                else
                {
                    PassWordStruct curpswstuct = (PassWordStruct)EncryptionClass.LoadPasswordPure(item);
                    curpswstuct.name = names[i];
                    curpswstuct.key = keys[i];
                    curpswstuct.info = infos[i];


                    curcccc.MYpasswordList.Add(curpswstuct);
                }


                i++;

            }
            EncryptionClass.SavePassword(curcccc, sourcepath);

            return 1;
        }

        private int SearchUserMain()
        {
            //暂时不用

            return 1;
        }
        public static int UserCheck(String Username, String KEY)
        {
            //确认用户名密码是否正确
            //正确返回1 错误返回2 不存在返回0
            CurUserinit(Username);
            string sourcepath = pswpath + "psw";
            if (!File.Exists(sourcepath))
            {
                //用户名不存在
                return 2;
            }

            PassWordDic curbbbb = (PassWordDic)EncryptionClass.LoadPassword(sourcepath);

            string pswget = EncryptionClass.sha256(KEY);
            if (curbbbb.password == pswget)
            {
                return 1;
            }
                

            return 0;
        }
    }
}
