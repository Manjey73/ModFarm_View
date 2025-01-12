using Scada.Data.Entities;
using Scada.Data.Models;
using Scada.Lang;
using Scada.Log;
using Scada.Server.Lang;
using System.Reflection;

namespace Scada.Server.Modules.ModFarm.Logic
{
    public class ModFarmLogic : ModuleLogic
    {
        private ModLogix Logix;
        private List<ModLogix.AllProgram> LoadProgram = new List<ModLogix.AllProgram>();
        private List<ModLogix.AllDevice> LoadDevice = new List<ModLogix.AllDevice>();
        private class ModThread()
        {
            public Thread thread { get; set; }
            public Dictionary<FieldInfo, object> fieldObj { get; set; }
            public string name { get; set; }
            public object inst { get; set; } // obj
            public Type type { get; set; }
            public ModLogix.AllProgram programm { get; set; }
            public Dictionary<string, Cnl> threadCnl { get; set; }
        }

        private Dictionary<string, ModThread> PrgThread = new Dictionary<string, ModThread>();
        private Dictionary<string, Cnl> dictCnl; // = new Dictionary<int, Cnl>(); // Словарь каналов с привязками к потокам, добавлять ModThread
        private Dictionary<FieldInfo, object> fieldCnl;
        private Dictionary<int, string> CnlFromThread = new Dictionary<int, string>(); // Словарь принадлежности номеров каналов потокам, где значение это имя потока.

        /// <summary>
        /// The module log file name.
        /// </summary>
        private const string LogFileName = "ModFarm.log";

        private readonly ILog moduleLog;            // the module log
        //private readonly ModuleConfig moduleConfig; // the module configuration

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ModFarmLogic(IServerContext serverContext)
            : base(serverContext)
        {
            moduleLog = new LogFile(LogFormat.Simple)
            {
                FileName = Path.Combine(serverContext.AppDirs.LogDir, LogFileName),
                CapacityMB = serverContext.AppConfig.GeneralOptions.MaxLogSize
            };
            //moduleConfig = new ModuleConfig();
        }

        /// <summary>
        /// Gets the module code.
        /// </summary>
        public override string Code => "ModFarm";

        private Func.TON tonIteration = new Func.TON { IN = false}; // 1 секунда по умолчанию
        private Func.TON tonSave = new Func.TON { IN = false}; // 1 минута по умолчанию


        // TEST TEST
        #region TraceObject
        void TraceObjectFields(object obj) // выводит список полей, их имена и значения:
        {
            foreach (FieldInfo mi in obj.GetType().GetFields())
                moduleLog.WriteLine(mi.FieldType.Name + " " + mi.Name + " = " + mi.GetValue(obj) + mi.FieldType.ToString());
        }
        void TraceObjectProperties(object obj) // выводит список свойств объекта:
        {
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
                moduleLog.WriteLine(pi.PropertyType.Name + " " + pi.Name + " = " + pi.GetValue(obj, null));
        }
        void TraceObjectMethods(object obj)
        {
            foreach (MethodInfo mi in obj.GetType().GetMethods())
                moduleLog.WriteLine(mi.Name + " " + mi.IsPublic);

        }
        #endregion TraceObject

        /// <summary>
        /// Performs actions when starting the service.
        /// </summary>
        public override void OnServiceStart()
        {
            // write to log
            moduleLog.WriteBreak();
            moduleLog.WriteAction(ServerPhrases.StartModule, Code, Version);

            Logix = new ModLogix(ServerContext);
            Logix.LoadConfig();
            Logix.ReadDevice();
            LoadProgram = Logix.GetPrograms();
            LoadDevice = Logix.GetDevices();
            tonIteration.PT = (ulong)Logix.IterationTime();
            tonSave.PT = (ulong)Logix.LogPrgTime() * 60000;

            PrgThread.Clear(); // Очистка словаря всех потоков
            foreach (var program in LoadProgram)
            {
                bool enableThread = false;
                int priority = 0;
                bool tp = int.TryParse(program.program.TaskPriority, out priority);

                ushort cycle = 0;
                bool tc = ushort.TryParse(program.program.TaskCycle, out cycle);

                // Здесь запускать потоки программ
                // Получить список требуемых потоков
                string myProg = program.program.Task;

                Type typelist = Assembly.GetExecutingAssembly().GetExportedTypes().Where(x => x.Name == myProg).FirstOrDefault(); // Получение списка только интересующего класса
                Type type = typelist;

                if (typelist == null)
                {
                    typelist = Assembly.Load("ModFarmPlc").GetExportedTypes().Where(x => x.Name == myProg).FirstOrDefault();

                    if (typelist != null)
                    {
                        enableThread = true;
                        type = typelist;

                        moduleLog.WriteAction(Locale.IsRussian ?
                            $"Задача {myProg} загружена из ModFarmPlc.dll" :
                            $"Task {myProg} loaded from ModFarmPlc.dll");
                    }
                    else
                    {
                        moduleLog.WriteError(Locale.IsRussian ?
                            $"Задачи {myProg} не существует в коде" :
                            $"The task {myProg} does not exist in the code");
                    }
                }
                else
                {
                    enableThread = true;
                    type = typelist;
                    moduleLog.WriteAction(Locale.IsRussian ?
                        $"Задача {myProg} загружена из основного кода" :
                        $"The {myProg} task is loaded from the main code");
                }

                if (enableThread)
                {
                    object instance = Activator.CreateInstance(type);

                    FieldInfo[] fi = type.GetFields();

                    // Проверка наличия переменной с типом Days в Устройстве используемой программы задачи CalendarPrg
                    if (myProg == "CalendarPrg")
                    {
                        Farm.Calendar farmdays = null;
                        var farmDev = LoadDevice.ElementAt(program.deviceIndex); // Чтение данных устройства, которому принадлежит программа

                        FieldInfo days = fi.Where(x => x.FieldType.ToString().EndsWith("Farm+Calendar")).FirstOrDefault();
                        if (days != null)
                        {
                            // Добавить в эту переменную класс Days с содержимым всех переменных.
                            farmdays = farmDev.farmdevice.Calendar;
                            days?.SetValue(instance, farmdays); // запись класса Days в программу
                        }
                    }

                    // Получение списка каналов данной программы из параметров INX_IN и OUTX_OUT
                    dictCnl = new Dictionary<string, Cnl>();
                    AddCnlToDict(fi, program);

                    fieldCnl = new Dictionary<FieldInfo, object>();

                    // Тут поиск переменной по имени связывания цикла для управления из программы(web) ???
                    FieldInfo field = type.GetField("cycle");
                    field?.SetValue(instance, cycle);

                    // Переменные надо инициализировать перед стартом потока, если есть Initialize и Retain
                    foreach (var cnl in dictCnl)
                    {
                        double valDouble = 0.0;
                        int typeIdCnl = cnl.Value.CnlTypeID;
                        field = type.GetField(cnl.Key); // Имя ключа это имя переменной
                        object fieldObj = field?.GetValue(instance);

                        try
                        {
                            valDouble = Convert.ToDouble(fieldObj);
                        }
                        catch
                        {
                            if (field.FieldType.ToString() == "System.DateTime")
                            {
                                // Преобразуем время как есть, Преобразование в UTC выполняется самим Сервером (Формулами)
                                DateTime dt = Convert.ToDateTime(fieldObj);
                                valDouble = dt.ToOADate();
                            }
                            //moduleLog.WriteLine($"Тип :  {field.FieldType.ToString()}"); // TEST
                        }

                        #region InputIsInitialize
                        // Поиск переменной в INX и является ли она init ?
                        Farm.Programs.ProgramX.Input prgIn = program.program.Inputs.Where(x => x.ValName == cnl.Key).FirstOrDefault();
                        if (prgIn != null && Logix.Initialize(prgIn)) // Logix.Initialize(prgIn) // prgIn.Initialize
                        {
                            bool parse = double.TryParse(prgIn.Value, out valDouble);
                            // Тут тоже нужны проверки на типы переменных класса TEST TEST TEST 
                            if (parse)
                            {
                                fieldObj = ConvertToType(valDouble, field.FieldType.ToString()); // Конвертирование значения в зависимости от типа поля. // object value = ConvertToType(valDouble, field.FieldType.ToString());
                                field?.SetValue(instance, fieldObj);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(prgIn.Value))
                                {
                                    if (field.FieldType.ToString() == "System.DateTime")
                                    {
                                        fieldObj = DateTime.Parse(prgIn.Value); // Приведение времени к UTC (НАХУЙ)
                                        valDouble = Convert.ToDateTime(fieldObj).ToOADate(); // Преобразуем DateTime в double для инициализации0 в канале
                                    }
                                    else if (field.FieldType.ToString() == "System.Boolean")
                                    {
                                        fieldObj = Convert.ToBoolean(prgIn.Value);
                                        valDouble = Convert.ToDouble(fieldObj); // Преобразуем Bool в double для инициализации0 в канале
                                    }
                                    field?.SetValue(instance, fieldObj);
                                }
                            }
                        }
                        #endregion InputIsInitialize

                        #region OutputIsRetain
                        // Поиск переменной в OUTX и является ли она retain ?
                        Farm.Programs.ProgramX.Output prgOut = program.program.Outputs.Where(x => x.ValName == cnl.Key).FirstOrDefault();
                        if (prgOut != null && Logix.Retain(prgOut))
                        {
                            // Если Retain True то прочитать ее из файла в valDouble
                            bool parse = double.TryParse(prgOut.Value, out valDouble);
                            // Тут тоже нужны проверки на типы переменных класса
                            if (parse)
                            {
                                fieldObj = ConvertToType(valDouble, field.FieldType.ToString()); // Конвертирование значения в зависимости от типа поля. // object value = ConvertToType(valDouble, field.FieldType.ToString());
                                field?.SetValue(instance, fieldObj); // value
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(prgIn.Value))
                                {
                                    if (field.FieldType.ToString() == "System.DateTime")
                                    {
                                        fieldObj = DateTime.Parse(prgIn.Value); // Приведение времени к UTC (НАХУЙ)
                                        valDouble = Convert.ToDateTime(fieldObj).ToOADate(); // Преобразуем DateTime в double для инициализации0 в канале // valDouble = Convert.ToDateTime(value).ToOADate();
                                    }
                                    field?.SetValue(instance, fieldObj);
                                }
                            }
                        }
                        #endregion OutputIsRetain

                        if (cnl.Value.DataTypeID == 0 || cnl.Value.DataTypeID == null) // тип данных канала Double
                        {
                            ServerContext.WriteCurrentData(cnl.Value.CnlNum, new CnlData(valDouble, 1));
                        }
                        else if (cnl.Value.DataTypeID == 1) // тип данных канала Int64
                        {
                            long val = Convert.ToInt64(fieldObj);
                            ServerContext.WriteCurrentData(cnl.Value.CnlNum, new CnlData(BitConverter.Int64BitsToDouble(val), 1));
                        }

                        // Запись всех значение в словарь Field в виде объектов где ключ Field.Name или само поле FieldInfo со всем содержимым
                        if (!fieldCnl.ContainsKey(field))
                        {
                            fieldCnl.Add(field, fieldObj);
                        }
                    }

                    #region Task ????
                    //var ss = delegate ()
                    //{
                    //    object obj = instance;
                    //    object[] parameters = [];

                    //    MethodInfo mi = type.GetMethod("Run");
                    //    mi.Invoke(obj, parameters);
                    //};
                    //Task task = new Task(ss);
                    //task.Start();
                    #endregion Task ????

                    // Создание и запуск потоков программ
                    Thread thr = new Thread(delegate ()
                    {
                        object obj = instance;
                        object[] parameters = [];

                        MethodInfo mi = type.GetMethod("Run");
                        mi.Invoke(obj, parameters);
                    });
                    thr.Name = $"di{program.deviceIndex}pi{program.programIndex}"; // Задание имени потока по deviceIndex и programIndex
                    thr.IsBackground = true;
                    thr.Priority = (ThreadPriority)priority; // Задание приоритета выполнения программы из шаблона
                    thr.Start();

                    // Добавление в словарь потоков
                    ModThread modThread = new ModThread
                    {
                        thread = thr,
                        name = thr.Name,
                        fieldObj = fieldCnl,
                        inst = instance,
                        type = type,
                        programm = program,
                        threadCnl = dictCnl, // Добавления словаря привязанных каналов входных и выходных переменных
                    };
                    PrgThread.Add(thr.Name, modThread);

                    // Тут уже известен поток и его имя. Сделать словарь Номер канала в качестве ключа - имя потока в качестве значения где применяется этот номер ????
                    foreach (var channel in dictCnl)
                    {
                        int cnlNum = channel.Value.CnlNum;
                        if (!CnlFromThread.ContainsKey(cnlNum))
                        {
                            CnlFromThread.Add(cnlNum, thr.Name);
                        }
                    }
                }
            }
        }

        #region AddCnlDict Добавление в словарь каналов программ
        private void AddCnlToDict(FieldInfo[] fi, ModLogix.AllProgram prg)
        {
            foreach (FieldInfo fie in fi)
            {
                var fIn = prg.program.Inputs.Where(ni => ni.ValName == fie.Name).FirstOrDefault();
                if (fIn != null)
                {
                    CnlToDict(fIn.ValName, fIn.CnlNum); // Нужно имя вместо номера
                }

                var fOut = prg.program.Outputs.Where(ni => ni.ValName == fie.Name).FirstOrDefault();
                if (fOut != null)
                {
                    CnlToDict(fOut.ValName, fOut.CnlNum);
                }
            }
        }

        private void CnlToDict(string Name, string CnlNum) // string Name, string CnlNum
        {
            int cnlNum = 0;
            bool cNum = int.TryParse(CnlNum, out cnlNum);

            // Каналы Archive - все Input, InputOutput, Calculated, CalculatedOutput
            try
            {
                Cnl CnlArc = ServerContext.Cnls.ArcCnls.Values.Where(x => x.CnlNum == cnlNum).FirstOrDefault();
                if (CnlArc != null)
                {
                    if (!dictCnl.ContainsKey(Name)) // Сразу добавлять в словарь наверное не получится, так как неизвестен поток ??? Известна программа
                    {
                        dictCnl.Add(Name, CnlArc);
                    }
                }
            }
            catch
            {
                //moduleLog.WriteLine($"НЕТ ArcCnls"); // TEST
            }

            // Каналы Output
            try
            {
                Cnl CnlOut = ServerContext.Cnls.OutCnls.Values.Where(x => x.CnlNum == cnlNum).FirstOrDefault();
                if (CnlOut != null)
                {
                    if (!dictCnl.ContainsKey(Name))
                    {
                        dictCnl.Add(Name, CnlOut);
                    }
                }
            }
            catch
            {
                //moduleLog.WriteLine($"НЕТ OutCnls"); // TEST
            }
        }
        #endregion AddCnlDict

        #region OnServiceStop
        /// <summary>
        /// Performs actions when the service stops.
        /// </summary>
        public override void OnServiceStop()
        {
            // Здесь останавливать потоки программ
            if (PrgThread.Count > 0)
            {
                foreach (var thr in PrgThread)
                {
                    if (thr.Value.thread != null && thr.Value.thread.IsAlive)
                    {
                        // Необходимо считать все переменные из потока в класс программ для сохранения в файл
                        thr.Value.programm = ValueToPrgClass(thr);

                        // Запись в поток переменной terminated = true для остановки
                        FieldInfo field = thr.Value.type.GetField("terminated");
                        field?.SetValue(thr.Value.inst, true);

                        // Сохранить файл с тем же именем
                        string fname = $"{ServerContext.AppDirs.StorageDir}PRG_{thr.Value.programm.group}_di{thr.Value.programm.deviceIndex}_pi{thr.Value.programm.programIndex}.xml"; // Может все же XML через сериализатор, будет разумнее

                        Logix.VisibleLog = true;
                        bool save = Logix.SaveFile(thr.Value.programm, fname);

                        thr.Value.thread.Join(); // Завершение потока
                    }
                }
            }

            // write to log
            moduleLog.WriteAction(ServerPhrases.StopModule, Code);
            moduleLog.WriteBreak();
        }
        #endregion OnServiceStop

        /// <summary>
        /// Performs actions on a new iteration of the main operating loop.
        /// </summary>
        public override void OnIteration()
        {
            // Здесь контролировать потоки и сохранять их данные каждую минуту или требуемое время

            if (!tonIteration.Q)
            {
                tonIteration.Run(true);
            }
            else
            {
                if (PrgThread.Count > 0)
                {
                    foreach (var thr in PrgThread)
                    {
                        if (thr.Value.thread != null && thr.Value.thread.IsAlive) // Проверка что поток не пустой и жив
                        {
                            // Считать все переменные потока по словарям с проверкой Вход/Выход/Расчетные
                            foreach (var cnl in thr.Value.threadCnl)
                            {
                                int typeIdCnl = cnl.Value.CnlTypeID; // Чтение типа канала

                                FieldInfo field = thr.Value.type.GetField(cnl.Key); // Имя ключа это имя переменной
                                object fieldObj = field?.GetValue(thr.Value.inst);

                                //moduleLog.WriteLine($"Тип канала: {typeIdCnl}  Канал {cnl.Value.CnlNum}  {cnl.Value.Name} {cnl.Value.FormatID}");// TEST


                                if (typeIdCnl == 1 || typeIdCnl == 2 || typeIdCnl == 3) // || typeIdCnl == 4) // Каналы  Input, Input/Output, Calculated/Output - Отключаем Calculated/Output
                                {
                                    CnlData curData = ServerContext.GetCurrentData(cnl.Value.CnlNum);
                                    if (curData.IsDefined)
                                    {
                                        // Конвертирование значения в зависимости от типа поля.
                                        object value = ConvertToType(curData.Val, field.FieldType.ToString());

                                        // Проверка на равенство значения в канале и в потоке
                                        if (!Equals(value, thr.Value.fieldObj[field]))
                                        {
                                            field?.SetValue(thr.Value.inst, value);
                                            // Снова считать для подтверждения и записи в словарь
                                            fieldObj = field?.GetValue(thr.Value.inst);
                                            thr.Value.fieldObj[field] = fieldObj;
                                        }
                                    }
                                }

                                if (!Equals(fieldObj, thr.Value.fieldObj[field])) // && !write
                                {
                                    double valDouble = 0.0;
                                    try
                                    {
                                        valDouble = Convert.ToDouble(fieldObj);
                                    }
                                    catch
                                    {
                                        if (field.FieldType.ToString() == "System.DateTime")
                                        {
                                            DateTime dt = Convert.ToDateTime(fieldObj);
                                            valDouble = dt.ToOADate();
                                        }
                                    }

                                    // Запись в канал и потом запись в словарь предыдущих до следующей итерации
                                    if (typeIdCnl == 3)  // Каналы с входом Calculated - только передача значенийв из программы
                                    {
                                        if (cnl.Value.DataTypeID == 0 || cnl.Value.DataTypeID == null) // тип данных канала Double
                                        {
                                            ServerContext.WriteCurrentData(cnl.Value.CnlNum, new CnlData(valDouble, 1));
                                        }
                                        else if (cnl.Value.DataTypeID == 1) // тип данных канала Int64
                                        {
                                            long val = Convert.ToInt64(fieldObj);
                                            ServerContext.WriteCurrentData(cnl.Value.CnlNum, new CnlData(BitConverter.Int64BitsToDouble(val), 1));
                                        }
                                    }

                                    // Каналы с выходом Input/Output
                                    if (typeIdCnl == 2) // || typeIdCnl == 4) // - Отключаем Calculated/Output так как в этот канал отправляет данные Scada
                                    {
                                        if (cnl.Value.DataTypeID == 0 || cnl.Value.DataTypeID == null)
                                        {
                                            ServerContext.SendCommand(new TeleCommand(cnl.Value.CnlNum, valDouble, 11)); // 11 здесь ID админа
                                        }
                                        else if (cnl.Value.DataTypeID == 1) // тип данных канала Int64
                                        {
                                            long val = Convert.ToInt64(fieldObj);
                                            ServerContext.SendCommand(new TeleCommand(cnl.Value.CnlNum, Convert.ToDouble(val), 11)); // 11 здесь ID админа
                                        }
                                    }

                                    // Output - только выходной канал - отправить Telecommand
                                    if (typeIdCnl == 5)
                                    {
                                        ServerContext.SendCommand(new TeleCommand(cnl.Value.CnlNum, valDouble, 11)); // 11 здесь ID админа
                                    }

                                    thr.Value.fieldObj[field] = ConvertToType(valDouble, field.FieldType.ToString());
                                }
                            }
                        }
                        else
                        {
                            moduleLog.WriteError(Locale.IsRussian ?
                                $"Поток задачи: {thr.Value.programm.program.Task} ID потока: {thr.Value.name} закрыт" :
                                $"Thread task: {thr.Value.programm.program.Task} ThreadID: {thr.Value.name} closed");

                            // Вероятно предусмотреть перезапуск потока с сообщением в лог, что он был перезапущен
                        }
                    }
                }
                tonIteration.Run(false);
            }

            if (!tonSave.Q)
            {
                tonSave.Run(true);
            }
            else
            {
                // Тут сохраняем файлы программ для сохранения данных
                if (PrgThread.Count > 0)
                {
                    if (Logix.VisibleLog) // Выводить в лог запись о сохранении файлов
                    {
                        moduleLog.WriteInfo(Locale.IsRussian ?
                            $"Сохранение промежуточных значений программ:" :
                            $"Saving intermediate program values:");
                    }

                    foreach (var thr in PrgThread)
                    {
                        if (thr.Value.thread != null && thr.Value.thread.IsAlive)
                        {
                            thr.Value.programm = ValueToPrgClass(thr);
                            // Сохранить файл с тем же именем
                            string fname = $"{ServerContext.AppDirs.StorageDir}PRG_{thr.Value.programm.group}_di{thr.Value.programm.deviceIndex}_pi{thr.Value.programm.programIndex}.xml"; // Может все же XML через сериализатор, будет разумнее
                            bool save = Logix.SaveFile(thr.Value.programm, fname);
                        }
                        else
                        {
                            moduleLog.WriteError(Locale.IsRussian ?
                                $"Поток задачи: {thr.Value.name} закрыт" :
                                $"Task thread: {thr.Value.name} closed");
                        }
                    }
                }
                tonSave.Run(false); // Перезапуск таймера TON
            }
        }


        /// <summary>
        /// Performs actions after receiving and processing new current data.
        /// </summary>
        /// <remarks>In general, channel numbers are not sorted.</remarks>
        public override void OnCurrentDataProcessed(Slice slice)
        {

        }

        /// <summary>
        /// Performs actions after receiving and processing new historical data.
        /// </summary>
        public override void OnHistoricalDataProcessed(Slice slice)
        {

        }

        /// <summary>
        /// Performs actions after creating and before writing an event.
        /// </summary>
        public override void OnEvent(Event ev)
        {

        }

        /// <summary>
        /// Performs actions when acknowledging an event.
        /// </summary>
        public override void OnEventAck(EventAck eventAck)
        {

        }

        /// <summary>
        /// Performs actions after receiving and before enqueuing a telecontrol command.
        /// </summary>
        public override void OnCommand(TeleCommand command, CommandResult commandResult)
        {
            if (CnlFromThread.ContainsKey(command.CnlNum))
            {
                ModThread modThread = PrgThread[CnlFromThread[command.CnlNum]]; // Выбор потока по имени из словаря потоков

                // Если поток не пустой и жив
                if (modThread.thread != null && modThread.thread.IsAlive)
                {
                    object instance = modThread.inst;
                    Dictionary<string, Cnl> cnlDict = modThread.threadCnl;

                    // KeyCnl.Key - Имя переменной с номером канала, KeyCnl.Value все данные Cnl по каналу
                    KeyValuePair<string, Cnl> KeyCnl = cnlDict.Where(x => x.Value.CnlNum == command.CnlNum).FirstOrDefault();

                    string devName = "";
                    try
                    {
                        devName = ServerContext.ConfigDatabase.DeviceTable.Where(x => x.DeviceNum == command.DeviceNum).FirstOrDefault().Name;
                    }
                    catch { }

                    string objName = "";
                    try
                    {
                        objName = ServerContext.ConfigDatabase.ObjTable.Where(x => x.ObjNum == command.ObjNum).FirstOrDefault().Name;
                    }
                    catch { }

                    moduleLog.WriteAction(Locale.IsRussian ?
                        $"Отправка команды: TaskID: {CnlFromThread[command.CnlNum]}, Канал:[{command.CnlNum}] {KeyCnl.Value.Name}, " +
                        $"Устройство:[{command.DeviceNum}] {devName}, Объект:[{command.ObjNum}] {objName}, Значение: {command.CmdVal}" :
                        $"Sending a command: TaskID: {CnlFromThread[command.CnlNum]}, Channel:[{command.CnlNum}] {KeyCnl.Value.Name}, " +
                        $"Device:[{command.DeviceNum}] {devName}, Object:[{command.ObjNum}] {objName}, Value: {command.CmdVal}");

                    if (CnlTypeIdToString(KeyCnl.Value.CnlTypeID) == "CalculatedOutput")
                    {
                        FieldInfo field = modThread.type.GetField(KeyCnl.Key); // Имя ключа это имя переменной

                        object value = ConvertToType(command.CmdVal, field.FieldType.ToString());
                        field?.SetValue(instance, value);

                        //moduleLog.WriteLine($"Cnl {KeyCnl.Key} {field.FieldType.ToString()} {field?.GetValue(instance)}"); // TEST

                        // Записать в словарь измененное значение
                        modThread.fieldObj[field] = value;
                    }
                    if (CnlTypeIdToString(KeyCnl.Value.CnlTypeID) == "InputOutput" || CnlTypeIdToString(KeyCnl.Value.CnlTypeID) == "Input")
                    {
                        // Входная часть должна попадать как команда или это в рамках Iteration ?

                    }
                }
            }
        }

        #region ConvertToType Конвертирование переменной в требуемый тип объекта
        private object ConvertToType(double valDouble, string valueType)
        {
            object result = valDouble;
            if (valueType == "System.Single")
            {
                result = Convert.ToSingle(valDouble);
            }
            else if (valueType == "System.Int16")
            {
                result = Convert.ToInt16(valDouble);
            }
            else if (valueType == "System.UInt16")
            {
                result = Convert.ToUInt16(valDouble);
            }
            else if (valueType == "System.Int32")
            {
                result = Convert.ToInt32(valDouble);
            }
            else if (valueType == "System.UInt32")
            {
                result = Convert.ToUInt32(valDouble);
            }
            else if (valueType == "System.Boolean")
            {
                result = Convert.ToBoolean(valDouble);
            }
            else if (valueType == "System.Int64")
            {
                result = Convert.ToInt64(valDouble);
            }
            else if (valueType == "System.UInt64")
            {
                result = Convert.ToUInt64(valDouble);
            }
            else if (valueType == "System.Byte")
            {
                result = Convert.ToByte(valDouble);
            }
            else if (valueType == "System.SByte")
            {
                result = Convert.ToSByte(valDouble);
            }
            else if (valueType == "System.DateTime") // Не знаю зачем ? по идее нужен DateTime а не double
            {
                //result = DateTime.FromOADate(valDouble);
                result = Convert.ToDateTime(DateTime.FromOADate(valDouble));
            }
            return result;
        }
        #endregion ConvertToType 

        #region ValueToProgramm
        private ModLogix.AllProgram ValueToPrgClass(KeyValuePair<string, ModThread> thr)
        {
            FieldInfo[] values = thr.Value.type.GetFields();
            if (thr.Value.programm.program.Inputs.Count() > 0)
            {
                foreach (var cin in thr.Value.programm.program.Inputs)
                {
                    FieldInfo fi = values.Where(v => v.Name == cin.ValName).FirstOrDefault();
                    if (fi != null)
                        cin.Value = fi.GetValue(thr.Value.inst).ToString();
                }
            }

            if (thr.Value.programm.program.Outputs.Count() > 0)
            {
                foreach (var cout in thr.Value.programm.program.Outputs)
                {
                    FieldInfo fi = values.Where(v => v.Name == cout.ValName).FirstOrDefault();
                    if (fi != null)
                        cout.Value = fi.GetValue(thr.Value.inst).ToString();
                }
            }
            return thr.Value.programm;
        }
        #endregion ValueToProgramm

        #region CnlTypeIdToString
        public static string CnlTypeIdToString(int tipeid)
        {
            string idtype = "";
            switch (tipeid)
            {
                case 1: idtype = "Input"; break;
                case 2: idtype = "InputOutput"; break;
                case 3: idtype = "Calculated"; break;
                case 4: idtype = "CalculatedOutput"; break;
                case 5: idtype = "Output"; break;
            }
            return idtype;
        }
        #endregion TypeIdToString

    }
}
