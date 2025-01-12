using Scada.Lang;
using Scada.Log;
using Scada.Server.Modules.ModFarm.Config;
using ScadaCommFunc;
using System.Reflection;

namespace Scada.Server.Modules.ModFarm.Logic
{
    internal class ModLogix : Comp
    {
        private IServerContext serverContext;
        private const string LogFileName = "ModFarm.log";   // The module log file name.
        private readonly ILog moduleLog;                    // the module log
        private uint license = 0;
        private Farm farm;
        private Farm.Programs.ProgramX loadprg;
        private AllProgram newProgram;
        Random r = new Random();
        private int rInt;
        private bool active = true;

        #region AllDevice
        public class AllDevice()
        {
            public string group { get; set; }
            public int deviceIndex { get; set; }

            public Farm.Device farmdevice { get; set; }
        }

        public List<AllDevice> devices = new List<AllDevice>();
        #endregion AllDevice

        #region AllProgram
        public class AllProgram() // Класс для создания списка программ с указанием имени группы (Chickens, Cows и т.д.), Индекса устройства в группе, индекса программы, и собственно данных самой программы.
        {
            public string group { get; set; }
            public string devName { get; set; }
            public int deviceIndex { get; set; }
            public int programIndex { get; set; }
            public Farm.Programs.ProgramX program { get; set; }
        }

        private List<AllProgram> programs = new List<AllProgram>();
        #endregion AllProgram

        // Сюда заносить загруженные и созданные активные программы
        public List<AllProgram> loadProgram = new List<AllProgram>();

        // Получение времени и количества лицензий
        #region GetmyDT
        private DateTime GetmyDT()
        {
            return myNewDTs;
        }

        private uint GetLicense()
        {
            return myLicense;
        }
        #endregion GetmyDT

        private bool Act()
        {
            return myAct();
        }

        #region TimerTon

        private Func.TON tonReq = new Func.TON { IN = false }; // 5 минут Заменить 5 минут на рендом от 5 до 15 минут , PT = 5 * 60000
        #endregion TimerTon

        #region SaveDll
        // -----------------------------------------------               Защита работы dll          ---------------------------------------------------------
        public ModLogix(IServerContext serverContext)
            : base(serverContext)
        {
            this.serverContext = serverContext;
            moduleLog = new LogFile(LogFormat.Simple)
            {
                FileName = Path.Combine(serverContext.AppDirs.LogDir, LogFileName),
                CapacityMB = serverContext.AppConfig.GeneralOptions.MaxLogSize
            };
            firstСheck();
        }

        private DateTime curTime;
        private DateTime endTime;
        //DateTime endTime1 = new DateTime(2025, 1, 10, 14, 50, 0);

        #region ActiveReq
        private bool activeReq()
        {
            bool ret = true;
            if (!tonReq.Q)
            {
                tonReq.Run(true);
            }
            else if (tonReq.Q)
            {
                curTime = DateTime.Now;

                if (curTime <= endTime) // TEST endTime
                {
                    tonReq.Run(false); // Перезапуск таймера TON

                    rInt = r.Next(10, 20); // TEST 
                    tonReq.PT = (ulong)rInt * 60000;
                }
                else
                {
                    license = 0;
                    ret = false;
                }
            }
            return ret;
        }
        #endregion ActiveReq

        private void firstСheck()
        {
            rInt = r.Next(12, 25); // TEST диапазон от 5 до 10

            if (!Act())
            {
                string info = Locale.IsRussian ?
                   $"В демонстрационном режиме доступно только одно устройтсво с ограничением работы Модуля" :
                   $"In the demo mode, only one device is available with limited Module operation.";
                moduleLog.WriteLine(info);
                active = false;
            }
            else
            {
                endTime = GetmyDT();
                license = GetLicense();

                string info = Locale.IsRussian ?
                        $"Количество доступных активных устройств {license}" :
                        $"Number of available active devices {license}";
                moduleLog.WriteInfo(info);
            }
        }
        // -----------------------------------------------  END             Защита работы dll        END  ---------------------------------------------------------
        #endregion SaveDll

        #region AddProgram
        private void AddProgram(AllDevice Dev)
        {
            if (Dev.farmdevice.Programs == null) return;
            if (Dev.farmdevice.Programs.Program.Count() > 0)
            {
                foreach (var p in Dev.farmdevice.Programs.Program)
                {
                    AllProgram allProgram = new AllProgram();
                    allProgram.program = p;
                    allProgram.group = Dev.group;
                    allProgram.deviceIndex = Dev.deviceIndex;
                    allProgram.devName = Dev.farmdevice.DeviceName;
                    allProgram.programIndex = Dev.farmdevice.Programs.Program.IndexOf(p);
                    programs.Add(allProgram);
                }
            }
        }
        #endregion AddProgram

        #region LoadConfig
        public void LoadConfig()
        {
            _LoadConfig();
        }

        private void _LoadConfig()
        {
            farm = new Farm();
            devices.Clear();
            programs.Clear();

            string configFileName = Path.Combine(serverContext.AppDirs.ConfigDir, ModuleConfig.DefaultFileName);

            if (!File.Exists(configFileName)) // Чтение файла шаблона
            {
                moduleLog.WriteError(Locale.IsRussian ?
                    $"Ошибка: Не задан шаблон для модуля {Code}" :
                    $"Error: The template for the module {Code} is not set");
            } // Чтение файла шаблона
            else
            {
                try
                {
                    farm = FileFunc.LoadXml(typeof(Farm), configFileName) as Farm;
                    //fileyes = true;
                }
                catch (Exception err)
                {
                    moduleLog.WriteError(string.Format(Locale.IsRussian ?
                    "Ошибка: " + err.Message :
                    "Error: " + err.Message));
                }
                finally
                {
                    if (farm != null)
                    {
                        PropertyInfo[] propertyInfos = farm.GetType().GetProperties(); // Получение массива 

                        foreach (var property in propertyInfos)
                        {
                            Type propType = property.PropertyType;

                            if (propType.ToString().Contains("Farm+Device")) // propType != typeof(bool)
                            {
                                PropertyInfo propertyInfo = farm.GetType().GetProperty(property.Name);
                                List<Farm.Device> propertyValue = (List<Farm.Device>)propertyInfo.GetValue(farm);

                                int Count = propertyValue.Count();
                                if (Count > 0)
                                {
                                    foreach (var d in propertyValue)
                                    {
                                        AllDevice actDev = new AllDevice();
                                        int index = propertyValue.IndexOf(d);

                                        actDev.group = property.Name;
                                        actDev.deviceIndex = index;
                                        actDev.farmdevice = d;

                                        devices.Add(actDev); // Добавление устройства в список с указанием принадлежности и индекса в группе
                                        AddProgram(actDev);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion LoadConfig

        #region ReadDevice
        public void ReadDevice()
        {


            _ReadDevice();
        }

        private void _ReadDevice()
        {
            licensel = license > 0 ? license : 1;
            lgroup.Clear();
            loadProgram.Clear();

            if (devices.Count > 0)
            {
                moduleLog.WriteInfo(Locale.IsRussian ?
                    $"Загрузка и сохранение конфигураций программ:" :
                    $"Downloading and saving program configurations:");

                foreach (var device in devices)
                {
                    string name = device.farmdevice.DeviceName;

                    if (device.farmdevice.Active)
                    {
                        if (CheckDevice(name))
                        {
                            _ReadProgram(name);
                        }
                        else
                        {
                            // Удалить файлы программ устройств за пределами лицензии
                            var program = programs.Where(x => x.devName == name).ToList();
                            foreach (var prog in program)
                            {
                                string fname = $"{ServerContext.AppDirs.StorageDir}PRG_{prog.group}_di{prog.deviceIndex}_pi{prog.programIndex}.xml"; // Может все же XML через сериализатор, будет разумнее
                                File.Delete(fname);
                            }
                        }
                    }
                    else
                    {
                        // Удалить файлы программ неактивного устройства
                        var program = programs.Where(x => x.devName == name).ToList();
                        foreach (var prog in program)
                        {
                            string fname = $"{ServerContext.AppDirs.StorageDir}PRG_{prog.group}_di{prog.deviceIndex}_pi{prog.programIndex}.xml";
                            File.Delete(fname);
                        }
                    }
                }
            }
            moduleLog.WriteInfo(Locale.IsRussian ?
                "Загрузка логики задач:" :
                "Loading Task logic:");
        }
        #endregion ReadDevice

        #region _ReadProgram
        private void _ReadProgram(string name)
        {
            if (programs.Count > 0) // На случай, если программ нет вообще
            {
                var program = programs.Where(x => x.devName == name).ToList();

                foreach (var prog in program)
                {
                    string fname = $"{ServerContext.AppDirs.StorageDir}PRG_{prog.group}_di{prog.deviceIndex}_pi{prog.programIndex}.xml";

                    if ((prog.program.ProgramActive && !File.Exists(fname)) || (prog.program.ProgramActive && prog.program.ProgramRestart))
                    {
                        // добавить создание файла если ProgramRestart
                        CreateFile(prog, fname, true);
                    }
                    else if (prog.program.ProgramActive && File.Exists(fname))
                    {
                        if (LoadPrg(fname, prog)) // Передать имя и программу для загрузки
                        {
                            moduleLog.WriteLine(Locale.IsRussian ?
                                $"Файл {fname} Загружен" :
                                $"File {fname} Is uploaded");
                        }
                        else
                        {
                            moduleLog.WriteError(Locale.IsRussian ?
                                $"Ошибка загрузки файла {fname}" :
                                $"File {fname} upload error");
                        }
                    }
                    else
                    {
                        File.Delete(fname);
                    }
                }
            }
            else
            {
                moduleLog.WriteError(Locale.IsRussian ?
                    $"Нет добавленных программ" :
                    $"There are no added programs");
            }
        }
        #endregion _ReadProgram

        #region TestInitialize
        public bool Initialize(Farm.Programs.ProgramX.Input prgIn)
        {
            return _Initialize(prgIn);
        }
        private bool _Initialize(Farm.Programs.ProgramX.Input prgIn)
        {
            if (prgIn.Initialize && license != 0) return true;
            return false;
        }
        #endregion TestInitialize

        #region TestRetain
        public bool Retain(Farm.Programs.ProgramX.Output prgOut)
        {
            return _Retain(prgOut);
        }
        private bool _Retain(Farm.Programs.ProgramX.Output prgOut)
        {
            if (prgOut.Retain && license != 0) return true;
            return false;
        }
        #endregion TestRetain

        #region SaveFile
        public bool SaveFile(AllProgram prog, string fname)
        {
            return _SaveFile(prog, fname);
        }

        private bool _SaveFile(AllProgram prog, string fname)
        {
            if (!activeReq() && active)
            {
                moduleLog.WriteInfo(Locale.IsRussian ?
                    $"Время активации истекло, есть ограничения в работе Модуля" :
                    $"The activation time has expired, and there are limitations in the Module's operation");
                active = !active;
            }

            return SavePrg(prog, fname);
        }
        #endregion SaveFile

        private void CreateFile(AllProgram prog, string fname, bool addprg = false) // Farm.Programs.ProgramX prg
        {
            // Сохранение файлов при помощи сериализации
            prog.program.ProgramRestart = false;
            SavePrg(prog, fname, addprg);
        }

        private bool SavePrg(AllProgram prog, string fname, bool addprg = false) // Farm.Programs.ProgramX prg
        {
            try
            {
                FileFunc.SaveXml(prog.program, fname); // добавить сохранение в выбранный файл TEST
                if (farm.VisibleLogPrgFile && !addprg)
                {
                    LogWriteLine(fname);
                }
                else if (addprg)
                {
                    LogWriteLine(fname);
                }

                if (addprg) loadProgram.Add(prog); // Добавить программу в список активных программ
                return true;
            }
            catch (Exception ex)
            {
                moduleLog.WriteInfo(Locale.IsRussian ?
                    $"Ошибка записи {fname}  {ex.ToString()}" :
                    $"Recording error {fname}  {ex.ToString()}");
                return false;
            }
        }

        private void LogWriteLine(string fname)
        {
            moduleLog.WriteLine(Locale.IsRussian ?
                $"Файл {fname} Сохранен" :
                $"File {fname} Saved");
        }

        public bool VisibleLog
        {
            get { return farm.VisibleLogPrgFile; }
            set { farm.VisibleLogPrgFile = value; }
        }

        // Проверка значения из шаблона, если не задан, то = 1 минуте
        public int LogPrgTime()
        {
            bool rint = int.TryParse(farm.SaveLogPrgTime, out int result);
            if (rint) return result;
            return 1;
        }

        public int IterationTime()
        {
            bool iTime = int.TryParse(farm.IterationTime, out int result);
            if (iTime) return result;
            return 1000;
        }

        private bool LoadPrg(string fname, AllProgram program)
        {
            try
            {
                loadprg = new Farm.Programs.ProgramX();
                newProgram = new AllProgram();
                loadprg = FileFunc.LoadXml(typeof(Farm.Programs.ProgramX), fname) as Farm.Programs.ProgramX; // При новой загрузке очищается

                newProgram = program;
                newProgram.program = loadprg;
                loadProgram.Add(newProgram); // Добавить считанную программу в список активных программ

                return true;
            }
            catch (FileNotFoundException ex)
            {
                moduleLog.WriteLine(ex.Message);
                return false;
            }
            catch (Exception err)
            {
                moduleLog.WriteLine(err.Message);
                return false;
            }
        }

        #region CheckDevice
        private uint licensel;
        private Dictionary<string, uint> lgroup = new Dictionary<string, uint>();
        private bool CheckDevice(string deviceName)
        {
            if (licensel == 0) return false; // Указать что лицензии закончились и выйти

            if (!lgroup.ContainsKey(deviceName))
            {
                lgroup.Add(deviceName, licensel);
                licensel--;
            }
            return true;
        }
        #endregion CheckDevice

        // Тут запускать потоки программ

        #region GetPrograms
        public List<AllProgram> GetPrograms()
        {
            return loadProgram;
        }
        #endregion GetPrograms

        #region GetDevices
        public List<AllDevice> GetDevices()
        {
            return devices;
        }
        #endregion GetDevices

    }
}
