using Scada.Forms;
using Scada.Lang;
using Scada.Server.Modules.ModFarm.View.Forms;
using System.Reflection;

namespace Scada.Server.Modules.ModFarm.View
{
    /// <summary>
    /// Implements the server module user interface.
    /// <para>Реализует пользовательский интерфейс серверного модуля.</para>
    /// </summary>
    internal class ModFarmView : ModuleView
    {

        public static FrmModuleConfig fConfig;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ModFarmView()
        {
            CanShowProperties = true;
            ProductCode = "ModFarm";
            RequireRegistration = true;
        }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public override string Name
        {
            get
            {
                return Locale.IsRussian ?
                    "Модуль управления Фермой" :
                    "Farm Management Module";
            }
        }

        /// <summary>
        /// Gets the module description.
        /// </summary>
        public override string Descr
        {
            get
            {
                return Locale.IsRussian ?
                    "Автор Бурахин Андрей email: aburakhin@bk.ru\n" +
                    "Модуль управляет техническим процессом фермы\n" +
                    "Доступны шаблоны для птиц (куры, утки, гуси, перепела, индюки, страусы)\n" +
                    "Для животных (коровы, свиньи, овцы, козы, кролики)\n" +
                    "Отсутствующие добавляются в папку Другие":
                    "Author Andrey Burakhin email: aburakhin@bk.ru\n" +
                    "The module manages the technical process of the farm\n" +
                    "Bird templates are available(chickens, ducks, geese, quails, turkeys, ostriches)\n" +
                    "For animals (cows, pigs, sheep, goats, rabbits)\n" +
                    "The missing ones are added to the Others folder";
            }
        }

        public static Dictionary<string, string> GetLangTo()
        {
            Dictionary<string, string> lang = new Dictionary<string, string>();
            PropertyInfo[] data = typeof(ModulePhrases).GetProperties();
            lang = data.ToDictionary(x => x.Name, x => x.GetValue(data)?.ToString() ?? "");
            return lang;
        }

        /// <summary>
        /// Loads language dictionaries.
        /// </summary>
        public override void LoadDictionaries()
        {
            FrmModuleConfig.PrevLang.Clear();
            FrmModuleConfig.PrevLang = GetLangTo();

            if (!Locale.LoadDictionaries(AppDirs.LangDir, "ModFarm", out string errMsg))
            {
                ScadaUiUtils.ShowError(errMsg);
            }
            else
            {
                ModulePhrases.Init();
            }
        }

        /// <summary>
        /// Shows a modal dialog box for editing module properties.
        /// </summary>
        public override bool ShowProperties()
        {
            return new FrmModuleConfig(ConfigDataset, AppDirs).ShowDialog() == DialogResult.OK;
        }
    }
}
