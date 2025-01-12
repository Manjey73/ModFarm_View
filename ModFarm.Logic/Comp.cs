using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using ScadaCommFunc;
using Scada.Server.Modules.ModFarm.Logic.Properties;
using Scada.Lang;
using Scada.Log;

namespace Scada.Server.Modules.ModFarm.Logic
{
    internal class Comp : ModuleLogic
    {
        private Comp comp;
        public Comp(IServerContext serverContext)
            : base(serverContext)
        {
            comp = this;
            moduleLog = new LogFile(LogFormat.Simple)
            {
                FileName = Path.Combine(serverContext.AppDirs.LogDir, LogFileName),
                CapacityMB = serverContext.AppConfig.GeneralOptions.MaxLogSize
            };
            //this.serverContext = serverContext;
        }

        /// <summary>
        /// Gets the module code.
        /// </summary>
        public override string Code => "ModFarm";

        #region Variables
        private int timedate4;
        private uint license;
        private DateTime _dt;

        private byte[] allserial = new byte[24];
        private readonly AppReg appReg = new AppReg();

        private const string LogFileName = "ModFarm.log"; // The module log file name.
        private readonly ILog moduleLog;            // the module log

        private static string strcode = "";
        private readonly byte[] _publicKey = new byte[] { 0x9C,0x08,0x52,0x08,0x49,0x46,0xF8,0x5D,0xE5,0x3A,0x8A,0x34,0x2B,0xB2,0x3F,0x2A,0xD2,0x28,0x83,0xE3,0xE5,0xEC,0xB6,0xE5,0xC2,0xF9,0x63,0x03,0x6E,0xA9,0x3F,0x60,0xF8,0xC4,0x6A,
            0xB6,0x39,0x91,0x1E,0x6C,0x83,0x4E,0xE0,0x96,0x79,0x27,0xD4,0x43,0x74,0x1C,0x39,0x3E,0xBC,0x8F,0x87,0x45,0xFE,0x7D,0xB3,0x9F,0xAF,0x28,0x97,0xE9,0x73,0xE4,0xF4,0x18,0xF9,0xAE,0x74,0x22,0x37,0xC4,0xD1,0xD6,0x54,0x54,0x9F,
            0x89,0x8E,0x56,0xF5,0xEB,0xA9,0x01,0x9E,0x34,0x7F,0x9C,0x79,0xD4,0xAA,0x49,0x4A,0x1B,0xA4,0x6D,0xDF,0x0C,0x78,0xDD,0x65,0x8A,0xEE,0x2F,0x49,0x16,0x13,0x22,0xC0,0xCD,0x2E,0x18,0x65,0x85,0x48,0x8E,0x89,0xEF,0xB9,0xE0,0x1E,
            0xE8,0x96,0x83,0xAA,0xB8,0x90,0x98,0x87,0x94,0xA4,0x87,0x02,0x8B,0xF6,0x14,0xDD,0x56,0x28,0xE2,0xEE,0x5B,0xF1,0xF2,0xC2,0xC0,0x9C,0x51,0xE3,0x00,0xDA,0x16,0x6B,0x32,0x32,0xD3,0xDA,0x39,0xC2,0xF9,0x1B,0x7F,0xEB,0xC3,0x8D,
            0x3B,0x54,0xA7,0xF4,0x75,0x10,0xB6,0x5F,0x96,0xE1,0x00,0xA5,0xEB,0x43,0x38,0xFA,0xA1,0x6B,0x22,0xFE,0xDD,0x5E,0x75,0x7D,0x6F,0x30,0x7E,0xFB,0xC8,0xA3,0x3B,0x5D,0xD8,0xE0,0xDE,0x79,0xC7,0x90,0xAB,0xF4,0x84,0x4E,0xC1,0x04,
            0xA9,0xEE,0x17,0x1A,0xB9,0x15,0x03,0x45,0xF9,0x4E,0xC0,0x29,0x9D,0x32,0xF9,0x95,0x6E,0x07,0x1D,0xDE,0x7A,0x43,0x4B,0xE6,0x58,0x09,0xED,0xB7,0xA1,0x8E,0x9C,0xF8,0x9D,0x72,0x1A,0xA3,0xBF,0xF7,0xFE,0x68,0x55,0x48,0x87,0xBF,
            0xB4,0xF8,0x32,0x45,0xA8,0x05,0x5B,0xDF,0xFA,0xE4,0x20,0xE2,0x97,0xCB,0xED,0xF1,0x8B,0x29,0xBC,0xE4,0xAC,0x49,0xD5,0xB3,0xEF,0x67,0x73,0xD7,0xCD,0x59,0x6F,0xED,0x32,0x97,0xEC,0x60,0x86,0xF0,0xAD,0x8B,0x3D,0x8C,0x39,0x5C,
            0x53,0x64,0xDD,0xF2,0x33,0x80,0x2F,0x28,0x9C,0x16,0x56,0x46,0x2B,0x03,0x80,0x02,0xC9,0x9A,0xDD,0x69,0x3F,0xE2,0xE6,0x0A,0x58,0x86,0xEA,0xEB,0x44,0xCD,0x5C,0x65,0xD5,0x50,0x09,0x21,0x21,0xDD,0xFD,0x7E,0xD6,0x4B,0xD2,0xDD,
            0xFB,0x2A,0x85,0x3C,0xC1,0x53,0x21,0xA0,0xE6,0x26,0xE1,0x8C,0x71,0x31,0x53,0xE2,0x55,0x86,0x9D,0xC2,0xA4,0xC0,0xBB,0x16,0x15,0xF9,0x25,0xD3,0xAD,0x67,0x0E,0x8B,0x46,0x3E,0xC0,0x21,0x96,0x97,0x28,0xE4,0x3E,0x79,0xCE,0x54,
            0x30,0xD8,0x4A,0xFF,0xB0,0x1E,0x1E,0x84,0xC0,0x9F,0x21,0xED,0x27,0xE0,0x85,0x91,0x03,0x50,0xA5,0x27,0x5C,0x31,0x67,0x62,0xEB,0x0A,0x10,0x67,0x16,0x29,0x8E,0xEC,0x97,0x5D,0xA6,0x66,0xB8,0x8F,0xA6,0x22,0x75,0x1A,0x98,0x17,
            0x57,0x3E,0xE9,0x79,0x6A,0x26,0x40,0x30,0xC4,0x9E,0x11,0x17,0xC2,0x7D,0x4E,0xC1,0xEB,0x1D,0xA5,0x1D,0x6B,0x7B,0x13,0xD5,0xFE,0x2E,0x3D,0xE7,0xEB,0x51,0xAA,0x88,0x03,0x23,0xF4,0xBE,0x4F,0xDB,0x0C,0xA3,0x3F,0x12,0x5C,0x6B,
            0x9F,0x0C,0x26,0xE3,0x08,0xB1,0x3E,0x9A,0xC6,0xD5,0x54,0xEB,0xBD,0x6B,0x59,0xC8,0x0E,0xD4,0x9B,0x6F,0xF3,0x11,0x7A,0xAA,0xED,0x24,0x97,0xEA,0x2A,0x1A,0x85,0xE2,0xBC,0x4E,0x9E,0x4D,0x61,0x4A,0x0F,0x25,0x66,0xC2,0xCA,0x0D,
            0xEF,0xA3,0x04,0xF9,0x33,0x03,0x45,0xF0,0x5E,0x5E,0x9D,0xCA,0x20,0x78,0x57,0x38,0x5A,0x76,0xB6,0xDE,0xA2,0xE9,0x66,0x72,0xCC,0x37,0x85,0x1A,0x94,0x28,0x09,0xFE,0x0D,0x8E,0xC1,0x9F,0x82,0xD5,0xF3,0x30,0xFE,0xAD,0x76,0x4A,
            0xFD,0x34,0xDA,0x61,0x1F,0xA3,0x18,0xF3,0xAE,0xC9,0x50,0xA5,0x44,0xED,0x9D,0x7B,0xB4,0x44,0xF7,0x80,0xC5,0xA4,0x2F,0xA4,0x93,0x46,0x6C,0xB1,0x55,0x66,0x25,0x5B,0x67,0x49,0x6F,0x8E,0x95,0xA4,0xA7,0xA7,0x75,0xCC,0x83,0xC4,
            0x09,0xFD,0xA0,0x4C,0x56,0x4D,0xB5,0xD4,0x87,0x28,0xF7,0x4B,0xE3,0xD6,0x34,0x8B,0xAD,0x28,0x11,0xBA,0x4B,0xBF,0x0B,0x18,0x3D,0x41,0x52,0xE3,0xCA,0x1B,0x7B,0x4B,0xD4,0xDA,0x68,0x93,0x68,0xCE,0x1A,0x07,0x40,0x64,0xDB,0x82,
            0x5F,0x73,0x50,0x84,0x04,0x11,0x95,0xFD,0x72,0xC8,0x1F,0x45,0x54,0x57,0xA6,0x76,0x97,0x0C,0x99,0xE3,0x9F,0xCC,0x33,0xBD,0xB6,0xEE,0xDB,0x5A,0x72,0xE2,0xB3,0x2D,0xC7,0xBD,0x9D,0x7D,0x80,0xAC,0xA4,0x47,0x57,0x34,0x9A,0xB4,
            0x95,0x11,0x84,0xEA,0x70,0x39,0x11,0x4B,0xBC,0x4D,0xE8,0x58,0x6E,0xBC,0xEC,0xBE,0x96,0x61,0x70,0x84,0x3F,0x39,0x44,0x22,0x3A,0x1C,0xC9,0x26,0xBA,0x8E,0x30,0x9B,0x9A,0xC6,0x3C,0xC2,0x1E,0x2C,0xA2,0x8D,0xC0,0x99,0x71,0xCF,
            0xA6,0xA6,0xCA,0xDC,0x8C,0x4C,0x94,0x2D,0xE4,0x8A,0x20,0xAC,0xA5,0x41,0xCA,0xF6,0x99,0x99,0xBE,0x42,0xA2,0x89,0x97,0x60,0xC9,0x9D,0x7D,0xE7,0x25,0x1B,0x08,0x40,0x37,0x60,0xF9,0x11,0x93,0x1A,0x1D,0x12,0x7B,0xD4,0x94,0x5D,
            0xAC,0x00,0xDB,0x76,0x57,0x86,0x71,0x42,0x72,0x9E,0x01,0x26,0xD2,0xCD,0x08,0x22,0x82 };
        private byte[] byteget = new byte[24];
        private byte[] pere = new byte[24];
        private int er = 0;
        private bool xche = false;
        protected DateTime myNewDTs => _dt;

        protected uint myLicense => license;
        protected bool myAct() => Act();
        #endregion Variables

        private DateTime FromDateTime4(int date)
        {
            DateTime dt = new DateTime(date / 384, (date / 32) % 12 + 1, date % 32);
            return dt;
        }

        private void bitmask(byte[] mask) // 7-ка тут чтобы не было чисел больше 7-ми, можно добавить смещение а маску из 3-х последовательных битов наложить ранее
        {
            byteget = Encoding.UTF8.GetBytes(appReg.KeyFileName());
            for (int f = 0; f < 8; f++)
            {
                bool cycl = true;
                int sd = mask[f] & 7;
                while (cycl)
                {
                    int index = Array.IndexOf(byteget, (byte)sd);
                    if (index < 0)
                    {
                        byteget[f] = (byte)sd;
                        cycl = false;
                    }
                    else
                    {
                        sd++;
                        if (sd > 7) sd = 0;
                    }
                }
            }
        }

        private void datecheck(bool check)
        {
            //_ = DateTime.Now;
            if (check)
            {
                double maxdt;
                try
                {
                    // Чтение времени из ключа драйвера TEST бредятина какая-то, типа если время не задано?
                    DateTime dtkey = FromDateTime4(timedate4);
                    maxdt = dtkey.ToOADate();

                    if (dtkey.ToOADate() > (DateTime.MaxValue.ToOADate() - 365000))
                    {
                        er = 6; // "Registration key is valid"
                        _dt = DateTime.MaxValue;
                        moduleLog.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
                        Log.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
                    }
                    else
                    {
                        if (DateTime.Now.ToOADate() > maxdt)
                        {
                            er = 9; // "Registration key is expired {0}"
                            _dt = DateTime.Now;
                            license = 0;
                            xche = false;
                            moduleLog.WriteAction($"{string.Format(warn(18), Code)} {string.Format(warn(er), dtkey)}");
                            moduleLog.WriteLine($"{string.Format(warn(19), strcode)}");
                            Log.WriteAction($"{string.Format(warn(18), Code)} {string.Format(warn(er), dtkey)}");
                            Log.WriteLine($"{string.Format(warn(19), strcode)}");
                        }
                        else
                        {
                            er = 7; // "Registration key is valid. Expiration date is {0}
                            _dt = dtkey; // Сюда записать дату ключа.... dtkey.AddDays(1)
                            moduleLog.WriteAction($"{string.Format(warn(18), Code)} {string.Format(warn(er), dtkey)}");
                            Log.WriteAction($"{string.Format(warn(18), Code)} {string.Format(warn(er), dtkey)}");
                        }
                    }
                }
                catch // Не отрабатывает при некорректном времени.... TEST TEST сюда код не доходит...
                {
                    er = 12; // "Registration key is incorrect" ------ НЕ ОТРАБАТЫВАЕТ
                    //strInfo = string.Concat(strInfo, warn(er));
                }
            }
            else
            {
                er = 8; //  "Registration key is not valid"
                moduleLog.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
                Log.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
            }
        }

        // Перестановка байт в массиве  хэш + дата (24 байта) согласно маске в массиве byteget для драйвера
        private byte[] PerDrv(byte[] mass, int c)
        {
            byte[] drvout = new byte[24];
            for (int r = c; r >= 0; r--)
            {
                int l = r;
                for (int r1 = 0; r1 < 8; r1++)
                {
                    drvout[r1 + (8 * r)] = mass[byteget[r1] + (8 * l)];
                    l--;
                    if (l < 0) l = c;
                }
            }
            return drvout;
        }

        private string getCode(out string name, out string mac, out string exedir) //, out bool listm) // получение кода ПК, имени ПК и списка MAC адресов
        {

            Assembly devlib = Assembly.Load(Resources.DevLib); // Resources.DevLib Assembly.Load("DevLib")
            string code = devlib.GetType("Scada.Reg.CompCodeUtils", true).GetMethod("GetCompCode").Invoke(null, null).ToString();

            object[] strNM = new object[4];
            strNM[0] = code;

            devlib.GetType("Scada.Reg.CompCodeUtils", true).GetMethod("GetCompCodeInfo").Invoke(null, strNM); // object mlist = // (null, strNM)

            name = strNM[1].ToString();
            mac = strNM[2].ToString();
            exedir = strNM[3].ToString();
            return code;
        }

        private bool load(string fileName, out string keyCode, out string errMsg)
        {
            bool flag;
            try
            {
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(string.Format(warn(13), fileName)); // warn 13 - "Файл регистрационного ключа {0} не найден."
                }
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                keyCode = (xmlDocument.DocumentElement.Name == "RegKey" ? xmlDocument.DocumentElement.InnerText.Trim() : "");
                errMsg = "";
                flag = true;
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                keyCode = "";
                errMsg = string.Concat(warn(16), ": ", exception.Message); // warn 16 - "Ошибка при загрузке регистрационного ключа"
                flag = false;
            }
            return flag;
        }

        private bool save(string fileName, string compCode, out string errMsg)
        {
            try
            {
                string existingCompCode = File.Exists(fileName) ? File.ReadAllText(fileName) : "";

                if (existingCompCode != compCode)
                    File.WriteAllText(fileName, compCode);

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        private bool LoadKey()
        {
            string compName;
            string mAddr;
            string exedir;
            strcode = getCode(out compName, out mAddr, out exedir); // получить код компьютера (out compName, out mAddr, out exedir)

            string keyCode;
            string errLoad;

            string errSave;
            bool s = save(string.Concat(ServerContext.AppDirs.LogDir, appReg.CodeFileName()), strcode, out errSave); //  appDirs.LogDir,

            // Загрузка ключа
            bool l = load(string.Concat(ServerContext.AppDirs.ConfigDir, appReg.KeyFileName()), out keyCode, out errLoad); // bool l = load(string.Concat(appDir.ConfigDir, appReg.KeyFileName()), out keyCode, out errLoad);  // serverContext.AppDirs.
            if (!l)
            {
                if (!s)
                {
                    er = 15;    // 15: error = "Error saving computer code";
                    Log.WriteError(warn(er), errSave);
                }
                er = 13; // warn 13 - "Файл регистрационного ключа {0} не найден."
                moduleLog.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", errLoad)}");
                moduleLog.WriteLine($"{string.Format(warn(19), strcode)}");
                Log.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", errLoad)}");
                Log.WriteLine($"{string.Format(warn(19), strcode)}");
            }
            else // Если код загружен, отправляем на проверку
            {
                // Прочитать код из файла с проверкой, что он 16-чный
                var keyString = keyCode.Replace("=", "").Replace("-", "");

                if (ConvFunc.IsHex(keyString))
                {
                    byte[] npere = ScadaUtils.HexToBytes(keyString, true);
                    if (npere == null && keyString.Length > 0)
                    {
                        moduleLog.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", warn(5))}"); // er = 5 "Registration key info is incorrect."
                        moduleLog.WriteLine($"{string.Format(warn(19), strcode)}");

                        Log.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", warn(5))}"); // er = 5 "Registration key info is incorrect."
                        Log.WriteLine($"{string.Format(warn(19), strcode)}");
                    }
                    else
                    {
                        if (npere.Length > 0) // npere.Length > 0
                        {
                            Array.Copy(npere, 0, pere, 0, npere.Length);

                            if (npere.Length != 24)
                            {
                                Array.Resize(ref pere, npere.Length);
                                // er = 4 "Registration key length is incorrect."
                                moduleLog.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", warn(4))}"); 
                                moduleLog.WriteLine($"{string.Format(warn(19), strcode)}");
                                Log.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", warn(4))}");
                                Log.WriteLine($"{string.Format(warn(19), strcode)}");
                            }
                            else
                            {
                                // Здесь сделать проверку по общему ключу, если не совпало, переходить к проверке по мак адресам

                                byte[] sout = Encoding.UTF8.GetBytes(strcode);
                                Array.Resize(ref sout, 8);

                                // Определение маски перестановки по MAC адресу
                                bitmask(sout);  // на основе 8 байт MAC + два доп байта сделать маску перестановки

                                // Перестановка в драйвере отличается от перестановки в генераторе ключа
                                // обратный порядок идет, должно усложнить создание кейгена
                                allserial = PerDrv(pere, 2);

                                timedate4 = BitConverter.ToInt32(allserial, 16);
                                license = BitConverter.ToUInt32(allserial, 20);

                                byte[] key = new byte[16];
                                Array.Copy(allserial, 0, key, 0, 16);

                                // добавить Время и лицензии для проверки подписи
                                byte[] testm = Encoding.UTF8.GetBytes($"{appReg.ProdCode()}{strcode}{timedate4}{license}"); // Для общего ключа

                                xche = VerifySignature(testm, key);

                                if (!xche)
                                {
                                    // Проверка по мак адресам
                                    string[] str_out = mAddr.Split("; ");
                                    int str_outx = 0;
                                    do
                                    {
                                        sout = ConvFunc.StringToHex(str_out[str_outx]);
                                        Array.Resize(ref sout, 8);
                                        // добавление до 8-ми байт, чтобы не были нулями из md5 хеша
                                        sout[7] = sout[0];
                                        sout[6] = sout[1];

                                        // Определение маски перестановки по MAC адресу
                                        bitmask(sout);  // на основе 8 байт MAC + два доп байта сделать маску перестановки

                                        // Перестановка в драйвере отличается от перестановки в генераторе ключа
                                        // обратный порядок идет, должно усложнить создание кейгена
                                        allserial = PerDrv(pere, 2);

                                        timedate4 = BitConverter.ToInt32(allserial, 16);
                                        license = BitConverter.ToUInt32(allserial, 20);

                                        key = new byte[16];
                                        Array.Copy(allserial, 0, key, 0, 16);

                                        // добавить Время и лицензии для подписи
                                        testm = Encoding.UTF8.GetBytes($"{appReg.ProdCode()}{compName}{exedir}{str_out[str_outx]}{timedate4}{license}");

                                        xche = VerifySignature(testm, key);
                                        str_outx++;
                                    }
                                    while (str_outx < str_out.Length && xche == false);
                                }
                                datecheck(xche);
                            }
                        }
                        else
                        {
                            moduleLog.WriteAction($"{string.Format(warn(18), Code)} {warn(10)}"); // er = 10 "Registration key is empty"
                            Log.WriteAction($"{string.Format(warn(18), Code)} {warn(10)}"); // er = 10 "Registration key is empty"
                        }
                    }
                }
                else
                {
                    // Ключ не HEX строка
                    moduleLog.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", warn(0))}"); // er = 0 "String is not hexadecimal."
                    moduleLog.WriteLine($"{string.Format(warn(19), strcode)}");
                    Log.WriteError($"{string.Concat(string.Format(warn(18), Code), " ", warn(0))}"); // er = 0 "String is not hexadecimal."
                    Log.WriteLine($"{string.Format(warn(19), strcode)}");
                }
            }
            return xche; // Вроде как проверка - результат true в случае успеха - здесь нужно число для лицензий
        }

        private static string warn(int err)
        {
            string error = "";
            if (Locale.IsRussian)
            {
                switch (err)
                {
                    case 0: error = "Строка не является 16-ричной записью."; break;
                    case 1: error = "Ошибка при извлечении информации регистрационного ключа"; break;
                    case 2: error = "Ошибка при декодировании кода компьютера"; break;
                    case 3: error = "Ошибка при получении информации о регистрационном ключе"; break;
                    case 4: error = "Некорректная длина регистрационного ключа."; break;
                    case 5: error = "Некорректная информация регистрационного ключа."; break;
                    case 6: error = "Регистрационный ключ действителен."; break;
                    case 7: error = "Регистрационный ключ действителен. Дата окончания {0}"; break;
                    case 8: error = "Регистрационный ключ не действителен."; break;
                    case 9: error = "Регистрационный ключ истёк {0}."; break;
                    case 10: error = "Регистрационный ключ пуст."; break;
                    case 11: error = "Регистрационный ключ содержит запись об ошибке."; break;
                    case 12: error = "Регистрационный ключ некорректный."; break;
                    case 13: error = "Файл регистрационного ключа {0} не найден."; break;
                    case 14: error = "Ошибка при загрузке кода компьютера"; break;
                    case 15: error = "Ошибка при сохранении кода компьютера"; break;
                    case 16: error = "Ошибка при загрузке регистрационного ключа"; break;
                    case 17: error = "Ошибка при сохранении регистрационного ключа"; break;
                    case 18: error = "Проверка регистрации {0}:"; break;
                    case 19: error = "Код компьютера: {0}"; break; // {0}
                    case 20: error = "Регистрация не удалась."; break;
                }
            }
            else
            {
                switch (err)
                {
                    case 0: error = "String is not hexadecimal."; break;
                    case 1: error = "Computer code contains error record."; break;
                    case 2: error = "Error decoding computer code"; break;
                    case 3: error = "Error retrieving registration key info"; break;
                    case 4: error = "Registration key length is incorrect."; break;
                    case 5: error = "Registration key info is incorrect."; break;
                    case 6: error = "Registration key is valid"; break;
                    case 7: error = "Registration key is valid. Expiration date is {0}"; break;
                    case 8: error = "Registration key is not valid"; break;
                    case 9: error = "Registration key is expired {0}"; break;
                    case 10: error = "Registration key is empty"; break;
                    case 11: error = "Registration key contains error record"; break;
                    case 12: error = "Registration key is incorrect"; break;
                    case 13: error = "Registration key file {0} not found."; break;
                    case 14: error = "Error loading computer code"; break;
                    case 15: error = "Error saving computer code"; break;
                    case 16: error = "Error loading registration key"; break;
                    case 17: error = "Error saving registration key"; break;
                    case 18: error = "Check \"{0}\" registration:"; break;
                    case 19: error = "Computer code: {0}"; break; // "Computer code: {0}"; break;
                    case 20: error = "Registration failed."; break;
                }
            }
            return error;
        }

        private bool VerifySignature(byte[] value, byte[] signature)
        {
            if (null == signature || signature.Length <= 0)
            {
                //er = 10; //10: error = "Registration key is empty"
                //moduleLog.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
                //Log.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
                return false;
            }

            byte[] hash = computeHash(value);

            uint pos = BitConverter.ToUInt32(hash, hash.Length - 16);

            pos = pos % 50; // Указать сразу аналогично как константу или прописать тут ((uint)_keyLength)

            byte[] etalone = new byte[16]; // _signatureLength все 16

            for (int i = 0; i < 16; i++)
            {
                etalone[i] = _publicKey[pos * 16 + i];
            }

            byte[] signatureHash = computeHash(signature);

            for (int i = 0; i < 16; i++)
            {
                if (etalone[i] != signatureHash[i])
                {
                    er = 8; //8: error = "Registration key is not valid"
                    //moduleLog.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");
                    //Log.WriteAction($"{string.Format(warn(18), Code)} {warn(er)}");

                    return false;
                }
            }
            return true;
        }

        private byte[] computeHash(byte[] value)
        {
            for (int i = 0; i < 512; i++) // INTRICACY
            {
                value = MD5.Create().ComputeHash(value);
            }
            return value;
        }

        private bool Act()
        {
            return comp.LoadKey();
        }

        private class AppReg : IAppReg
        {

            public AppReg()
            {
            }

            string IAppReg.KeyFileName => throw new NotImplementedException();

            string IAppReg.ProdCode => throw new NotImplementedException();

            string IAppReg.ProdName => throw new NotImplementedException();

            string IAppReg.CodeFileName => throw new NotImplementedException();

            public string KeyFileName()
            {
                return "ModFarm_Reg.xml";
            }

            public string ProdCode()
            {
                return "ModFarm";
            }

            public string ProdName()
            {
                return "Farm Management Module";
            }

            public string CodeFileName()
            {
                return "CompCode.txt";
            }
        }
        private interface IAppReg
        {
            string KeyFileName
            {
                get;
            }

            string ProdCode
            {
                get;
            }

            string ProdName
            {
                get;
            }

            string CodeFileName
            {
                get;
            }
        }
    }
}
