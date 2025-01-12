using Scada.Lang;

namespace Scada.Server.Modules.ModFarm.View
{
    internal class ModulePhrases
    {
        // Scada.Server.Modules.ModDbExport.View.Forms.FrmModuleConfig
        public static string AppTitle { get; private set; } = "Farm Management Module";
        public static string MainNode { get; private set; } = "Farm";

        public static string VisibleLogPrgFile { get; private set; } = "VisibleLogPrgFile";
        public static string SaveLogPrgTime { get; private set; } = "SaveLogPrgTime";
        public static string IterationTime { get; private set; } = "IterationTime";


        #region AnimalsBirdNodes
        // Ноды птиц и животных в выпадающем списке
        public static string Chickens { get; private set; } = "Chickens";
        public static string Gooses { get; private set; } = "Gooses";
        public static string Turkeys { get; private set; } = "Turkeys";
        public static string Quails { get; private set; } = "Quails";
        public static string Ducks { get; private set; } = "Ducks";
        public static string Ostriches { get; private set; } = "Ostriches";
        public static string Cows { get; private set; } = "Cows";
        public static string Sheeps { get; private set; } = "Sheeps";
        public static string Goats { get; private set; } = "Goats";
        public static string Pigs { get; private set; } = "Pigs";
        public static string Rabbits { get; private set; } = "Rabbits";
        public static string Others { get; private set; } = "Others";
        #endregion AnimalsBirdNodes

        public static string Calendar { get; private set; } = "Calendar";
        public static string Day { get; private set; } = "Day";
        public static string Device { get; private set; } = "Device";
        public static string Programs { get; private set; } = "Programs";
        public static string Program { get; private set; } = "Program";
        public static string Inputs { get; private set; } = "Inputs";
        public static string Input { get; private set; } = "Input";
        public static string Outputs { get; private set; } = "Outputs";
        public static string Output { get; private set; } = "Output";



        public static string TitleLoadProject { get; private set; } = "Load  project...";
        public static string FilterProject { get; private set; } = "XML (*.xml)|*.xml|All files (*.*)|*.*";

        // Именование параметров (Аттрибутов) 
        public static string DeviceName { get; private set; } = "DeviceName";
        public static string Location { get; private set; } = "Location";
        public static string Active { get; private set; } = "Active";
        public static string DeviceType { get; private set; } = "DeviceType";

        // Именование параметров Программы (Аттрибутов) 
        public static string Task { get; private set; } = "Task";
        public static string ProgramType { get; private set; } = "ProgramType";
        public static string ProgramActive { get; private set; } = "ProgramActive";
        public static string ProgramRestart { get; private set; } = "ProgramRestart";
        public static string TaskPriority { get; private set; } = "TaskPriority";
        public static string TaskCycle { get; private set; } = "TaskCycle";

        public static string ValName { get; private set; } = "ValName";
        public static string Value { get; private set; } = "Value";
        public static string CnlNum { get; private set; } = "ChannelNumber";
        public static string CnlCode { get; private set; } = "TagChannel";
        public static string Initialize { get; private set; } = "Initialization";
        public static string Retain { get; private set; } = "RetainValue";



        // TEST наоборот
        // в xml заменить DayNum на DayNumber и так далее в Календаре или все же наоборот в словарях?
        public static string DayNum { get; private set; } = "DayNumber";
        public static string TempA { get; private set; } = "AirTemp";
        public static string TempS { get; private set; } = "ShellTemp";
        public static string Humidity { get; private set; } = "Humidity";
        public static string iTempA { get; private set; } = "AirTempInsens";
        public static string iTempS { get; private set; } = "InsenOfShellTemp";
        public static string iHumidity { get; private set; } = "HumidityInsens";

        public static string Cooling { get; private set; } = "CoolingActive";
        public static string CoolTime { get; private set; } = "TimeCooling";
        public static string CoolAmount { get; private set; } = "AmountCooling";
        public static string CoolTemp { get; private set; } = "CoolingTemp";
        public static string CooliTemp { get; private set; } = "CoolingTempInsens";

        // Названия столбцов Атрибутов
        public static string dgvAttrName { get; private set; } = "Parameter";
        public static string dgvAttrValue { get; private set; } = "Value";

        // Контекстное меню
        #region ContextMenu Nodes
        public static string AddGroup { get; private set; } = "Add Group";
        public static string AddDevice { get; private set; } = "Add Device";
        public static string AddParameters { get; private set; } = "Add Parameters";
        public static string AddCalendar { get; private set; } = "Add Calendar";
        public static string AddDay { get; private set; } = "Add Day";
        public static string AddPrograms { get; private set; } = "Add Programs";
        public static string AddProgram { get; private set; } = "Add Program";
        public static string AddInputs { get; private set; } = "Add Inputs";
        public static string AddOutputs { get; private set; } = "Add Outputs";
        public static string AddInput { get; private set; } = "Add Input";
        public static string AddOutput { get; private set; } = "Add Output";
        public static string AddComment { get; private set; } = "Add Comment";
        public static string AddText { get; private set; } = "Add Text";
        public static string Delete { get; private set; } = "Delete";
        #endregion ContextMenu Nodes

        public static void Init()
        {
            LocaleDict dict = Locale.GetDictionary("Scada.Server.Modules.ModFarm.View.Forms.FrmModuleConfig");
            AppTitle = dict[nameof(AppTitle)];
            MainNode = dict[nameof(MainNode)];      // Главная Нода
            VisibleLogPrgFile = dict[nameof(VisibleLogPrgFile)];
            SaveLogPrgTime = dict[nameof(SaveLogPrgTime)];
            IterationTime = dict[nameof(IterationTime)];

            Chickens = dict[nameof(Chickens)];      // Куры
            Gooses = dict[nameof(Gooses)];          // Гуси
            Turkeys = dict[nameof(Turkeys)];        // Индюки
            Quails = dict[nameof(Quails)];          // Перепелки
            Ducks = dict[nameof(Ducks)];            // Утки
            Ostriches = dict[nameof(Ostriches)];    // Страусы
            Cows = dict[nameof(Cows)];              // Коровы
            Sheeps = dict[nameof(Sheeps)];          // Овцы
            Goats = dict[nameof(Goats)];            // Козы
            Pigs = dict[nameof(Pigs)];              // Свиньи
            Rabbits = dict[nameof(Rabbits)];        // Кролики
            Others = dict[nameof(Others)];          // Другое

            Calendar = dict[nameof(Calendar)];      // Календарь
            Day = dict[nameof(Day)];                // День

            Device = dict[nameof(Device)];          // Устройство
            Programs = dict[nameof(Programs)];      // Программы
            Program = dict[nameof(Program)];        // Программа

            Inputs = dict[nameof(Inputs)];          // Входы
            Input = dict[nameof(Input)];            // Вход
            Outputs = dict[nameof(Outputs)];        // Выходы
            Output = dict[nameof(Output)];          // Выход

            TitleLoadProject = dict[nameof(TitleLoadProject)];
            FilterProject = dict[nameof(FilterProject)];

            DeviceName = dict[nameof(DeviceName)];
            Location = dict[nameof(Location)];
            Active = dict[nameof(Active)];
            DeviceType = dict[nameof(DeviceType)];

            // Аттрибуты программы
            Task = dict[nameof(Task)];
            ProgramType = dict[nameof(ProgramType)];
            ProgramActive = dict[nameof(ProgramActive)];
            ProgramRestart = dict[nameof(ProgramRestart)];
            TaskPriority = dict[nameof(TaskPriority)];
            TaskCycle = dict[nameof(TaskCycle)];

            // Аттрибуты входов и выходов
            ValName = dict[nameof(ValName)];
            Value = dict[nameof(Value)];
            CnlNum = dict[nameof(CnlNum)];
            CnlCode = dict[nameof(CnlCode)];
            Initialize = dict[nameof(Initialize)];

            // Аттрибуты дней
            DayNum = dict[nameof(DayNum)];
            TempA = dict[nameof(TempA)];
            TempS = dict[nameof(TempS)];
            Humidity = dict[nameof(Humidity)];
            iTempA = dict[nameof(iTempA)];
            iTempS = dict[nameof(iTempS)];
            iHumidity = dict[nameof(iHumidity)];

            Cooling = dict[nameof(Cooling)];
            CoolTime = dict[nameof(CoolTime)];
            CoolAmount = dict[nameof(CoolAmount)];
            CoolTemp = dict[nameof(CoolTemp)];
            CooliTemp = dict[nameof(CooliTemp)];

            // Имя столбцов Атрибутов
            dgvAttrName = dict[nameof(dgvAttrName)];
            dgvAttrValue = dict[nameof(dgvAttrValue)];

            // Сообщения окон об ошибках
            //errAddParameter = dict[nameof(errAddParameter)];
            //errAddText = dict[nameof(errAddText)];

            // Контекстное меню
            AddGroup = dict[nameof(AddGroup)];
            AddDevice = dict[nameof(AddDevice)];
            AddParameters = dict[nameof(AddParameters)];
            AddCalendar = dict[nameof(AddCalendar)];
            AddDay = dict[nameof(AddDay)];
            AddPrograms = dict[nameof(AddPrograms)];
            AddProgram = dict[nameof(AddProgram)];
            AddInputs = dict[nameof(AddInputs)];
            AddOutputs = dict[nameof(AddOutputs)];
            AddInput = dict[nameof(AddInput)];
            AddOutput = dict[nameof(AddOutput)];
            AddComment = dict[nameof(AddComment)];
            AddText = dict[nameof(AddText)];
            Delete = dict[nameof(Delete)];
        }
    }
}
