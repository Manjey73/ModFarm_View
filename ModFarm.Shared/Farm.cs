//using System.Threading.Channels;
using System.Xml;
using System.Xml.Serialization;

namespace Scada.Server.Modules.ModFarm
{
    [Serializable]
    public class Farm
    {
        #region ClassFarm Основная часть класса Farm
        public Farm()
        {
            Chickens = new List<Device>();      // Курица
            Gooses = new List<Device>();        // Гусь
            Turkeys = new List<Device>();       // Индюк
            Quails = new List<Device>();        // Перепелка
            Ducks = new List<Device>();         // Утка
            Ostriches = new List<Device>();     // Страусы
            Cows = new List<Device>();          // Корова
            Sheeps = new List<Device>();        // Овца
            Goats = new List<Device>();         // Козы
            Pigs = new List<Device>();          // Свинья
            Rabbits = new List<Device>();       // Кролик
            Others = new List<Device>();        // Другое
        }

        [XmlAttribute]
        public bool VisibleLogPrgFile { get; set; }

        [XmlAttribute]
        public string SaveLogPrgTime { get; set; }

        [XmlAttribute]
        public string IterationTime { get; set; }

        public List<Device> Chickens { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool ChickensSpecified { get { return Chickens.Count != 0; } }

        public List<Device> Gooses { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool GoosesSpecified { get { return Gooses.Count != 0; } }

        public List<Device> Turkeys { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool TurkeysSpecified { get { return Turkeys.Count != 0; } }

        public List<Device> Quails { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool QuailsSpecified { get { return Quails.Count != 0; } }

        public List<Device> Ducks { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool DucksSpecified { get { return Ducks.Count != 0; } }

        public List<Device> Ostriches { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool OstrichesSpecified { get { return Ostriches.Count != 0; } }

        public List<Device> Cows { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool CowsSpecified { get { return Cows.Count != 0; } }

        public List<Device> Sheeps { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool SheepsSpecified { get { return Sheeps.Count != 0; } }

        public List<Device> Goats { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool GoatsSpecified { get { return Goats.Count != 0; } }

        public List<Device> Pigs { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool PigsSpecified { get { return Pigs.Count != 0; } }

        public List<Device> Rabbits { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool RabbitsSpecified { get { return Rabbits.Count != 0; } }

        public List<Device> Others { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
        [XmlIgnore]
        public bool OthersSpecified { get { return Others.Count != 0; } }

        #endregion ClassFarm

        #region ClassDevice Описание класса Device (Устройств, Сущностей)
        public class Device
        {
            public Device()
            {
            }

            public Device(string DeviceName, string Location, bool Active, string DeviceType) // string Program,
            {
                this.DeviceName = DeviceName;
                this.Location = Location;
                this.Active = Active;
                this.DeviceType = DeviceType;
            }

            [XmlAttribute]
            public string DeviceName { get; set; }

            [XmlAttribute]
            public string Location { get; set; }

            [XmlAttribute]
            public bool Active { get; set; }

            [XmlAttribute]
            public string DeviceType { get; set; }

            public Calendar Calendar;
            public Programs Programs;
        }
        #endregion ClassDevice

        #region ClassPrograms Описание класса Programs для сериализации
        public class Programs
        {
            public Programs()
            {
                Program = new List<ProgramX>();
            }

            [XmlElement]
            public List<ProgramX> Program { get; set; }           
            
            // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
            [XmlIgnore]
            public bool ProgramSpecified { get { return Program.Count != 0; } }

            public class ProgramX
            {
                public ProgramX() { }

                public ProgramX(string Task, string TaskPiority, string TaskCycle, string ProgramType, bool ProgramActive, bool ProgramRestart) 
                {
                    this.Task = Task;
                    this.TaskPriority = TaskPriority;
                    this.TaskCycle = TaskCycle;
                    this.ProgramType = ProgramType;
                    this.ProgramActive = ProgramActive;
                    this.ProgramRestart = ProgramRestart;
                }

                [XmlAttribute]
                public string Task { get; set; }

                [XmlAttribute]
                public string TaskPriority { get; set; }

                [XmlAttribute]
                public string TaskCycle { get; set; }

                [XmlAttribute]
                public string ProgramType { get; set; }

                [XmlAttribute]
                public bool ProgramActive { get; set; }

                [XmlAttribute]
                public bool ProgramRestart { get; set; }

                public List<Input> Inputs;
                public List<Output> Outputs;

                public class Input
                {
                    public Input() { }

                    public Input(string ValName, bool Initialize,  string Value, string CnlNum, string CnlCode) // string ValueType,
                    {
                        this.ValName = ValName;
                        this.Initialize = Initialize;
                        this.Value = Value;
                        this.CnlNum = CnlNum;
                        this.CnlCode = CnlCode;
                    }

                    [XmlAttribute]
                    public string ValName { get; set; }

                    [XmlAttribute]
                    public bool Initialize { get; set; }

                    [XmlAttribute]
                    public string Value { get; set; }

                    [XmlAttribute]
                    public string CnlNum { get; set; }

                    [XmlAttribute]
                    public string CnlCode { get; set; }
                }

                public class Output
                {
                    public Output() { }

                    public Output(string ValName, bool Retain, string Value, string CnlNum, string CnlCode)
                    {
                        this.ValName = ValName;
                        this.Retain = Retain;
                        this.Value = Value;
                        this.CnlNum = CnlNum;
                        this.CnlCode = CnlCode;
                    }

                    [XmlAttribute]
                    public string ValName { get; set; }

                    [XmlAttribute]
                    public bool Retain { get; set; }

                    [XmlAttribute]
                    public string Value { get; set; }

                    [XmlAttribute]
                    public string CnlNum { get; set; }

                    [XmlAttribute]
                    public string CnlCode { get; set; }
                }
            }
        }
        #endregion ClassPrograms

        #region ClassCalendar Описание класса Calendar для сериализации
        public class Calendar
        {
            public Calendar()
            {
                Day = new List<DayX>();
            }

            [XmlElement]
            public List<DayX> Day { get; set; }           // это позволяет сделать сериализацию без создания соответствующих групп если они пустые
            [XmlIgnore]
            public bool DaySpecified { get { return Day.Count != 0; } }

            public class DayX
            {
                public DayX()
                {
                }

                public DayX(int DayNum, string TempA, string TempS, string Humidity, string iTempA, string iTempS, string iHumidity, bool Cooling, string CoolTime, string CoolAmount, string CoolTemp, string CooliTemp)
                {
                    this.DayNum = DayNum;
                    this.TempA = TempA;
                    this.TempS = TempS;
                    this.Humidity = Humidity;
                    this.iTempA = iTempA;
                    this.iTempS = iTempS;
                    this.iHumidity = iHumidity;
                    this.Cooling = Cooling;
                    this.CoolTime = CoolTime;
                    this.CoolAmount = CoolAmount;
                    this.CoolTemp = CoolTemp;
                    this.CooliTemp = CooliTemp;
                }

                [XmlAttribute]
                public int DayNum { get; set; }

                [XmlAttribute]
                public string TempA { get; set; }

                [XmlAttribute]
                public string TempS { get; set; }

                [XmlAttribute]
                public string Humidity { get; set; }

                [XmlAttribute]
                public string iTempA { get; set; }

                [XmlAttribute]
                public string iTempS { get; set; }

                [XmlAttribute]
                public string iHumidity { get; set; }

                [XmlAttribute]
                public bool Cooling { get; set; }

                [XmlAttribute]
                public string CoolTime { get; set; }

                [XmlAttribute]
                public string CoolAmount { get; set; }

                [XmlAttribute]
                public string CoolTemp { get; set; }

                [XmlAttribute]
                public string CooliTemp { get;set; }

                // Игнорировать при записи если строки пустые и bool = false
                [XmlIgnore]
                public bool CoolingSpecified { get { return Cooling == true; } }
                public bool CoolTimeSpecified { get { return CoolTime != ""; } }
                public bool CoolAmountSpecified { get { return CoolAmount != ""; } }
                public bool CoolTempSpecified { get { return CoolAmount != ""; } }
                public bool CooliTempSpecified { get { return CoolAmount != ""; } }
            }
        }
        #endregion ClassCalendar
    }
}