

namespace Scada.Server.Modules.ModFarm
{
    // Программы для выполнения в потоках

    #region TestAttribute
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ProgramzAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public ProgramzAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
    #endregion TestAttribute

    // Программа ротации кондиционеров
    #region Rotation
    public class Rotation()
    {
        public bool EN; // входная переменная разрешающая работу программы
        public int Quantity; // количество кондиционеров всего
        public int InReserve; // Количество в резерве
        public int SwitchingTime; // Время переключения в часах



        public TimeSpan timeSpan1, timeSpan2, timeSpan3, timeSpan4, timeSpan5, timeSpan6, timeSpan7, timeSpan8; // Время наработки кондиционеров, должны быть retain

        public ushort cycle; // Задание цикла выполнения программы в мс
        public bool terminated = false; // Переменная остановки цикла для закрытия потока

        public void Run()
        {
            while (!terminated)
            {
                if (EN)
                {





                    // Задание времени цикла
                    Thread.Sleep(cycle);
                }
            }
        }
    }
    #endregion Rotation

    // Программа, работающая на основе класса Days для Инкубаторов и выводных шкафов
    #region CalendarPrg
    public class CalendarPrg()
    {
        [Programz("Input", "Allowing the program to work")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public bool EN; // входная переменная разрешающая работу программы
        //[MyCustom("Input", "The date of the zero day")]
        public DateTime startDt; // Входная переменная даты 0 дня
        public Farm.Calendar calendar; // Как передать сюда класс Days ? // TEST TEST TEST
        public double tempA, tempS; // Выход температуры для подачи команды в устройство (канал)

        //[MyCustom("Input", "Humidity parameter input")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public double humidity; // Выход влажности для подачи команды в устройство (канал)
        public int dayNum; // Текущий день

        public ushort cycle; // Задание цикла выполнения программы в мс
        public bool terminated = false; // Переменная остановки цикла для закрытия потока

        // --Переменные для Гистерезиса нагревателя ---
        public double HysInCnl, HysLow, HysHigh;
        public bool HysHQ, HysCQ; // HysHQ - выход нагревателя, HysCQ - выход охладителя
        // --------------------------------------------

        // Test переменные
        public double CURRT, STARTT, TOCURR, TOSTART;
        // Test переменные

        // Внутренние переменные для работы программы
        DateTime currDt; // Текущая дата
        double _tempA, _tempS, _ctempA;
        double _humidity;
        double iTA, iTS, ciTA;
        bool Heat, Cool;

        Func func = new Func(); // Подтягивание функций для работы программ

        Hysteresis Hys = new Hysteresis { EN = true, cycle = 0, terminated = true, mode = true }; // Настройки в режиме нагревателя - Проверка класса Hysteresis c циклом do/while

        public void Run()
        {
            List<Farm.Calendar.DayX> listDay = calendar.Day;
            int _dayNum = 0;
            int indexDay = 0;
            int minute = 20;
            int amount = 2;
            TimeOnly toStart;
            TimeOnly toCurr;
            HysHQ = false;
            HysCQ = false;

            while (!terminated)
            {
                if (EN)
                {
                    // Копирование входных переменных во внутренние в начале цикла программы
                    currDt = DateTime.UtcNow; // Работает
                    toCurr = TimeOnly.FromDateTime(currDt);
                    toStart = TimeOnly.FromDateTime(startDt);

                    // Инициализация выходных переменных если требуется.
                    _tempA = tempA; // для возможности задания retain

                    // тело программы
                    if (currDt >= startDt)
                    {
                        // Как тут взять температуру из массива Days на требуемый день из xml?
                        TimeSpan interval = currDt - startDt;
                        _dayNum = interval.Days;

                        if (_dayNum >= 0)  // можно же >= 0 и далее проверку на > listDay.Count
                        {
                            indexDay = _dayNum;
                            if (_dayNum > listDay.Count - 1) indexDay = listDay.Count - 1;

                            _tempA = func.SToDouble(listDay[indexDay].TempA);    // Уставка температуры воздуха
                            _humidity = func.SToDouble(listDay[indexDay].Humidity); // Уставка влажности

                            iTA = func.SToDouble(listDay[indexDay].iTempA); // Допуск температур воздуха
                            iTS = func.SToDouble(listDay[indexDay].iTempS); // Допуск температур скорлупы

                            Heat = true;
                            Cool = false;
                            Hys.mode = true;    // Режим нагрева
                            Hys.inCnl = HysInCnl;
                            Hys.low = _tempA - iTA;
                            Hys.high = _tempA + iTA;


                            if (listDay[indexDay].Cooling) // Проверяем что при наступлении новых суток Cooling = true - Требуется охлаждение
                            {
                                // Количество раз в сутки режима охлаждения
                                int.TryParse(listDay[indexDay].CoolAmount, out amount);

                                int period = 24 / amount;
                                int.TryParse(listDay[indexDay].CoolTime, out minute); // Получение минут охлаждения из конфига

                                double totalminute = (toCurr - toStart).TotalMinutes;
                                int col = (int)Math.Round(totalminute / (period * 60));

                                if (toStart.AddMinutes(col * period * 60) < toCurr && toCurr < toStart.AddMinutes(minute).AddMinutes(col * period * 60))
                                {
                                    _ctempA = func.SToDouble(listDay[indexDay].CoolTemp);   // Уставка температуры воздуха олаждения
                                    ciTA = func.SToDouble(listDay[indexDay].CooliTemp);     // Допуск температур воздуха охлаждения

                                    Hys.mode = false; // Переключить в режим охлаждения
                                    Hys.low = _ctempA - ciTA;
                                    Hys.high = _ctempA + ciTA;

                                    Heat = false;
                                    Cool = true;
                                }
                                else
                                {

                                    //toNext = toStart.AddMinutes(Math.Ceiling(th / (period * 60)) * period * 60);
                                }
                            }
                        }

                        // Работа нагревателя или охладителя
                        // Если нагреватель не отключен, то выход управляет нагревателем
                        if (Heat)
                        {
                            Hys.Run();
                            HysHQ = Hys.Q;
                        }
                        else HysHQ = false; // Если нагреватель отключен, выход выключен

                        if (Cool)
                        {
                            Hys.Run();
                            HysCQ = Hys.Q;
                        }
                        else HysCQ = false; // Если охладитель отключен, выход выключен
                    }

                    // тело программы

                    // Копирование выходных переменных из внутренних в конце цикла программы, для некоторых необязательно, будут обработаны программой
                    tempA = _tempA;
                    humidity = _humidity;
                    dayNum = _dayNum;
                    HysLow = Hys.low;   // Передача в выходные переменные
                    HysHigh = Hys.high; // Передача в выходные переменные


                    CURRT = currDt.ToOADate();
                    STARTT = startDt.ToOADate();


                    var referenceDate = new DateTime(2024, 9, 11);
                    referenceDate += toCurr.ToTimeSpan();
                    TOCURR = referenceDate.ToOADate();

                    referenceDate = new DateTime(startDt.Year, startDt.Month, startDt.Day, 0, 0, 0);
                    referenceDate += toStart.ToTimeSpan();
                    TOSTART = referenceDate.ToOADate();

                    // Задание времени цикла
                    Thread.Sleep(cycle);
                }
            }
        }
    }
    #endregion CalendarPrg

    #region Hysteresis Нагрев/Охлаждение do/while

    //[MyCustom("Hysteresis", "Hysteresis for heater and cooler\n" +
    //    "EN - control input")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
    public class Hysteresis()
    {
        [Programz("Input", "Allowing the program to work")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public bool EN; // входная переменная разрешающая работу программы
        //[MyCustom("Input", "Incoming measurement channel")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public double inCnl;
        //[MyCustom("Input", "Minimum value")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public double low;
        //[MyCustom("Input", "Maximum value")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public double high;

        //[MyCustom("Input", "Mode = Heater, false = Cooler")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public bool mode = true; // По умолчанию нагрев = true, охлаждение = false

        //[MyCustom("Output", "Hysteresis output")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public bool Q; // Выход гистерезиса

        //[MyCustom("Input", "Task cycle in ms")] // TEST свои атрибуты для переменных - Как их переводить в словарях ?
        public ushort cycle; // Задание цикла выполнения программы в мс
        public bool terminated = false; // Переменная остановки цикла для остановки программы и(или) для закрытия потока

        bool _mode;

        public void Run()
        {
            // Копирование входных переменных во внутренние при программы
            bool res = false;

            do
            {
                if (EN)
                {
                    // Копирование входных переменных во внутренние в начале цикла программы
                    _mode = mode;
                    // Инициализация выходных переменных если требуется.
                    res = Q; // для возможности задания retain при перезапусках
                    // тело программы
                    if (_mode)
                    {
                        if (inCnl < low) res = true;
                        if (inCnl > high) res = false;
                    }
                    else
                    {
                        if (inCnl < low) res = false;
                        if (inCnl > high) res = true;
                    }
                    // тело программы
                    // Копирование выходных переменных из внутренних в конце цикла программы, для некоторых необязательно, будут обработаны программой
                    Q = res;
                    // Задание времени цикла
                    Thread.Sleep(cycle);
                }
            }
            while (!terminated);
        }
    }
    #endregion Hysteresis Нагрев/Охлаждение do/while


    public class Func()
    {
        // Преобразование строки в числе double с проверкой Культуры.
        public double SToDouble(string s)
        {
            double result = 1;
            if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out result))
            {
                if (!double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out result))
                {
                    result = double.NaN; // было return 1;
                }
            }
            return result;
        }

        #region TimerTon
        // Таймеры МЭК и другие
        public static long Ticks()
        {
            DateTime now = DateTime.Now;
            long time = now.Ticks / 10000;
            return time;
        }

        public class TON
        {
            public TON()
            {
            }

            public TON(bool IN, ulong PT, ulong ET, bool Q)
            {
                this.IN = IN;
                this.PT = PT;
                this.ET = ET;
                this.Q = Q;
            }

            public bool IN; // входная переменная
            public ulong PT; // входная переменная
            public ulong ET; // выходная переменная - текущее значение таймера
            public bool Q; // выходная переменная
            bool _M; // внутренний флаг
            ulong _StartTime;

            public bool Run(bool IN)
            {
                if (!IN)
                {
                    Q = false;
                    ET = 0;
                    _M = false;
                }
                else
                {
                    if (!_M)
                    {
                        _M = true; // взводим флаг М
                        _StartTime = (ulong)Ticks();
                        // ET = 0; // сразу = 0
                    }
                    else
                    {
                        if (!Q)
                            ET = (ulong)Ticks() - _StartTime; // вычисляем время
                    }
                    if (ET >= PT)
                        Q = true;
                }
                return Q;
            }
        }
        #endregion TimerTon

    }
}
