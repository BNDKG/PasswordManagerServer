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
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.

                Int32 port = Convert.ToInt32(textBox1.Text);
                string ipadd = textBox2.Text;


                IPAddress localAddr = IPAddress.Parse(ipadd);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();


                //String data = null;


                //设置退出flag
                Endflag = true;


                // Enter the listening loop.
                while (Endflag)
                {

                    try
                    {
                        if (!server.Pending())
                        {

                            //为了避免每次都被tcpListener.AcceptTcpClient()阻塞线程，添加了此判断，

                            //no connection requests have arrived。

                            //当没有连接请求时，什么也不做，有了请求再执行到tcpListener.AcceptTcpClient()

                        }
                        else
                        {
                            //Console.Write("Waiting for a connection... ");
                            // Perform a blocking call to accept requests.
                            // You could also user server.AcceptSocket() here.
                            TcpClient client = server.AcceptTcpClient();

                            Console.WriteLine("Connected!");

                            //data = null;

                            // Get a stream object for reading and writing
                            NetworkStream stream = client.GetStream();
                            
                            //int i;

                            // Loop to receive all the data sent by the client.
                            //while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) { }

                            // Buffer for reading data
                            Byte[] bytes = new Byte[11];

                            stream.Read(bytes, 0, bytes.Length);

                            // 将传来字符串转换为字符串数组,todo添加长度限制防止崩溃
                            //data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                            //string[] firstget=(string[])EncryptionClass.BytesToObject(bytes);
                            //
                            string[] firstget = EncryptionClass.BytesToStringArr(bytes);

                            ClientType ddd;
                            if (GetClientType(firstget) == 1)
                            {
                                ddd = ClientType.Csharp;
                            }
                            else if(GetClientType(firstget) == 2)
                            {
                                ddd = ClientType.Android;
                            }
                            else
                            {
                                ddd = ClientType.Csharp;
                            }

                            //判断安卓还是c#客户端

                            switch (ddd)
                            {
                                case ClientType.Csharp:

                                    Byte[] bytes2 = new Byte[256];

                                    stream.Read(bytes2, 0, bytes2.Length);

                                    string[] csharplogicget = (string[])EncryptionClass.BytesToObject(bytes2);

                                    //处理请求并回复
                                    byte[][] backmsgs = backmsglogic(csharplogicget);

                                    foreach (var singlemsg in backmsgs)
                                    {
                                        int len = singlemsg.Length;

                                        stream.Write(singlemsg, 0, len);

                                        //这里考虑下要不要wait
                                    }


                                    break;
                                case ClientType.Android:
                                    break;
                                default:
                                    break;
                            }


                            // Shutdown and end connection
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


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();

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
                        CurUserinit(nameupload);
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
                        CurUserinit(nameupload);
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
        private void syncpassword()
        {

        }
        private void uploadpassword()
        {

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
            aaaa.Name = "";
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
            CurUserinit("BNDKG");
            bakOnLoad();

            saveandchange();
        }

        private static void CurUserinit(string username)
        {
            //这里改多用户名逻辑
            //string username = "BNDKG";

            CurUserPath = UsersDataPath + username + "\\";
        }


        static List<string> bakPWDPathList = new List<string>();
        static List<string> bakPWDNameList = new List<string>();

        private static void bakOnLoad()
        {
            string bakpath = CurUserPath + "bak\\";
            //获取指定文件夹的所有文件
            string[] paths = Directory.GetFiles(bakpath);
            foreach (var item in paths)
            {
                //获取文件后缀名
                string extension = Path.GetExtension(item).ToLower();
                if (extension == ".bak")
                {

                    string savename=Path.GetFileNameWithoutExtension(item).ToLower();
                    bakPWDPathList.Add(item);
                    bakPWDNameList.Add(savename);
                }
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void saveandchange()
        {
            int i = 0;

            List<PassWordStruct> pswlist = new List<PassWordStruct>();

            foreach (var item in bakPWDPathList)
            {

                PassWordStruct curpswstuct = (PassWordStruct)EncryptionClass.LoadPasswordPure(item);
                curpswstuct.info= bakPWDNameList[i];
                pswlist.Add(curpswstuct);


                i++;

            }


            PassWordDic aaaa;
            aaaa.Name = "";
            aaaa.password = "";
            aaaa.otherinfo = "";
            aaaa.numberofpassword = pswlist.Count();
            aaaa.MYpasswordList = pswlist;

            EncryptionClass.SavePassword(aaaa, "ceshi");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            PassWordDic ddd = new PassWordDic();
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {

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
        public string Name;
        public string password;
        public string otherinfo;
        public int numberofpassword;
        //public PassWordStruct[] MYpasswords;
        public List<PassWordStruct> MYpasswordList;


    }


    public class EncryptionClass
    {
        public static object LoadPassword(string filename)            //序列化读取文件
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
                MessageBox.Show("读取失败！！！");
            }

            return password;
        }
        public static object LoadPasswordPure(string filename)            //序列化读取文件
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
                MessageBox.Show("备份读取失败！！！");
            }

            return password;
        }

        public static void SavePassword(object password, string filename) //序列化保存
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, password);
                fs.Close();
                MessageBox.Show("保存成功！");
            }
            catch (Exception)
            {
                MessageBox.Show("保存失败！！！");
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


    }

}
